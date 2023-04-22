namespace Assets.Scripts.Data
{
    public static class HeroRelationEffectsLibraryExtensions
    {
        public static bool TrySpawnAdditionalEffect(this HeroRelationEffectsLibrary library, int currentCount)
        {
            if (!library.AdditionalEffectSpawnRate.TryGetValue(currentCount, out var spawnRate))
                return false;

            return spawnRate.RatedRandomBool();
        }
    }
}