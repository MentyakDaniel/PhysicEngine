using System;

namespace PhysicEngine.Interfaces
{
    public interface IPoolable<T> : IDisposable where T : IPoolable<T>
    {
        void Reset();
    }
}
