using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    public struct TerrainEventLibrary
    {
        public Dictionary<TerrainAttribute, TerrainEventConfig>
            TerrainEvents { get; private set; }
        public Dictionary<HeroTrait, TerrainEventConfig>
            TraitEvents { get; private set; }

        public static TerrainEventLibrary EmptyLibrary()
        {
            TerrainEventLibrary result = default;

            result.TerrainEvents = new();
            result.TraitEvents = new();

            return result;
        }

        internal void UpdateConfig(
            TerrainAttribute attribute,
            HeroTrait trait,
            string name)
        {
            var config = TerrainEventConfig.Create(
                attribute,
                trait,
                name);

            if (TerrainEvents.TryGetValue(attribute, out _))
                TerrainEvents[attribute] = config;
            else
                TerrainEvents.Add(attribute, config);

            if (TraitEvents.TryGetValue(trait, out _))
                TraitEvents[trait] = config;
            else
                TraitEvents.Add(trait, config);
        }

    }
}