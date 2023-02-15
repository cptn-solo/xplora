namespace Assets.Scripts.Data
{
    public struct TerrainEventConfig
    {
        public TerrainAttribute Attribute { get; private set; }
        public HeroTrait Trait { get; private set; }
        public string Name { get; private set; }

        public static TerrainEventConfig Create(
            TerrainAttribute attribute,
            HeroTrait trait,
            string name)
        {
            return new()
            {
                Attribute = attribute,
                Trait = trait,
                Name = name
            };
        }

    }
}