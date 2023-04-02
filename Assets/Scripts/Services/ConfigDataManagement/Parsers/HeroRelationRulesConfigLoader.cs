using Assets.Scripts.Data;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public delegate ref HeroRelationsConfig HeroRelationsConfigProcessor();
    
    public class HeroRelationRulesConfigLoader : BaseConfigLoader
    {
        private readonly HeroRelationsConfigProcessor configProcessor;
        private readonly bool process = false;
        
        private delegate object Ev(int row, int col);

        protected override string RangeString => "'События Отношений'!A1:M59";
        protected override string ConfigName => "RelationsConfig";

        public HeroRelationRulesConfigLoader(
            HeroRelationsConfigProcessor configProcessor, DataDelegate onDataAvailable) :
            base(onDataAvailable)
        {
            this.configProcessor = configProcessor;
            process = true;
        }

        public HeroRelationRulesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            var rowsCount = 7;

            if (list == null || list.Count < rowsCount || !process)
                return;

            object val(int row, int cell)
            {
                if (list.Count <= row || list[row].Count <= cell)
                    return "";

                return list[row][cell];
            }

            ref var config = ref configProcessor();

            config.SpiritKindGroup = new[] {
                HeroKind.Asc,
                HeroKind.Spi,
                HeroKind.Int,
                HeroKind.Cha,
            };

            config.BodyKindGroup = new[] {
                HeroKind.Tem,
                HeroKind.Con,
                HeroKind.Str,
                HeroKind.Dex,
            };
            
            var cellNeutral = val(8, 4).ParseIntArray(true);
            config.NeutralGroupScoreRange = new IntRange(cellNeutral[0], cellNeutral[1]);

            var cellSpirit = val(9, 4).ParseIntArray(true);
            config.SpiritGroupScoreRange = new IntRange(cellSpirit[0], cellSpirit[1]);
            
            var cellBody = val(10, 4).ParseIntArray(true);
            config.BodyGroupScoreRange = new IntRange(cellBody[0], cellBody[1]);

            config.RelationMatrix = new();

            var spiritToSpirit = val(14, 2).ParseIntArray(true);
            var bodyToBody = val(15, 2).ParseIntArray(true);
            var spiritToNeutral = val(16, 2).ParseIntArray(true);
            var bodyToNeutral = val(17, 2).ParseIntArray(true);
            var spiritToBody = val(18, 2).ParseIntArray(true);
            var bodyToSpirit = val(19, 2).ParseIntArray(true);
            
            config.RelationMatrix.Add(HeroKindGroup.Spirit, new()
            {
                { HeroKindGroup.Spirit, new IntRange(spiritToSpirit[0], spiritToSpirit[1]) },
                { HeroKindGroup.Neutral, new IntRange(spiritToNeutral[0], spiritToNeutral[1]) },
                { HeroKindGroup.Body, new IntRange(spiritToBody[0], spiritToBody[1]) },
            });

            config.RelationMatrix.Add(HeroKindGroup.Body, new()
            {
                { HeroKindGroup.Spirit, new IntRange(bodyToSpirit[0], bodyToSpirit[1]) },
                { HeroKindGroup.Neutral, new IntRange(bodyToNeutral[0], bodyToNeutral[1]) },
                { HeroKindGroup.Body, new IntRange(bodyToBody[0], bodyToBody[1]) },
            });

            // all neutral relations is taken from a single cell - spirit-to-neutral
            config.RelationMatrix.Add(HeroKindGroup.Neutral, new() 
            {
                { HeroKindGroup.Spirit, new IntRange(spiritToNeutral[0], spiritToNeutral[1]) },
                { HeroKindGroup.Neutral, new IntRange(spiritToNeutral[0], spiritToNeutral[1]) },
                { HeroKindGroup.Body, new IntRange(spiritToNeutral[0], spiritToNeutral[1]) },
            });

            var basisValue = val(34, 1).ParseFloatRateValue();
            config.EventSpawnRateThresholds = new RelationEventTriggerRange[] {
                new (val(33, 0).ParseIntRangeValue(), val(33,1).ParseFloatRateValue(), 0f),
                new (val(34, 0).ParseIntRangeValue(), basisValue, 0f),
                new (val(36, 0).ParseIntRangeValue(), basisValue, val(36,1).ParseFloatRateValue()),
                new (val(37, 0).ParseIntRangeValue(), basisValue, val(37,1).ParseFloatRateValue()),
                new (val(38, 0).ParseIntRangeValue(), basisValue, val(38,1).ParseFloatRateValue()),
            };

            var rstLow = val(4, 3).ParseIntValue(0, true);
            var rstEnemies = val(4, 4).ParseIntValue(0, true);
            var rstBad = val(4, 5).ParseIntValue(0, true);
            var rstGood = val(4, 6).ParseIntValue(0, true);
            var rstFrieds = val(4, 7).ParseIntValue(0, true);
            var rstHigh = val(4, 8).ParseIntValue(0, true);

            config.RelationStateThresholds = new[]{
                new RelationStateValue(RelationState.Low, rstLow),
                new RelationStateValue(RelationState.Enemies, rstEnemies),
                new RelationStateValue(RelationState.Bad, rstBad),
                new RelationStateValue(RelationState.Good, 0),
                new RelationStateValue(RelationState.Friends, rstGood),
                new RelationStateValue(RelationState.High, rstFrieds),
                new RelationStateValue(RelationState.OverHigh, rstHigh),
            };
            
            var spiritRules = ReadTargetRules(6, 44, val);
            var bodyRules = ReadTargetRules(6, 53, val);
            
            config.RelationTargetRules = new()
            {
                { HeroKindGroup.Spirit, spiritRules},
                { HeroKindGroup.Body, bodyRules }
            };
                   
        }

        private RelationTargetRuleConfig[] ReadTargetRules(int rangeCnt, int spiritRow, Ev val)
        {
            var spiritTargetRules = new RelationTargetRuleConfig[rangeCnt];

            for (int idx = 0; idx < rangeCnt; idx++)
            {
                var row = spiritRow + idx;
                var range = val(row, 0).ParseIntArray();

                var neutralRate = val(row, 1).ParseIntValue();
                var neutralIn = val(row, 2).ParseRelationBonus();
                var neutralOut = val(row, 3).ParseRelationBonus();

                var spiritRate = val(row, 4).ParseIntValue();
                var spiritCompare = val(row, 5).ParseIntValue(0, true);
                var spiritIn = val(row, 6).ParseRelationBonus();
                var spiritOut = val(row, 7).ParseRelationBonus();

                var bodyRate = val(row, 8).ParseIntValue();
                var bodyCompare = val(row, 9).ParseIntValue(0, true);
                var bodyIn = val(row, 10).ParseRelationBonus();
                var bodyOut = val(row, 11).ParseRelationBonus();

                var bonusRules = new Dictionary<HeroKindGroup, RelationTargetInfo>
                {
                    {
                        HeroKindGroup.Neutral,
                        new RelationTargetInfo()
                        {
                            Rate = neutralRate,
                            Compared = 0,
                            IncomingBonus = neutralIn,
                            OutgoingBonus = neutralOut
                        }
                    },
                    {
                        HeroKindGroup.Spirit,
                        new RelationTargetInfo()
                        {
                            Rate = spiritRate,
                            Compared = spiritCompare,
                            IncomingBonus = spiritIn,
                            OutgoingBonus = spiritOut
                        }
                    },
                    {
                        HeroKindGroup.Body,
                        new RelationTargetInfo()
                        {
                            Rate = bodyRate,
                            Compared = bodyCompare,
                            IncomingBonus = bodyIn,
                            OutgoingBonus = bodyOut
                        }
                    }
                };

                var targetConfig = new RelationTargetRuleConfig()
                {
                    TiltThreshold = new IntRange(range[0], range[1]),
                    KindGroupBonusRules = bonusRules
                };

                spiritTargetRules[idx] = targetConfig;                            
            }

            return spiritTargetRules;
        }
    }
}