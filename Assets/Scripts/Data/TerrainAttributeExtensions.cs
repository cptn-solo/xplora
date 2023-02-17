namespace Assets.Scripts.Data
{
    public static class TerrainAttributeExtensions
    {
        public static string Name(this TerrainAttribute attribute) =>
            attribute switch
            {
                TerrainAttribute.Bush =>        "Кустарник",
                TerrainAttribute.Frowers =>     "Цветы",
                TerrainAttribute.Mushrums =>    "Грибы",
                TerrainAttribute.Trees =>       "Деревья",
                TerrainAttribute.River =>       "Река",
                TerrainAttribute.Path =>        "Тропа",
                _ => "Атрибут"
            };
    }
}