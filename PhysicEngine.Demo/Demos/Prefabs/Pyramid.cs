using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PhysicEngine.Collision.Shapes;
using PhysicEngine.Demo.MediaSystem;
using PhysicEngine.Demo.MediaSystem.Graphics;
using PhysicEngine.Dynamics;
using PhysicEngine.Factories;
using PhysicEngine.Shared;
using PhysicEngine.Utilities;

namespace PhysicEngine.Demo.Demos.Prefabs
{
    public class Pyramid
    {
        private readonly Sprite _box;
        private readonly List<Body> _boxes;

        public Pyramid(World world, Vector2 position, int count, float density)
        {
            Vertices rect = PolygonUtils.CreateRectangle(0.2f, 0.2f);
            PolygonShape shape = new PolygonShape(rect, density);

            Vector2 rowStart = position;
            rowStart.Y -= 0.5f + count * 1.1f;

            Vector2 deltaRow = new Vector2(-0.625f, 1.1f);
            const float spacing = 1.25f;

            // Physics
            _boxes = new List<Body>();

            for (int i = 0; i < count; i++)
            {
                Vector2 pos = rowStart;

                for (int j = 0; j < i + 1; j++)
                {
                    Body body = BodyFactory.CreateBody(world);
                    body.BodyType = BodyType.Dynamic;
                    body.Position = pos;
                    body.AddFixture(shape);
                    _boxes.Add(body);

                    pos.X += spacing;
                }

                rowStart += deltaRow;
            }

            //GFX
            _box = new Sprite(Managers.TextureManager.PolygonTexture(rect, "Square", Colors.Blue, Colors.Gold, Colors.Black, 1f));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Body body in _boxes)
            {
                batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(body.Position), null, Color.White, body.Rotation, _box.Origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}