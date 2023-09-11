using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Handlers;
using PhysicEngine.Collision.Raycast;
using PhysicEngine.Dynamics;
using PhysicEngine.Interfaces;
using PhysicEngine.Shared;
using System;

namespace PhysicEngine.Collision.Broadphase
{
    public class DynamicTreeBroadPhase : IBroadPhase
    {
        public const int NullProxy = -1;
        private int[] _moveBuffer;
        private int _moveCapacity;
        private int _moveCount;

        private Pair[] _pairBuffer;
        private int _pairCapacity;
        private int _pairCount;
        private int _proxyCount;
        private Func<int, bool> _queryCallback;
        private int _queryProxyId;
        private DynamicTree<FixtureProxy> _tree = new DynamicTree<FixtureProxy>();

        public DynamicTreeBroadPhase()
        {
            _queryCallback = QueryCallback;
            _proxyCount = 0;

            _pairCapacity = 16;
            _pairCount = 0;
            _pairBuffer = new Pair[_pairCapacity];

            _moveCapacity = 16;
            _moveCount = 0;
            _moveBuffer = new int[_moveCapacity];
        }

        public float TreeQuality => _tree.AreaRatio;

        public int TreeHeight => _tree.Height;
        public int ProxyCount => _proxyCount;
        public int AddProxy(ref FixtureProxy proxy)
        {
            int proxyId = _tree.CreateProxy(ref proxy.AABB, proxy);
            ++_proxyCount;
            BufferMove(proxyId);
            return proxyId;
        }

        public bool TestOverlap(int proxyIdA, int proxyIdB)
        {
            _tree.GetFatAABB(proxyIdA, out AABB aabbA);
            _tree.GetFatAABB(proxyIdB, out AABB aabbB);
            return AABB.TestOverlap(ref aabbA, ref aabbB);
        }
        private bool QueryCallback(int proxyId)
        {
            if (proxyId == _queryProxyId)
                return true;

            bool moved = _tree.WasMoved(proxyId);

            if (moved && proxyId > _queryProxyId)
                return true;

            if (_pairCount == _pairCapacity)
            {
                Pair[] oldBuffer = _pairBuffer;
                _pairCapacity += (_pairCapacity >> 1);
                _pairBuffer = new Pair[_pairCapacity];
                Array.Copy(oldBuffer, _pairBuffer, _pairCount);
            }

            _pairBuffer[_pairCount].ProxyIdA = Math.Min(proxyId, _queryProxyId);
            _pairBuffer[_pairCount].ProxyIdB = Math.Max(proxyId, _queryProxyId);
            ++_pairCount;

            return true;
        }

        public void UpdatePairs(BroadphaseHandler callback)
        {
            _pairCount = 0;

            for (int i = 0; i < _moveCount; ++i)
            {
                _queryProxyId = _moveBuffer[i];
                if (_queryProxyId == NullProxy)
                    continue;

                _tree.GetFatAABB(_queryProxyId, out AABB fatAABB);

                _tree.Query(_queryCallback, ref fatAABB);
            }

            for (int i = 0; i < _pairCount; ++i)
            {
                Pair primaryPair = _pairBuffer[i];
                FixtureProxy userDataA = _tree.GetUserData(primaryPair.ProxyIdA);
                FixtureProxy userDataB = _tree.GetUserData(primaryPair.ProxyIdB);

                callback(ref userDataA, ref userDataB);
            }

            for (int i = 0; i < _moveCount; ++i)
            {
                int proxyId = _moveBuffer[i];
                if (proxyId == NullProxy)
                    continue;

                _tree.ClearMoved(proxyId);
            }

            _moveCount = 0;
        }
        private void BufferMove(int proxyId)
        {
            if (_moveCount == _moveCapacity)
            {
                int[] oldBuffer = _moveBuffer;
                _moveCapacity *= 2;
                _moveBuffer = new int[_moveCapacity];
                Array.Copy(oldBuffer, _moveBuffer, _moveCount);
            }

            _moveBuffer[_moveCount] = proxyId;
            ++_moveCount;
        }
        private void UnBufferMove(int proxyId)
        {
            for (int i = 0; i < _moveCount; ++i)
            {
                if (_moveBuffer[i] == proxyId)
                    _moveBuffer[i] = NullProxy;
            }
        }
        public void RemoveProxy(int proxyId)
        {
            UnBufferMove(proxyId);
            --_proxyCount;
            _tree.DestroyProxy(proxyId);
        }
        public void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            bool buffer = _tree.MoveProxy(proxyId, ref aabb, displacement);
            if (buffer)
                BufferMove(proxyId);
        }

        public void TouchProxy(int proxyId)                                                     => BufferMove(proxyId);
        public void GetFatAABB(int proxyId, out AABB aabb)                                      => _tree.GetFatAABB(proxyId, out aabb);
        public FixtureProxy GetProxy(int proxyId)                                               => _tree.GetUserData(proxyId);
        public void Query(Func<int, bool> callback, ref AABB aabb)                              => _tree.Query(callback, ref aabb);
        public void RayCast(Func<RaycastInput, int, float> callback, ref RaycastInput input)    => _tree.RayCast(callback, ref input);
        public void ShiftOrigin(ref Vector2 newOrigin)                                          => _tree.ShiftOrigin(ref newOrigin);
    }
}
