using System.Numerics;
using Hitch.Simulation.Input;
using Hitch.Simulation.State;
using Hitch.Simulation.World;

namespace Hitch.Simulation.Player;

/// <summary>
/// One fixed-tick update for non-winch player locomotion.
/// Additional walking/jump/air-control behavior is layered into this class during Stage 4.
/// </summary>
public static class PlayerLocomotionSystem
{
    public static PlayerState Step(
        in PlayerState player,
        in PlayerInput input,
        PlayerLocomotionConfig config,
        IWorldQuery world,
        float fixedDeltaSeconds)
    {
        _ = input;

        var velocity = player.Velocity + new Vector3(
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
}
