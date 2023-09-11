using System;

namespace PhysicEngine.Shared.Contracts
{
    public class RequiredException : Exception
    {
        public RequiredException(string message) : base(message) { }
    }
}
