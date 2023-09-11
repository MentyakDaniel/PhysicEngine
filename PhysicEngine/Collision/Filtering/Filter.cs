using PhysicEngine.Utilities;

namespace PhysicEngine.Collision.Filtering
{
    public class Filter
    {
        public short Group { get; set; }
        public Category Category { get; set; }
        public Category CategoryMask { get; set; }

        public Filter()
        {
            Group = Settings.DefaultCollisionGroup;
            Category = Settings.DefaultFixtureCollisionCategories;
            CategoryMask = Settings.DefaultFixtureCollidesWith;
        }

        public Filter(short group, Category category, Category mask)
        {
            Group = group;
            Category = category;
            CategoryMask = mask;
        }
    }
}
