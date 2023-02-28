using System;
using Assets.Scripts.ECS.Data;

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

        public static Type BuffComp(this SpecOption specOption) =>
            specOption switch
            {
                SpecOption.DamageRange => typeof(BuffComp<DamageRangeComp>),
                SpecOption.Speed => typeof(BuffComp<SpeedComp>),
                SpecOption.Health => typeof(BuffComp<HealthComp>), // pls note that
                                                                   // health buff add HP
                                                                   // but not health (design decision)
                SpecOption.HP => typeof(BuffComp<HPComp>),
                _ => typeof(BuffComp<DummyBuff>)
            };
    }
}