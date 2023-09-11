using System.Collections.Generic;

namespace PhysicEngine.Extensions.PhysicsLogics.Explosion
{
    internal class RayDataComparer : IComparer<float>
    {
        int IComparer<float>.Compare(float a, float b)
        {
            float diff = a - b;
            if (diff > 0)
                return 1;
            if (diff < 0)
                return -1;
            return 0;
        }
    }
}
