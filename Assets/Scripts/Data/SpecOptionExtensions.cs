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
                SpecOption.HP => "ХП",
                _ => "Способность"
            };

        public static string BonusOptionName(this SpecOption specOption, int factor) =>
            specOption switch
            {
                SpecOption.DamageRange => factor == 100 ?
                    $"Удвоить начальную силу героя" : 
                    $"{specOption.Name()} +{factor}",
                SpecOption.DefenceRate => $"{specOption.Name()} +{factor}",
                SpecOption.AccuracyRate => $"{specOption.Name()} +{factor}",
                SpecOption.DodgeRate => $"{specOption.Name()} +{factor}",
                SpecOption.Health => factor == 100 ?
                    "Восстановить здоровье героя" :
                    $"{specOption.Name()} +{factor}",
                SpecOption.Speed => $"{specOption.Name()} +{factor}",
                SpecOption.UnlimitedStaminaTag =>
                    "Сэкономить выносливость отряда",
                SpecOption.HP => $"{specOption.Name()} +{factor}",
                _ => $"{specOption.Name()} +{factor}"
            };        
    }
}