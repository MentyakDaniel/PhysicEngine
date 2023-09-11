using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Handlers;
using PhysicEngine.Collision.Raycast;
using PhysicEngine.Dynamics;
using PhysicEngine.Shared;
using System;

namespace PhysicEngine.Interfaces
{
    public interface IBroadPhase
    {
        int ProxyCount { get; }

        void UpdatePairs(BroadphaseHandler callback);

        bool TestOverlap(int proxyIdA, int proxyIdB);

        int AddProxy(ref FixtureProxy proxy);

        void RemoveProxy(int proxyId);

        void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement);

        FixtureProxy GetProxy(int proxyId);

        void TouchProxy(int proxyId);

        void GetFatAABB(int proxyId, out AABB aabb);

        void Query(Func<int, bool> callback, ref AABB aabb);

        void RayCast(Func<RaycastInput, int, float> callback, ref RaycastInput input);

        void ShiftOrigin(ref Vector2 newOrigin);
    }
}
