using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Player;

/// <summary>
/// One fixed-tick update for intentionally modest non-winch locomotion.
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
        var airJumpAvailable = player.AirJumpAvailable;

        if (player.IsGrounded)
        {
            // Touching ground restores the one airborne jump.
            airJumpAvailable = true;

            velocity = ApplyGroundControl(
                velocity,
                input.Move,
                player.ViewYawRadians,
                config,
                fixedDeltaSeconds);

            if (input.Has(PlayerButtons.JumpPressed))
            {
                velocity = new Vector3(
                    velocity.X,
                    config.JumpSpeed,
                    velocity.Z);
            }
        }
        else
        {
            velocity = ApplyAirControl(
                velocity,
                input.Move,
                player.ViewYawRadians,
                config,
                fixedDeltaSeconds);

            if (input.Has(PlayerButtons.JumpPressed)
                && airJumpAvailable)
            {
                velocity = new Vector3(
                    velocity.X,
                    config.AirJumpSpeed,
                    velocity.Z);
                airJumpAvailable = false;
            }
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

        if (grounded)
        {
            airJumpAvailable = true;
        }

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

        return moved with
        {
            IsGrounded = grounded,
            AirJumpAvailable = airJumpAvailable,
        };
    }

    private static Vector3 ApplyGroundControl(
        Vector3 velocity,
        Vector2 moveInput,
        float viewYawRadians,
        PlayerLocomotionConfig config,
        float fixedDeltaSeconds)
    {
        var horizontal = new Vector3(velocity.X, 0f, velocity.Z);
        var wishDirection = BuildWishDirection(moveInput, viewYawRadians);

        var desired = wishDirection.LengthSquared() <= TinySpeedSquared
            ? Vector3.Zero
            : wishDirection * config.GroundMaxSpeed;

        var response = desired == Vector3.Zero
            ? config.GroundBraking
            : config.GroundAcceleration;

        horizontal = MoveTowards(
            horizontal,
            desired,
            response * fixedDeltaSeconds);

        return new Vector3(horizontal.X, velocity.Y, horizontal.Z);
    }

    private static Vector3 ApplyAirControl(
        Vector3 velocity,
        Vector2 moveInput,
        float viewYawRadians,
        PlayerLocomotionConfig config,
        float fixedDeltaSeconds)
    {
        var wishDirection = BuildWishDirection(moveInput, viewYawRadians);

        if (wishDirection.LengthSquared() <= TinySpeedSquared
            || config.AirAcceleration <= 0f)
        {
            return velocity;
        }

        var horizontal = new Vector3(velocity.X, 0f, velocity.Z);
        horizontal = AccelerateAlongWishDirection(
            horizontal,
            wishDirection,
            config.AirControlMaxSpeed,
            config.AirAcceleration,
            fixedDeltaSeconds);

        return new Vector3(horizontal.X, velocity.Y, horizontal.Z);
    }

    private static Vector3 BuildWishDirection(
        Vector2 moveInput,
        float viewYawRadians)
    {
        var inputLengthSquared = moveInput.LengthSquared();
        if (inputLengthSquared <= TinySpeedSquared)
        {
            return Vector3.Zero;
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

        var direction = (right * input.X) + (forward * input.Y);

        return direction.LengthSquared() > TinySpeedSquared
            ? Vector3.Normalize(direction)
            : Vector3.Zero;
    }

    private static Vector3 AccelerateAlongWishDirection(
        Vector3 horizontalVelocity,
        Vector3 wishDirection,
        float controlMaxSpeed,
        float acceleration,
        float fixedDeltaSeconds)
    {
        var speedAlongWish = Vector3.Dot(horizontalVelocity, wishDirection);
        var controllableSpeedRemaining = controlMaxSpeed - speedAlongWish;

        if (controllableSpeedRemaining <= 0f)
        {
            return horizontalVelocity;
        }

        var addedSpeed = MathF.Min(
            acceleration * fixedDeltaSeconds,
            controllableSpeedRemaining);

        return horizontalVelocity + (wishDirection * addedSpeed);
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
