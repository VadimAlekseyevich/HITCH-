using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 single-cable action prototype.
///
/// RMB attaches/replaces a finite cable. LMB starts automatic reel-in.
/// The rope remains swingable under gravity before and during reel-in.
/// </summary>
public static class WinchSystem
{
    private const float TinyDistanceSquared = 1e-10f;

    public static WinchStepResult Step(
        in PlayerState player,
        in WinchState previousWinch,
        in PlayerInput input,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world,
        float fixedDeltaSeconds,
        bool applyGravity = true,
        float motorScale = 1f)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(locomotionConfig);
        ArgumentNullException.ThrowIfNull(world);

        if (!float.IsFinite(motorScale)
            || motorScale <= 0f
            || motorScale > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(motorScale),
                motorScale,
                "Motor scale must be finite and in (0, 1].");
        }

        var winch = previousWinch;
        var updatedPlayer = player;

        if (input.Has(PlayerButtons.GrappleDetachPressed)
            && previousWinch.HasTarget)
        {
            // Space is the only explicit detach. Preserve full flight velocity.
            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial,
                false);
        }

        if (input.Has(PlayerButtons.GrappleShootPressed))
        {
            // RMB only attaches/replaces the rope. A miss leaves an existing cable alone;
            // Space is the deliberate detach control.
            if (TryShootCable(
                    player,
                    config,
                    locomotionConfig,
                    world,
                    out var shot))
            {
                winch = shot;
            }
        }

        if (input.Has(PlayerButtons.GrappleReelPressed)
            && winch.HasTarget
            && !winch.IsLatched
            && !winch.IsPulling)
        {
            // LMB starts automatic reel-in once. Repeated clicks while reeling do not
            // restart the launch envelope.
            winch = winch with
            {
                IsPulling = true,
                PullElapsedSeconds = 0f,
            };
        }

        if (!winch.HasTarget)
        {
            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial,
                false);
        }

        if (winch.IsLatched)
        {
            return new WinchStepResult(
                updatedPlayer with
                {
                    Velocity = Vector3.Zero,
                    IsGrounded = false,
                },
                winch,
                false);
        }

        var previousPath = winch.Path;
        var updatedPath = UpdateRopePath(
            updatedPlayer.Position,
            previousPath,
            config,
            world);
        var pathChanged = updatedPath != previousPath;

        winch = winch with
        {
            Path = updatedPath,
        };

        var toPullPoint =
            winch.Path.CurrentPullPoint - updatedPlayer.Position;
        var pullPointDistanceSquared = toPullPoint.LengthSquared();
        var pullPointDistance =
            pullPointDistanceSquared <= TinyDistanceSquared
                ? 0f
                : MathF.Sqrt(pullPointDistanceSquared);
        var direction =
            pullPointDistance > 0f
                ? toPullPoint / pullPointDistance
                : Vector3.Zero;

        var pathLength = ComputeRopePathLength(
            updatedPlayer.Position,
            winch.Path);

        var ropeLength = winch.RopeLength > 0f
            ? winch.RopeLength
            : MathF.Min(pathLength, config.MaxRopeLength);

        // A new geometric bend can make the polyline longer without the player moving.
        // Pay out only that geometric increase (up to the global rope limit) so adding a
        // corner cannot become an artificial catapult.
        if (pathChanged && pathLength > ropeLength)
        {
            ropeLength = MathF.Min(
                pathLength,
                config.MaxRopeLength);
        }

        if (winch.IsPulling
            && HasReachedAnchor(
                updatedPlayer,
                winch,
                config,
                locomotionConfig))
        {
            updatedPlayer = updatedPlayer with
            {
                Velocity = Vector3.Zero,
                IsGrounded = false,
            };

            return new WinchStepResult(
                updatedPlayer,
                winch with
                {
                    IsPulling = false,
                    IsArrivedLatched = true,
                    RopeLength = ropeLength,
                    LastActualDistance = pathLength,
                    LastPullAcceleration = 0f,
                },
                true);
        }

        var reelSpeed = 0f;
        if (winch.IsPulling)
        {
            reelSpeed = ComputeDirectPullSpeed(
                ropeLength,
                winch.PullElapsedSeconds,
                config);

            var fixedTailLength =
                MathF.Max(0f, pathLength - pullPointDistance);
            var minimumRopeLength = fixedTailLength;

            ropeLength = MathF.Max(
                minimumRopeLength,
                ropeLength - (reelSpeed * fixedDeltaSeconds));
        }

        var gravityVelocity =
            applyGravity
                ? updatedPlayer.Velocity
                  + new Vector3(
                      0f,
                      -locomotionConfig.Gravity * fixedDeltaSeconds,
                      0f)
                : updatedPlayer.Velocity;

        var velocity = gravityVelocity;
        var appliedRadialAcceleration = 0f;

        if (pullPointDistance > 0f)
        {
            var radialSpeed =
                Vector3.Dot(velocity, direction);
            var tangentialVelocity =
                velocity - (direction * radialSpeed);

            var excessLength =
                MathF.Max(0f, pathLength - ropeLength);
            var ropeTaut =
                pathLength >= ropeLength - config.RopeTautTolerance;

            if (ropeTaut)
            {
                var requiredInwardSpeed =
                    winch.IsPulling
                        ? reelSpeed
                        : 0f;

                if (excessLength > 0f)
                {
                    requiredInwardSpeed = MathF.Max(
                        requiredInwardSpeed,
                        MathF.Min(
                            excessLength / fixedDeltaSeconds,
                            config.RopeConstraintCorrectionSpeed));
                }

                if (radialSpeed < requiredInwardSpeed)
                {
                    var neededDelta =
                        requiredInwardSpeed - radialSpeed;

                    // Crucial for bends/walls: never rotate the full velocity vector in one
                    // tick. Radial tension ramps at a bounded acceleration.
                    var addedRadialSpeed = MathF.Min(
                        neededDelta,
                        config.PullRadialAcceleration
                        * motorScale
                        * fixedDeltaSeconds);

                    radialSpeed += addedRadialSpeed;
                    appliedRadialAcceleration =
                        addedRadialSpeed / fixedDeltaSeconds;
                }
            }

            velocity =
                tangentialVelocity
                + (direction * radialSpeed);

            if (winch.IsPulling)
            {
                var gasDirection = ViewForward(
                    updatedPlayer.ViewYawRadians,
                    updatedPlayer.ViewPitchRadians);

                // ODM-like gas propulsion assists motion around the cable instead of duplicating
                // the winch's radial job. Project the thrust onto the cable tangent plane.
                gasDirection -=
                    direction
                    * Vector3.Dot(gasDirection, direction);

                if (gasDirection.LengthSquared()
                    <= TinyDistanceSquared
                    && tangentialVelocity.LengthSquared()
                    > TinyDistanceSquared)
                {
                    gasDirection =
                        Vector3.Normalize(tangentialVelocity);
                }
                else if (gasDirection.LengthSquared()
                         > TinyDistanceSquared)
                {
                    gasDirection =
                        Vector3.Normalize(gasDirection);
                }

                if (gasDirection.LengthSquared()
                    > TinyDistanceSquared)
                {
                    var gasAuthority =
                        ComputeGasAuthority(
                            velocity.Length(),
                            config);

                    velocity +=
                        gasDirection
                        * config.GasAcceleration
                        * motorScale
                        * gasAuthority
                        * fixedDeltaSeconds;
                }
            }
        }

        updatedPlayer = updatedPlayer with
        {
            Velocity = velocity,
            IsGrounded = false,
        };

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                RopeLength = ropeLength,
                LastActualDistance = pathLength,
                LastPullAcceleration = appliedRadialAcceleration,
                PullElapsedSeconds =
                    winch.IsPulling
                        ? winch.PullElapsedSeconds + fixedDeltaSeconds
                        : 0f,
            },
            false);
    }

    public static float ComputeGasAuthority(
        float playerSpeed,
        WinchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!float.IsFinite(playerSpeed) || playerSpeed <= 0f)
        {
            return 1f;
        }

        if (playerSpeed <= config.GasFullAccelerationSpeed)
        {
            return 1f;
        }

        if (playerSpeed >= config.GasCutoffSpeed)
        {
            return 0f;
        }

        var t = Math.Clamp(
            (playerSpeed - config.GasFullAccelerationSpeed)
            / (config.GasCutoffSpeed
               - config.GasFullAccelerationSpeed),
            0f,
            1f);

        var smooth =
            t * t * (3f - (2f * t));

        return 1f - smooth;
    }

    public static float ComputeDirectPullSpeed(
        float distance,
        WinchConfig config) =>
        ComputeDistancePullSpeed(distance, config);

    public static float ComputeDirectPullSpeed(
        float distance,
        float pullElapsedSeconds,
        WinchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var distanceSpeed =
            ComputeDistancePullSpeed(distance, config);

        if (distanceSpeed <= 0f)
        {
            return 0f;
        }

        var age = MathF.Max(0f, pullElapsedSeconds);
        float launchMultiplier;

        if (age <= config.PullLaunchPeakSeconds)
        {
            // Stage 1: the cable already launches hard on frame zero, then surges into a
            // noticeably stronger peak a fraction of a second later.
            var riseT = Math.Clamp(
                age / config.PullLaunchPeakSeconds,
                0f,
                1f);
            var riseBlend =
                riseT
                * riseT
                * (3f - (2f * riseT));

            launchMultiplier =
                config.PullLaunchInitialMultiplier
                + ((config.PullLaunchPeakMultiplier
                    - config.PullLaunchInitialMultiplier)
                   * riseBlend);
        }
        else
        {
            // Stage 2: after the peak, ease naturally back to sustained pull speed.
            var decayAge =
                age - config.PullLaunchPeakSeconds;
            var decayT = Math.Clamp(
                decayAge / config.PullLaunchDecaySeconds,
                0f,
                1f);
            var decayBlend =
                decayT
                * decayT
                * (3f - (2f * decayT));

            launchMultiplier =
                config.PullLaunchPeakMultiplier
                + ((1f - config.PullLaunchPeakMultiplier)
                   * decayBlend);
        }

        return distanceSpeed * launchMultiplier;
    }

    private static float ComputeDistancePullSpeed(
        float distance,
        WinchConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!float.IsFinite(distance) || distance <= 0f)
        {
            return 0f;
        }

        var normalizedDistance = Math.Clamp(
            distance / config.PullLongRangeDistance,
            0f,
            1f);

        // Smoothstep avoids a visible speed discontinuity while still letting long lines
        // become dramatically faster.
        var speedBlend =
            normalizedDistance
            * normalizedDistance
            * (3f - (2f * normalizedDistance));

        return config.PullTargetInwardSpeed
            + ((config.PullLongRangeInwardSpeed
                - config.PullTargetInwardSpeed)
               * speedBlend);
    }

    public static WinchPathState UpdateRopePath(
        Vector3 playerPosition,
        WinchPathState path,
        WinchConfig config,
        IWorldQuery world)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(world);

        // Unwrap as soon as the player regains a clear segment to the point behind the
        // current bend. Multiple now-redundant bends can disappear in the same tick.
        while (path.ContactCount > 0
               && HasClearRopeSegment(
                   playerPosition,
                   path.PointBehindCurrent,
                   config,
                   world))
        {
            path = path.PopContact();
        }

        var currentTarget = path.CurrentPullPoint;
        var segment = currentTarget - playerPosition;
        if (segment.LengthSquared() <= TinyDistanceSquared)
        {
            return path;
        }

        var query = new RayQuery(
            playerPosition,
            currentTarget,
            config.GrappleCollisionMask);

        if (!world.TryRaycast(query, out var hit))
        {
            return path;
        }

        // Hitting the requested endpoint is expected for the anchor or an existing contact
        // resting on geometry. That is not a new wrap.
        if (Vector3.Distance(
                hit.Position,
                currentTarget)
            <= config.RopeEndpointTolerance)
        {
            return path;
        }

        var contact = FindWrapContact(
            playerPosition,
            currentTarget,
            hit,
            config,
            world);

        if (Vector3.Distance(
                contact,
                currentTarget)
            < config.RopeMinimumContactSpacing)
        {
            return path;
        }

        if (path.ContactCount > 0
            && Vector3.Distance(
                contact,
                path.CurrentPullPoint)
               < config.RopeMinimumContactSpacing)
        {
            return path;
        }

        return path.PushContact(contact);
    }

    private static Vector3 FindWrapContact(
        Vector3 playerPosition,
        Vector3 currentTarget,
        WorldHit hit,
        WinchConfig config,
        IWorldQuery world)
    {
        var normal = hit.Normal.LengthSquared() > TinyDistanceSquared
            ? Vector3.Normalize(hit.Normal)
            : Vector3.Zero;
        var baseContact =
            hit.Position + (normal * config.RopeContactSurfaceOffset);

        if (HasClearRopeSegment(
                playerPosition,
                baseContact,
                config,
                world)
            && HasClearRopeSegment(
                baseContact,
                currentTarget,
                config,
                world))
        {
            return baseContact;
        }

        // Move along the contacted surface toward the old target. For box-like city geometry
        // this converges on the silhouette edge/corner where both straight rope pieces clear.
        var towardTarget =
            currentTarget - hit.Position;
        var tangent =
            towardTarget
            - (normal * Vector3.Dot(towardTarget, normal));

        if (tangent.LengthSquared() <= TinyDistanceSquared)
        {
            return baseContact;
        }

        tangent = Vector3.Normalize(tangent);

        var maximum =
            config.RopeContactEdgeSearchDistance;
        var low = 0f;
        var high = MathF.Min(0.25f, maximum);
        var foundClearCandidate = false;

        while (high <= maximum)
        {
            var candidate =
                baseContact + (tangent * high);

            if (HasClearRopeSegment(
                    playerPosition,
                    candidate,
                    config,
                    world)
                && HasClearRopeSegment(
                    candidate,
                    currentTarget,
                    config,
                    world))
            {
                foundClearCandidate = true;
                break;
            }

            if (high >= maximum)
            {
                break;
            }

            low = high;
            high = MathF.Min(
                high * 2f,
                maximum);
        }

        if (!foundClearCandidate)
        {
            return baseContact;
        }

        // Refine back toward the surface edge instead of leaving a coarse bend floating
        // several meters away from the actual corner.
        for (var i = 0; i < 6; i++)
        {
            var middle = (low + high) * 0.5f;
            var candidate =
                baseContact + (tangent * middle);

            if (HasClearRopeSegment(
                    playerPosition,
                    candidate,
                    config,
                    world)
                && HasClearRopeSegment(
                    candidate,
                    currentTarget,
                    config,
                    world))
            {
                high = middle;
            }
            else
            {
                low = middle;
            }
        }

        return baseContact + (tangent * high);
    }

    public static float ComputeRopePathLength(
        Vector3 playerPosition,
        WinchPathState path)
    {
        var length = 0f;
        var from = playerPosition;

        for (var i = 0; i <= path.ContactCount; i++)
        {
            var to = path.GetPathPointFromPlayer(i);
            length += Vector3.Distance(from, to);
            from = to;
        }

        return length;
    }

    private static bool HasClearRopeSegment(
        Vector3 from,
        Vector3 to,
        WinchConfig config,
        IWorldQuery world)
    {
        if (Vector3.DistanceSquared(from, to)
            <= TinyDistanceSquared)
        {
            return true;
        }

        var query = new RayQuery(
            from,
            to,
            config.GrappleCollisionMask);

        if (!world.TryRaycast(query, out var hit))
        {
            return true;
        }

        return Vector3.Distance(
                   hit.Position,
                   to)
               <= config.RopeEndpointTolerance;
    }

    public static RayQuery BuildAimRay(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        var eye = player.Position
            + (Vector3.UnitY
               * locomotionConfig.EyeOffsetFromCapsuleCenter);
        var direction = ViewForward(
            player.ViewYawRadians,
            player.ViewPitchRadians);

        return new RayQuery(
            eye,
            eye + (direction * config.MaxRopeLength),
            config.GrappleCollisionMask);
    }

    public static bool HasReachedAnchor(
        in PlayerState player,
        in WinchState winch,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        if (!winch.IsPulling)
        {
            return false;
        }

        if (winch.Path.ContactCount > 0)
        {
            return false;
        }

        var anchor = winch.Path.WorldAnchor;
        var fromAnchorToPlayer = player.Position - anchor;

        if (winch.Path.HasAnchorSurfaceNormal)
        {
            var normal = winch.Path.WorldAnchorNormal;
            var signedSurfaceDistance =
                Vector3.Dot(fromAnchorToPlayer, normal);

            // The selected raycast surface should remain on the outward side of the capsule.
            // A small negative allowance covers numerical skin/margin noise without treating a
            // completely different side of geometry as the same arrival.
            if (signedSurfaceDistance < -config.ArrivalContactTolerance)
            {
                return false;
            }

            var surfaceArrivalDistance =
                ComputeCapsuleAwareArrivalDistance(
                    normal,
                    config,
                    locomotionConfig);

            if (signedSurfaceDistance > surfaceArrivalDistance)
            {
                return false;
            }

            var tangentialOffset =
                fromAnchorToPlayer - (normal * signedSurfaceDistance);

            return tangentialOffset.LengthSquared()
                <= config.ArrivalSurfaceCaptureRadius
                   * config.ArrivalSurfaceCaptureRadius;
        }

        // Fallback for legacy/tests that do not carry an anchor surface normal.
        var distanceSquared = fromAnchorToPlayer.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        var direction = distance > 0f
            ? -fromAnchorToPlayer / distance
            : Vector3.Zero;

        return distance <= ComputeCapsuleAwareArrivalDistance(
            direction,
            config,
            locomotionConfig);
    }

    public static float ComputeCapsuleAwareArrivalDistance(
        Vector3 cableDirection,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        if (cableDirection.LengthSquared() <= TinyDistanceSquared)
        {
            return config.ArrivalContactTolerance;
        }

        // Support distance of a vertical capsule along an arbitrary direction:
        // sphere radius + projected half-segment length.
        var capsuleSupport =
            locomotionConfig.CapsuleRadius
            + (locomotionConfig.CapsuleHalfSegmentLength
               * MathF.Abs(cableDirection.Y))
            + locomotionConfig.CollisionMargin;

        return capsuleSupport
            + config.ArrivalContactTolerance;
    }

    private static bool TryShootCable(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world,
        out WinchState winch)
    {
        var query = BuildAimRay(
            player,
            config,
            locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            winch = default;
            return false;
        }

        var ropeLength =
            Vector3.Distance(
                player.Position,
                hit.Position);

        if (ropeLength > config.MaxRopeLength)
        {
            winch = default;
            return false;
        }

        winch = new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(
                hit.Position,
                hit.Normal),
            false,
            ropeLength,
            0f)
        {
            RopeLength = ropeLength,
            PullElapsedSeconds = 0f,
            IsArrivedLatched = false,
        };

        return true;
    }

    private static Vector3 ViewForward(
        float yawRadians,
        float pitchRadians)
    {
        var cosPitch = MathF.Cos(pitchRadians);

        return Vector3.Normalize(new Vector3(
            -MathF.Sin(yawRadians) * cosPitch,
            MathF.Sin(pitchRadians),
            -MathF.Cos(yawRadians) * cosPitch));
    }
}
