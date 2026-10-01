using System.Numerics;

namespace Hitch.Simulation.Winch;

/// <summary>
/// Snapshot-friendly piecewise-linear rope path.
///
/// The rope is not simulated as many physical particles. It is represented as:
///
/// player -> newest bend -> ... -> oldest bend -> world anchor
///
/// Bends are added when the visible rope segment is obstructed and removed when the player
/// regains direct line of sight to the point behind the current bend.
/// </summary>
public readonly record struct WinchPathState(
    Vector3 WorldAnchor,
    Vector3 WorldAnchorNormal)
{
    public const int MaxContacts = 4;

    public Vector3 Contact0 { get; init; }
    public Vector3 Contact1 { get; init; }
    public Vector3 Contact2 { get; init; }
    public Vector3 Contact3 { get; init; }

    public int ContactCount { get; init; }

    public Vector3 CurrentPullPoint =>
        ContactCount switch
        {
            <= 0 => WorldAnchor,
            1 => Contact0,
            2 => Contact1,
            3 => Contact2,
            _ => Contact3,
        };

    public bool HasAnchorSurfaceNormal =>
        WorldAnchorNormal.LengthSquared() > 1e-10f;

    public static WinchPathState AtWorldAnchor(Vector3 worldAnchor) =>
        new(worldAnchor, Vector3.Zero);

    public static WinchPathState AtWorldAnchor(
        Vector3 worldAnchor,
        Vector3 worldAnchorNormal)
    {
        var normal = worldAnchorNormal.LengthSquared() > 1e-10f
            ? Vector3.Normalize(worldAnchorNormal)
            : Vector3.Zero;

        return new WinchPathState(worldAnchor, normal);
    }

    public Vector3 GetContactFromAnchor(int index)
    {
        if (index < 0 || index >= ContactCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return index switch
        {
            0 => Contact0,
            1 => Contact1,
            2 => Contact2,
            _ => Contact3,
        };
    }

    public Vector3 GetPathPointFromPlayer(int index)
    {
        if (index < 0 || index > ContactCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if (index == ContactCount)
        {
            return WorldAnchor;
        }

        return GetContactFromAnchor(ContactCount - 1 - index);
    }

    public Vector3 PointBehindCurrent =>
        ContactCount switch
        {
            <= 1 => WorldAnchor,
            2 => Contact0,
            3 => Contact1,
            _ => Contact2,
        };

    public WinchPathState PushContact(Vector3 point)
    {
        if (ContactCount >= MaxContacts)
        {
            return this;
        }

        return ContactCount switch
        {
            0 => this with
            {
                Contact0 = point,
                ContactCount = 1,
            },
            1 => this with
            {
                Contact1 = point,
                ContactCount = 2,
            },
            2 => this with
            {
                Contact2 = point,
                ContactCount = 3,
            },
            _ => this with
            {
                Contact3 = point,
                ContactCount = 4,
            },
        };
    }

    public WinchPathState PopContact()
    {
        if (ContactCount <= 0)
        {
            return this;
        }

        return ContactCount switch
        {
            1 => this with
            {
                Contact0 = Vector3.Zero,
                ContactCount = 0,
            },
            2 => this with
            {
                Contact1 = Vector3.Zero,
                ContactCount = 1,
            },
            3 => this with
            {
                Contact2 = Vector3.Zero,
                ContactCount = 2,
            },
            _ => this with
            {
                Contact3 = Vector3.Zero,
                ContactCount = 3,
            },
        };
    }
}
