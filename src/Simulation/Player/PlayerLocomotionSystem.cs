using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Player;

/// <summary>
/// One fixed-tick update for non-winch player locomotion.
/// </summary>
public static class PlayerLocomotionSystem
{
    private const float TinySpeedSquared = 1e-10f;

    public static PlayerState Step(
        in PlayerState player,
        in PlayerInput input,
        PlayerLocomotionConfig config,
        IWorldQuery world,
        float fixedDeltaSeconds)
    {
        var velocity = player.Velocity;

        if (player.IsGrounded)
        {
            velocity = ApplyGroundControl(
                velocity,
                input.Move,
                player.ViewYawRadians,
                config,
                fixedDeltaSeconds);
        }

        // Gravity is applied every tick. A floor collision removes the downward component.
        // This makes gravity begin immediately when a grounded player moves off an edge.
        velocity += new Vector3(
            0f,
            -config.Gravity * fixedDeltaSeconds,
            0f);

        var moved = CapsuleMovementSolver.Move(
            player with { Velocity = velocity },
            config,
            world,
            fixedDeltaSeconds);

        var grounded = GroundDetector.IsGrounded(moved, config, world);

        if (grounded && moved.Velocity.Y < 0f)
        {
            moved = moved with
            {
                Velocity = new Vector3(
                    moved.Velocity.X,
                    0f,
                    moved.Velocity.Z),
            };
        }

        return moved with { IsGrounded = grounded };
    }

    private static Vector3 ApplyGroundControl(
        Vector3 velocity,
        Vector2 moveInput,
        float viewYawRadians,
        PlayerLocomotionConfig config,
        float fixedDeltaSeconds)
    {
        var horizontal = new Vector3(velocity.X, 0f, velocity.Z);
        var inputLengthSquared = moveInput.LengthSquared();

        if (inputLengthSquared <= TinySpeedSquared)
        {
            // Do not automatically kill future winch/external momentum just because it exceeds
            // ordinary walking speed.
            if (horizontal.LengthSquared()
                <= config.GroundMaxSpeed * config.GroundMaxSpeed)
            {
                horizontal = MoveTowards(
                    horizontal,
                    Vector3.Zero,
                    config.GroundBraking * fixedDeltaSeconds);
            }

            return new Vector3(horizontal.X, velocity.Y, horizontal.Z);
        }

        var input = inputLengthSquared > 1f
            ? Vector2.Normalize(moveInput)
            : moveInput;

        var forward = new Vector3(
            -MathF.Sin(viewYawRadians),
            0f,
            -MathF.Cos(viewYawRadians));
        var right = new Vector3(
            MathF.Cos(viewYawRadians),
            0f,
            -MathF.Sin(viewYawRadians));

        var wishDirection = (right * input.X) + (forward * input.Y);
        if (wishDirection.LengthSquared() > TinySpeedSquared)
        {
            wishDirection = Vector3.Normalize(wishDirection);
        }

        var speedAlongWish = Vector3.Dot(horizontal, wishDirection);
        var controllableSpeedRemaining = config.GroundMaxSpeed - speedAlongWish;

        if (controllableSpeedRemaining > 0f)
        {
            var addedSpeed = MathF.Min(
                config.GroundAcceleration * fixedDeltaSeconds,
                controllableSpeedRemaining);
            horizontal += wishDirection * addedSpeed;
        }

        return new Vector3(horizontal.X, velocity.Y, horizontal.Z);
    }

    private static Vector3 MoveTowards(
        Vector3 current,
        Vector3 target,
        float maxDistanceDelta)
    {
        var delta = target - current;
        var distanceSquared = delta.LengthSquared();

        if (distanceSquared <= maxDistanceDelta * maxDistanceDelta
            || distanceSquared <= TinySpeedSquared)
        {
            return target;
        }

        return current + (Vector3.Normalize(delta) * maxDistanceDelta);
    }
}
