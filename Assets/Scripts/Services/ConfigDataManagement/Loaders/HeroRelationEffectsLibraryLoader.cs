using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public delegate ref HeroRelationEffectsLibrary HeroRelationsEffectsLibraryProcessor();

    public class HeroRelationEffectsLibraryLoader : BaseConfigLoader
    {
        private readonly HeroRelationsEffectsLibraryProcessor configProcessor;
        private readonly bool process = false;

        private delegate object Ev(int row, int col);

        protected override string RangeString => "'Эффекты Боя'!A1:K39";
        protected override string ConfigName => "RelationsEffectsLibrary";

        public HeroRelationEffectsLibraryLoader(
            HeroRelationsEffectsLibraryProcessor configProcessor, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.configProcessor = configProcessor;
            process = true;
        }

        public HeroRelationEffectsLibraryLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var heroCount = 8;
            var stateCount = 2;
            var relationStateCount = 4;

            if (list == null || list.Count < heroCount || !process)
                return;


            object val(int row, int cell)
            {
                if (list.Count <= row || list[row].Count <= cell)
                    return "";

                return list[row][cell];
            }

            ref var config = ref configProcessor();

            // Effect rules:

            Dictionary<RelationEffectLibraryKey, HeroRelationEffectConfig> index = new();

            for (int i = 0; i < heroCount; i++)
            {
                var heroId = i;
                var row = i + 2;
                for (int j = 0; j < stateCount; j++)
                {
                    var subjectState = j switch
                    {
                        0 => RelationSubjectState.Attacking,
                        _ => RelationSubjectState.BeingAttacked
                    };

                    for (int k = 1; k < relationStateCount * 2; k += 2)
                    {
                        var relationState = k switch
                        {
                            1 => RelationState.Enemies,
                            3 => RelationState.Bad,
                            5 => RelationState.Good,
                            _ => RelationState.Friends,
                        };
                        var col = k + 3;
                        var libKey = new RelationEffectLibraryKey(heroId, subjectState, relationState);
                        var ruleSource = val(row, col).ToString().Trim();
                        var effectConfig = new HeroRelationEffectConfig()
                        {
                            HeroId = heroId,
                            SelfState = subjectState,
                            TargetState = RelationSubjectState.NA,
                            RelationState = relationState,
                            EffectRule = ruleSource.ParseRelationEffectRule()
                        };
                        index.Add(libKey, effectConfig);
                    }
                }
            }

            config.SubjectStateEffectsIndex = index;

            // Additional effects spawn rate:

            Dictionary<int, int> spawnRates = new();
            var gradesCount = 4;
            var startRow = 34;

            for (int i = startRow; i < startRow + gradesCount; i++)
            {
                var appliedCnt = val(i, 0).ParseIntValue();
                var spawnRate = val(i, 1).ParseIntValue();
                spawnRates.Add(appliedCnt, spawnRate);
            }

            config.AdditionalEffectSpawnRate = spawnRates;

        }

    }
}