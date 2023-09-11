using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicEngine.Demo.MediaSystem;
using PhysicEngine.Demo.MediaSystem.Graphics;
using PhysicEngine.Demo.Screens;
using PhysicEngine.Dynamics;
using PhysicEngine.Factories;
using PhysicEngine.Utilities;

namespace PhysicEngine.Demo.Demos
{
    internal class D01_SingleFixture : PhysicsDemoScreen
    {
        private Body _rectangle;
        private Sprite _rectangleSprite;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0,1);

            _rectangle = BodyFactory.CreateRectangle(World, 10f, 10f, .1f);
            _rectangle.BodyType = BodyType.Dynamic;

            SetUserAgent(_rectangle, 100f, 100f);

            // create sprite based on body
            _rectangleSprite = new Sprite(Managers.TextureManager.TextureFromShape(_rectangle.FixtureList[0].Shape, "Square", Colors.Blue, Colors.Gold, Colors.Black, 1f));
        }

        public override void Draw()
        {
            Sprites.Begin(0, null, null, null, null, null, Camera.View);
            Sprites.Draw(_rectangleSprite.Image, ConvertUnits.ToDisplayUnits(_rectangle.Position), null, Color.White, _rectangle.Rotation, _rectangleSprite.Origin, 1f, SpriteEffects.None, 0f);
            Sprites.End();

            base.Draw();
        }

        public override string GetTitle()
        {
            return "Single body with a single fixture";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with one attached fixture and shape.");
            sb.AppendLine("A fixture binds a shape to a body and adds material properties such");
            sb.AppendLine("as density, friction, and restitution.");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate object: Q, E");
            sb.AppendLine("  - Move object: W, S, A, D");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse:");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: Move mouse");
#elif XBOX
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: Left and right trigger");
            sb.AppendLine("  - Move object: Right thumbstick");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.AppendLine("  - Exit to demo selection: Back button");
#endif
            return sb.ToString();
        }
    }
}