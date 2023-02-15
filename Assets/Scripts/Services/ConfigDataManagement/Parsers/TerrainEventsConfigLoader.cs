using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public class TerrainEventsConfigLoader : BaseConfigLoader
    {
        private readonly TerrainEventLibrary library;

        protected override string RangeString => "'Гексы/Черты'!A12:L17";
        protected override string ConfigName => "TerrainEvents";

        public TerrainEventsConfigLoader(TerrainEventLibrary library, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.library = library;
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 6;

            if (list == null || list.Count < rowsCount)
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

                library.UpdateConfig(attribute, trait, name);
            }
        }

    }
}