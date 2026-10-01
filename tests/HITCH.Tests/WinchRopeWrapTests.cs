using System.Numerics;
using Hitch.Simulation.Winch;
using Hitch.Simulation.World;

namespace Hitch.Tests;

public sealed class WinchRopeWrapTests
{
    [Fact]
    public void PathUsesNewestBendAsCurrentPullPoint()
    {
        var path = WinchPathState
            .AtWorldAnchor(new Vector3(10f, 0f, 0f))
            .PushContact(new Vector3(7f, 0f, 1f))
            .PushContact(new Vector3(4f, 0f, 2f));

        Assert.Equal(2, path.ContactCount);
        Assert.Equal(
            new Vector3(4f, 0f, 2f),
            path.CurrentPullPoint);
        Assert.Equal(
            new Vector3(7f, 0f, 1f),
            path.PointBehindCurrent);

        Assert.Equal(
            new Vector3(4f, 0f, 2f),
            path.GetPathPointFromPlayer(0));
        Assert.Equal(
            new Vector3(7f, 0f, 1f),
            path.GetPathPointFromPlayer(1));
        Assert.Equal(
            new Vector3(10f, 0f, 0f),
            path.GetPathPointFromPlayer(2));
    }

    [Fact]
    public void ObstructedSegmentAddsOutwardOffsetBend()
    {
        var config = TestConfig();
        var path =
            WinchPathState.AtWorldAnchor(
                new Vector3(10f, 0f, 0f));
        var world = new FixedRayHitWorld(
            new WorldHit(
                new Vector3(4f, 0f, 0f),
                -Vector3.UnitX,
                0.4f,
                1u));

        var updated = WinchSystem.UpdateRopePath(
            Vector3.Zero,
            path,
            config,
            world);

        Assert.Equal(1, updated.ContactCount);
        Assert.InRange(updated.CurrentPullPoint.X, 3.91f, 3.93f);
        Assert.Equal(0f, updated.CurrentPullPoint.Y);
        Assert.Equal(0f, updated.CurrentPullPoint.Z);
    }

    [Fact]
    public void EndpointHitDoesNotCreateDuplicateBend()
    {
        var config = TestConfig();
        var anchor = new Vector3(10f, 0f, 0f);
        var path =
            WinchPathState.AtWorldAnchor(anchor);

        var updated = WinchSystem.UpdateRopePath(
            Vector3.Zero,
            path,
            config,
            new EndpointHitWorld());

        Assert.Equal(0, updated.ContactCount);
        Assert.Equal(anchor, updated.CurrentPullPoint);
    }

    [Fact]
    public void ClearSightToPointBehindCurrentUnwrapsBend()
    {
        var path = WinchPathState
            .AtWorldAnchor(new Vector3(10f, 0f, 0f))
            .PushContact(new Vector3(5f, 0f, 2f));

        var updated = WinchSystem.UpdateRopePath(
            Vector3.Zero,
            path,
            TestConfig(),
            new NoHitWorld());

        Assert.Equal(0, updated.ContactCount);
        Assert.Equal(updated.WorldAnchor, updated.CurrentPullPoint);
    }

    [Fact]
    public void ContinuingAroundObstacleCanAddSecondBend()
    {
        var anchor = new Vector3(10f, 0f, 0f);
        var firstBend = new Vector3(5f, 0f, 2f);
        var path = WinchPathState
            .AtWorldAnchor(anchor)
            .PushContact(firstBend);

        var world = new ConditionalWrapWorld(
            anchor,
            firstBend);

        var updated = WinchSystem.UpdateRopePath(
            Vector3.Zero,
            path,
            TestConfig(),
            world);

        Assert.Equal(2, updated.ContactCount);
        Assert.Equal(firstBend, updated.GetContactFromAnchor(0));
        Assert.True(
            Vector3.Distance(
                updated.GetContactFromAnchor(1),
                firstBend)
            > 0.35f);
        Assert.Equal(
            updated.GetContactFromAnchor(1),
            updated.CurrentPullPoint);
    }

    [Fact]
    public void RopeLengthSumsEveryPolylineSegment()
    {
        var path = WinchPathState
            .AtWorldAnchor(new Vector3(3f, 4f, 12f))
            .PushContact(new Vector3(3f, 4f, 0f));

        var length = WinchSystem.ComputeRopePathLength(
            Vector3.Zero,
            path);

        Assert.InRange(length, 16.999f, 17.001f);
    }

    [Fact]
    public void PathNeverStoresMoreThanFourBends()
    {
        var path =
            WinchPathState.AtWorldAnchor(
                new Vector3(20f, 0f, 0f));

        for (var i = 0; i < 8; i++)
        {
            path = path.PushContact(
                new Vector3(15f - i, 0f, i));
        }

        Assert.Equal(
            WinchPathState.MaxContacts,
            path.ContactCount);
    }

    private static WinchConfig TestConfig() =>
        new()
        {
            RopeContactSurfaceOffset = 0.08f,
            RopeEndpointTolerance = 0.22f,
            RopeMinimumContactSpacing = 0.35f,
        };

    private sealed class NoHitWorld : IWorldQuery
    {
        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }

    private sealed class FixedRayHitWorld : IWorldQuery
    {
        private readonly WorldHit _hit;

        public FixedRayHitWorld(WorldHit hit)
        {
            _hit = hit;
        }

        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
        {
            hit = _hit;
            return true;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }

    private sealed class EndpointHitWorld : IWorldQuery
    {
        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
        {
            hit = new WorldHit(
                query.To,
                Vector3.UnitY,
                1f,
                1u);
            return true;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }

    private sealed class ConditionalWrapWorld : IWorldQuery
    {
        private readonly Vector3 _anchor;
        private readonly Vector3 _firstBend;

        public ConditionalWrapWorld(
            Vector3 anchor,
            Vector3 firstBend)
        {
            _anchor = anchor;
            _firstBend = firstBend;
        }

        public bool TryRaycast(
            in RayQuery query,
            out WorldHit hit)
        {
            if (Vector3.DistanceSquared(
                    query.To,
                    _anchor)
                < 1e-6f)
            {
                // The old obstacle still blocks direct sight to the anchor,
                // so the first bend must remain.
                hit = new WorldHit(
                    new Vector3(4.8f, 0f, 1.9f),
                    -Vector3.UnitX,
                    0.5f,
                    1u);
                return true;
            }

            if (Vector3.DistanceSquared(
                    query.To,
                    _firstBend)
                < 1e-6f)
            {
                // Moving farther around the obstacle causes the visible
                // player->bend segment to hit a new corner first.
                hit = new WorldHit(
                    new Vector3(2f, 0f, 1f),
                    -Vector3.UnitZ,
                    0.4f,
                    1u);
                return true;
            }

            hit = default;
            return false;
        }

        public bool TrySweepCapsule(
            in CapsuleSweepQuery query,
            out WorldHit hit)
        {
            hit = default;
            return false;
        }
    }
}
