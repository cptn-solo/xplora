using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public class DamageTypesConfigLoader : BaseConfigLoader
    {
        private readonly DamageTypesLibrary library;
        private const int colNumber = 6;

        protected override string RangeString => "'Урон'!A1:H6";
        protected override string ConfigName => "DamageTypes";

        public DamageTypesConfigLoader(DamageTypesLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }
        protected override void ProcessList(IList<IList<object>> list)
        {
            if (list == null || list.Count < 3)
                return;

            object val(int row, int cell) => list[row][cell];

            for (int row = 1; row <= 5; row++)
            {

                var damageType = val(row, 0).ParseDamageType();
                var effectType = val(row, 1).ParseDamageEffect();
                var turnsCountInt = val(row, 2).ParseAbsoluteValue();
                var skipTurnsBool = val(row, 3).ParseBoolValue(); //yes no true false 1 0 etc.
                var extraDamage = val(row, 4).ParseAbsoluteValue();
                var useShieldRate = val(row, 5).ParseRateValue();
                var chanceRate = val(row, 6).ParseRateValue();

                library.UpdateConfig(damageType, effectType, turnsCountInt, skipTurnsBool, extraDamage, useShieldRate,
                                     chanceRate);
            }
        }
    }
}