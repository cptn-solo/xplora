namespace Assets.Scripts.Data
{
    public static class SpecOptionExtensions
    {
        public static string Name(this SpecOption specOption) =>
            specOption switch
            {        
                SpecOption.DamageRange =>   "Урон",
                SpecOption.DefenceRate =>   "Защита",
                SpecOption.AccuracyRate =>  "Точность",
                SpecOption.DodgeRate =>     "Уклонение",
                SpecOption.Health =>        "Здоровье",
                SpecOption.Speed =>         "Скорость",
                SpecOption.UnlimitedStaminaTag => "Выносливость",
                _ => "Способность"
            };
    }
}