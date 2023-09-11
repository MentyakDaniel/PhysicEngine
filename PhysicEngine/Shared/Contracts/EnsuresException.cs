using System;

namespace PhysicEngine.Shared.Contracts
{
    public class EnsuresException : Exception
    {
        public EnsuresException(string message) : base(message) { }
    }
}
