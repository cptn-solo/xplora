using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public class TerrainEventsConfigLoader : BaseConfigLoader
    {
        private readonly TerrainEventLibrary? library;

        protected override string RangeString => "'Гексы/Черты'!A12:L17";
        protected override string ConfigName => "TerrainEvents";

        public TerrainEventsConfigLoader(TerrainEventLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }

        public TerrainEventsConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 6;

            if (list == null || list.Count < rowsCount || library == null)
                return;

            object val(int row, int cell) => list[row][cell];

            var prevAttr = TerrainAttribute.NA;
            for (int row = 0; row < rowsCount; row++)
            {

                var attribute = val(row, 0).ParseTerrainAttribute();

                if (attribute == TerrainAttribute.NA)
                    attribute = prevAttr;
                else prevAttr = attribute;

                var trait = val(row, 1).ParseHeroTrait();
                var name = (string)val(row, 2);

                var bonusOptions = ListPool<EventBonusConfig>.Get();

                var bonusOptionConfigs = ListPool<BonusOptionConfig>.Get();

                var optionKeys = val(row, 3).ParseSpecOptionsArray();
                var optionSpawnRates = val(row, 4).ParseIntArray();
                var optionFactors = val(row, 5).ParseIntArray();

                for (int i = 0; i < optionKeys.Length; i++)
                    bonusOptionConfigs.Add(BonusOptionConfig.Create(
                        optionKeys[i], HeroTrait.NA, optionSpawnRates[i], optionFactors[i]));

                bonusOptions.Add(EventBonusConfig.Create(bonusOptionConfigs.ToArray()));

                bonusOptionConfigs.Clear();

                optionKeys = val(row, 6).ParseSpecOptionsArray();
                optionSpawnRates = val(row, 7).ParseIntArray();
                optionFactors = val(row, 8).ParseIntArray();

                for (int i = 0; i < optionKeys.Length; i++)
                    bonusOptionConfigs.Add(BonusOptionConfig.Create(
                        optionKeys[i], HeroTrait.NA, optionSpawnRates[i], optionFactors[i]));

                bonusOptions.Add(EventBonusConfig.Create(bonusOptionConfigs.ToArray()));

                bonusOptionConfigs.Clear();

                var traitKeys = val(row, 9).ParseHeroTraitsArray();
                var traitSpawnRates = val(row, 10).ParseIntArray();
                var traitFactors = val(row, 11).ParseIntArray();

                for (int i = 0; i < traitKeys.Length; i++)
                    bonusOptionConfigs.Add(BonusOptionConfig.Create(
                        SpecOption.NA, traitKeys[i], traitSpawnRates[i], traitFactors[i]));

                bonusOptions.Add(EventBonusConfig.Create(bonusOptionConfigs.ToArray()));

                ListPool<BonusOptionConfig>.Add(bonusOptionConfigs);

                library.Value.UpdateConfig(attribute, trait, name, bonusOptions.ToArray());

                ListPool<EventBonusConfig>.Add(bonusOptions);
            }
        }

    }
}