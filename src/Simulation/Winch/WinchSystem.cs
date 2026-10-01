using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.Player;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Stage 5 iteration 3.
///
/// LMB selects/replaces a point and always cancels any active pull without touching momentum.
/// One RMB click starts an automatic pull. Starting the pull gives an immediate impulse, then
/// continuous acceleration bends the existing trajectory toward the target until arrival.
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

        var winch = previousWinch;
        var updatedPlayer = player;

        if (input.Has(PlayerButtons.SelectGrapplePointPressed))
        {
            // Throwing/replacing the cable always ends the previous pull first.
            // Velocity is untouched, so the player keeps flying only by inertia.
            winch = TrySelectTarget(
                player,
                config,
                locomotionConfig,
                world);
        }

        if (!winch.HasTarget)
        {
            return new WinchStepResult(updatedPlayer, WinchState.Initial);
        }

        var toTarget = winch.Path.CurrentPullPoint - player.Position;
        var distanceSquared = toTarget.LengthSquared();
        var distance = distanceSquared <= TinyDistanceSquared
            ? 0f
            : MathF.Sqrt(distanceSquared);

        var direction = distance > 0f
            ? toTarget / distance
            : Vector3.Zero;

        var startedThisTick =
            input.Has(PlayerButtons.PullPressed)
            && !winch.IsPulling;

        if (startedThisTick)
        {
            winch = winch with { IsPulling = true };

            if (distance > 0f)
            {
                updatedPlayer = updatedPlayer with
                {
                    Velocity = updatedPlayer.Velocity
                        + (direction * config.PullInitialImpulse),
                    IsGrounded = false,
                };
            }
        }

        if (!winch.IsPulling)
        {
            return new WinchStepResult(
                updatedPlayer,
                winch with
                {
                    LastActualDistance = distance,
                    LastPullAcceleration = 0f,
                });
        }

        if (distance <= config.ArrivalDistance)
        {
            // Pull is finished. Remove only motion still pointing into the anchor.
            // Tangential momentum remains, while gravity/ordinary locomotion resume immediately.
            var velocity = updatedPlayer.Velocity;
            if (distance > 0f)
            {
                var inwardSpeed = Vector3.Dot(velocity, direction);
                if (inwardSpeed > 0f)
                {
                    velocity -= direction * inwardSpeed;
                }
            }

            updatedPlayer = updatedPlayer with
            {
                Velocity = velocity,
                IsGrounded = false,
            };

            return new WinchStepResult(
                updatedPlayer,
                WinchState.Initial);
        }

        if (distance > 0f)
        {
            updatedPlayer = updatedPlayer with
            {
                Velocity = updatedPlayer.Velocity
                    + (direction * config.PullAcceleration * fixedDeltaSeconds),
                IsGrounded = false,
            };
        }

        return new WinchStepResult(
            updatedPlayer,
            winch with
            {
                LastActualDistance = distance,
                LastPullAcceleration = config.PullAcceleration,
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

    private static WinchState TrySelectTarget(
        in PlayerState player,
        WinchConfig config,
        PlayerLocomotionConfig locomotionConfig,
        IWorldQuery world)
    {
        var query = BuildAimRay(player, config, locomotionConfig);

        if (!world.TryRaycast(query, out var hit))
        {
            // LMB still retracts/cancels the old cable even if the new throw misses.
            return WinchState.Initial;
        }

        return new WinchState(
            WinchTargetState.Selected,
            WinchPathState.AtWorldAnchor(hit.Position),
            false,
            Vector3.Distance(player.Position, hit.Position),
            0f);
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
