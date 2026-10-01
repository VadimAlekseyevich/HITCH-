using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 dual-cable action prototype.
///
/// LMB owns the left cable. RMB owns the right cable.
/// Each click raycasts/replaces only that side and starts its pull immediately.
/// Both active cables contribute symmetrically to player velocity.
/// </summary>
public static class WinchSystem
{
    private const float TinyDistanceSquared = 1e-10f;

    // IWorldQuery requires a finite ray endpoint. This is an engine-query distance only,
    // deliberately far beyond the enclosed movement room. It is NOT a gameplay rope-length cap.
    private const float EngineSafeRaycastDistance = 10_000f;

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

        var left = previousWinch.Left;
        var right = previousWinch.Right;
        var updatedPlayer = player;

        var leftStartedThisTick = false;
        var rightStartedThisTick = false;

        if (input.Has(PlayerButtons.LeftGrapplePressed))
        {
            left = TryShootActiveCable(
                player,
                config,
                locomotionConfig,
                world);
            leftStartedThisTick = left.IsPulling;
        }

        if (input.Has(PlayerButtons.RightGrapplePressed))
        {
            right = TryShootActiveCable(
                player,
                config,
                locomotionConfig,
                world);
            rightStartedThisTick = right.IsPulling;
        }

        var leftDistance = GetCableGeometry(
            left,
            player.Position,
            out var leftDirection);
        var rightDistance = GetCableGeometry(
            right,
            player.Position,
            out var rightDirection);

        var leftArrived =
            left.IsPulling
            && leftDistance <= config.ArrivalDistance;
        var rightArrived =
            right.IsPulling
            && rightDistance <= config.ArrivalDistance;

        if (leftArrived)
        {
            left = WinchCableState.Initial;
            leftStartedThisTick = false;
        }

        if (rightArrived)
        {
            right = WinchCableState.Initial;
            rightStartedThisTick = false;
        }

        // Preserve the established single-cable rule:
        // when the LAST active cable fully reels in, stop completely.
        // If the other cable is still active, only the arrived side clears.
        if ((leftArrived || rightArrived)
            && !left.IsPulling
            && !right.IsPulling)
        {
            updatedPlayer = updatedPlayer with
            {
                Velocity = Vector3.Zero,
                IsGrounded = false,
            };

            return new WinchStepResult(
                updatedPlayer,
                new WinchState(left, right));
        }

        var velocity = updatedPlayer.Velocity;

        if (leftStartedThisTick && leftDistance > 0f)
        {
            velocity += leftDirection * config.PullInitialImpulse;
        }

        if (rightStartedThisTick && rightDistance > 0f)
        {
            velocity += rightDirection * config.PullInitialImpulse;
        }

        // Both cables read the same post-impulse base velocity, then their corrections are summed.
        // This keeps left/right behavior symmetric instead of depending on update order.
        var pullBaseVelocity = velocity;

        var leftDelta = ComputeCableVelocityDelta(
            left,
            pullBaseVelocity,
            leftDirection,
            config,
            fixedDeltaSeconds,
            out var leftAcceleration);
        var rightDelta = ComputeCableVelocityDelta(
            right,
            pullBaseVelocity,
            rightDirection,
            config,
            fixedDeltaSeconds,
            out var rightAcceleration);

        velocity += leftDelta + rightDelta;

        if (left.IsPulling)
        {
            left = left with
            {
                LastActualDistance = leftDistance,
                LastPullAcceleration = leftAcceleration,
            };
        }

        if (right.IsPulling)
        {
            right = right with
            {
                LastActualDistance = rightDistance,
                LastPullAcceleration = rightAcceleration,
            };
        }

        if (left.IsPulling || right.IsPulling)
        {
            updatedPlayer = updatedPlayer with
            {
                Velocity = velocity,
                IsGrounded = false,
            };
        }

        return new WinchStepResult(
            updatedPlayer,
            new WinchState(left, right));
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
            eye + (direction * EngineSafeRaycastDistance),
            config.GrappleCollisionMask);
    }

    private static WinchCableState TryShootActiveCable(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(
            player,
            config,
            locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            return WinchCableState.Initial;
        }

        return new WinchCableState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(hit.Position),
            true,
            Vector3.Distance(player.Position, hit.Position),
            0f);
    }

    private static float GetCableGeometry(
        in WinchCableState cable,
        Vector3 playerPosition,
        out Vector3 direction)
    {
        direction = Vector3.Zero;

        if (!cable.IsPulling)
        {
            return 0f;
        }

        var toTarget =
            cable.Path.CurrentPullPoint - playerPosition;
        var distanceSquared = toTarget.LengthSquared();

        if (distanceSquared <= TinyDistanceSquared)
        {
            return 0f;
        }

        var distance = MathF.Sqrt(distanceSquared);
        direction = toTarget / distance;
        return distance;
    }

    private static Vector3 ComputeCableVelocityDelta(
        in WinchCableState cable,
        Vector3 baseVelocity,
        Vector3 direction,
        WinchConfig config,
        float fixedDeltaSeconds,
        out float appliedRadialAcceleration)
    {
        appliedRadialAcceleration = 0f;

        if (!cable.IsPulling
            || direction.LengthSquared() <= TinyDistanceSquared)
        {
            return Vector3.Zero;
        }

        var inwardSpeed =
            Vector3.Dot(baseVelocity, direction);

        if (inwardSpeed >= config.PullTargetInwardSpeed)
        {
            return Vector3.Zero;
        }

        var neededSpeed =
            config.PullTargetInwardSpeed - inwardSpeed;
        var addedSpeed = MathF.Min(
            neededSpeed,
            config.PullRadialAcceleration * fixedDeltaSeconds);

        appliedRadialAcceleration =
            addedSpeed / fixedDeltaSeconds;

        return direction * addedSpeed;
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
