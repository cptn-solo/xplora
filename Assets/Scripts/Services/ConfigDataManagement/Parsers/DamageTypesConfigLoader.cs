using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public class DamageTypesConfigLoader : BaseConfigLoader
    {
        private readonly DamageTypesLibrary? library;
        private const int colNumber = 6;

        protected override string RangeString => "'Урон'!A1:H6";
        protected override string ConfigName => "DamageTypes";

        public DamageTypesConfigLoader(DamageTypesLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }
        public DamageTypesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            if (list == null || list.Count < 3 || library == null)
                return;

            object val(int row, int cell) => list[row][cell];

            for (int row = 1; row <= 5; row++)
            {

                var damageType = val(row, 0).ParseDamageType();
                var effectType = val(row, 1).ParseDamageEffect();
                var turnsCountInt = val(row, 2).ParseIntValue();
                var skipTurnsBool = val(row, 3).ParseBoolValue(); //yes no true false 1 0 etc.
                var extraDamage = val(row, 4).ParseIntValue();
                var useShieldRate = val(row, 5).ParseIntValue();
                var chanceRate = val(row, 6).ParseIntValue();

                library.Value.UpdateConfig(damageType, effectType, turnsCountInt, skipTurnsBool, extraDamage, useShieldRate,
                                     chanceRate);
            }
        }
    }
}