using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 world-anchor winch simulation.
///
/// The implementation is intentionally explicit and experimental. It owns gameplay state while
/// Godot only supplies world-query observations.
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
        float fixedDeltaSeconds)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(locomotionConfig);
        ArgumentNullException.ThrowIfNull(world);

        var winch = previousWinch with
        {
            ReattachCooldownRemaining = MathF.Max(
                0f,
                previousWinch.ReattachCooldownRemaining - fixedDeltaSeconds),
            LastTensionAcceleration = 0f,
        };

        // If press and release are observed within the same simulation tick, release wins.
        // This avoids creating a one-tick accidental attachment from a very fast click.
        if (input.Has(PlayerButtons.GrappleReleased))
        {
            if (winch.IsAttached)
            {
                winch = Detach(winch, config);
            }
        }
        else if (input.Has(PlayerButtons.GrapplePressed)
                 && !winch.IsAttached
                 && winch.ReattachCooldownRemaining <= 0f)
        {
            winch = TryAttach(player, winch, config, locomotionConfig, world);
        }

        if (!winch.IsAttached)
        {
            return new WinchStepResult(
                player,
                winch with
                {
                    ReelVelocity = 0f,
                    LastActualDistance = 0f,
                    LastTensionAcceleration = 0f,
                });
        }

        var pullPoint = winch.Path.CurrentPullPoint;
        var toAnchor = pullPoint - player.Position;
        var distanceSquared = toAnchor.LengthSquared();

        if (distanceSquared <= TinyDistanceSquared)
        {
            return new WinchStepResult(
                player,
                winch with
                {
                    LastActualDistance = 0f,
                    LastTensionAcceleration = 0f,
                });
        }

        var distance = MathF.Sqrt(distanceSquared);
        var reelVelocity = UpdateReelVelocity(
            winch.ReelVelocity,
            input.ReelAxis,
            player.Velocity.Length(),
            config,
            fixedDeltaSeconds);

        var restLength = MathF.Max(
            config.MinimumRopeLength,
            winch.RestLength - (reelVelocity * fixedDeltaSeconds));

        // Neutral/inward operation automatically takes up obvious slack without creating an
        // outward spring force. Explicit reel-out is allowed to create temporary controlled slack.
        if (input.ReelAxis >= 0f && distance < restLength)
        {
            restLength = MathF.Max(
                config.MinimumRopeLength,
                MoveTowards(
                    restLength,
                    distance,
                    config.SlackTakeUpSpeed * fixedDeltaSeconds));
        }

        var inward = toAnchor / distance;
        var outwardSpeed = MathF.Max(
            0f,
            Vector3.Dot(player.Velocity, -inward));

        var extension = distance - restLength + config.PretensionDistance;
        var tensionAcceleration = 0f;
        var updatedPlayer = player;

        if (extension > 0f)
        {
            tensionAcceleration =
                (extension * config.SpringAccelerationPerMeter)
                + (outwardSpeed * config.OutwardDampingPerSecond);

            updatedPlayer = player with
            {
                Velocity = player.Velocity
                    + (inward * tensionAcceleration * fixedDeltaSeconds),
            };
        }

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                RestLength = restLength,
                ReelVelocity = reelVelocity,
                LastActualDistance = distance,
                LastTensionAcceleration = tensionAcceleration,
            });
    }

    public static RayQuery BuildAimRay(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig)
    {
        var eye = player.Position
            + (Vector3.UnitY * locomotionConfig.EyeOffsetFromCapsuleCenter);
        var direction = ViewForward(
            player.ViewYawRadians,
            player.ViewPitchRadians);

        return new RayQuery(
            eye,
            eye + (direction * config.GrappleRange),
            config.GrappleCollisionMask);
    }

    public static float GetReelInMultiplier(
        float playerSpeed,
        WinchConfig config)
    {
        if (playerSpeed <= config.ReelFalloffStartSpeed)
        {
            return 1f;
        }

        if (playerSpeed >= config.ReelFalloffEndSpeed)
        {
            return config.MinimumReelInMultiplier;
        }

        var t =
            (playerSpeed - config.ReelFalloffStartSpeed)
            / (config.ReelFalloffEndSpeed - config.ReelFalloffStartSpeed);

        return 1f + ((config.MinimumReelInMultiplier - 1f) * t);
    }

    private static WinchState TryAttach(
        in PlayerState player,
        in WinchState current,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(player, config, locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            return current;
        }

        var distance = Vector3.Distance(player.Position, hit.Position);
        var restLength = MathF.Max(
            config.MinimumRopeLength,
            distance);

        return new WinchState(
            WinchAttachmentState.Attached,
            WinchPathState.AtWorldAnchor(hit.Position),
            restLength,
            0f,
            0f,
            distance,
            0f);
    }

    private static WinchState Detach(
        in WinchState winch,
        WinchConfig config) =>
        new(
            WinchAttachmentState.Detached,
            default,
            0f,
            0f,
            config.ReattachCooldownSeconds,
            0f,
            0f);

    private static float UpdateReelVelocity(
        float current,
        float reelAxis,
        float playerSpeed,
        WinchConfig config,
        float fixedDeltaSeconds)
    {
        var axis = Math.Clamp(reelAxis, -1f, 1f);
        var target = 0f;

        if (axis > 0f)
        {
            target =
                axis
                * config.ReelMaxSpeed
                * GetReelInMultiplier(playerSpeed, config);
        }
        else if (axis < 0f)
        {
            target = axis * config.ReelMaxSpeed;
        }

        var rate = axis == 0f
            ? config.ReelDeceleration
            : config.ReelAcceleration;

        return MoveTowards(
            current,
            target,
            rate * fixedDeltaSeconds);
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

    private static float MoveTowards(
        float current,
        float target,
        float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta)
        {
            return target;
        }

        return current + (MathF.Sign(target - current) * maxDelta);
    }
}
