using System;

namespace PhysicEngine.Collision.ContactSystem
{
    [Flags]
    internal enum ContactFlags : byte
    {
        Unknown = 0,
        IslandFlag = 1,
        TouchingFlag = 2,
        EnabledFlag = 4,
        FilterFlag = 8,
        BulletHitFlag = 16,
        TOIFlag = 32
    }
}
