using Microsoft.Xna.Framework;
using PhysicEngine.Collision.Shapes;
using PhysicEngine.Dynamics.Joints;
using PhysicEngine.Shared;

namespace PhysicEngine.Interfaces
{
    public interface IDebugView
    {
        void DrawJoint(Joint joint);
        void DrawShape(Shape shape, ref Transform transform, Color color);
    }
}