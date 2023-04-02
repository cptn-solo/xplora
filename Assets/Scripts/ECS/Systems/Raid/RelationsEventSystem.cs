using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class RelationsEventSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<RelationsEventInfo> eventInfoPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEventItemInfo>> draftTagPool = default;
        private readonly EcsPoolInject<IntValueComp<RelationScoreTag>> scorePool = default;
        private readonly EcsPoolInject<RelationScoreRef> scoreRefPool = default;
                
        private readonly EcsFilterInject<
            Inc<RelationsEventInfo, DraftTag<RelationEventItemInfo>>> filter = default; // test
        private readonly EcsFilterInject<
            Inc<KindGroupNeutralTag, IntValueComp<HeroKindRSDTag>, NameValueComp<NameTag>
                >> neutralTagFilter = default;
        private readonly EcsFilterInject<
            Inc<KindGroupSpiritTag, IntValueComp<HeroKindRSDTag>, NameValueComp<NameTag>
                >> spiritTagFilter = default;
        private readonly EcsFilterInject<
            Inc<KindGroupBodyTag, IntValueComp<HeroKindRSDTag>, NameValueComp<NameTag>
                >> bodyTagFilter = default;

        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value) {
                ComposeEvent(entity);
                draftTagPool.Value.Del(entity);
            }           
        }
        private void ComposeEvent(int playerEntity) // test implementation
        {
            ref var info = ref eventInfoPool.Value.Get(playerEntity);

            if (!info.SourceEntity.Unpack(ecsWorld.Value, out var srcHeroEntity))
            {
                // stale/incomplete draft, exception actually but let it go for now
                eventInfoPool.Value.Del(playerEntity);
                return;
            }

            ref var config = ref heroLibraryService.Value.HeroRelationsConfig;

            info.SrcRSD = ecsWorld.Value.ReadIntValue<HeroKindRSDTag>(srcHeroEntity);
            info.SrcKindGroup = config.GetKindGroup(info.SrcRSD);

            var targetRules = config.GetTargetRules(info.SrcRSD, info.SrcKindGroup);

            if (targetRules == null)
            {
                // no targets for neutral heroes yet (rules should be defined one day then we'll add'em here)
                eventInfoPool.Value.Del(playerEntity);
                return;
            }

            // 1. Pick event target kind group
            PickEventTargetGroup(targetRules,
                out Dictionary<HeroKindGroup, RelationTargetInfo> bonusRules,
                out HeroKindGroup targetKindGroup);

            if (targetKindGroup == HeroKindGroup.NA)
            {
                // hardly can imagine why this happened
                eventInfoPool.Value.Del(playerEntity);
                throw new Exception("Can't pick relation event target group");
            }

            // 2. Pik event target hero
            PickEventTargetHero(config, ref info, bonusRules, targetKindGroup, out int tgtHeroEntity);

            if (tgtHeroEntity < 0)
            {
                eventInfoPool.Value.Del(playerEntity);
                Debug.Log($"Relation event missed: no suitable target");
                return;
            }

            // 3. updating relation score
            UpateRelationScore(config, ref info);

            // 4. updating kind values 
            UpdateKindValues(ref info);

            // 5. producing view data
            ProduceViewData(config, ref info);
        }

        private void UpateRelationScore(
            HeroRelationsConfig config,
            ref RelationsEventInfo info)
        {
            if (!info.SourceEntity.Unpack(ecsWorld.Value, out var srcHeroEntity) ||
                info.TargetEntity == null ||
                !info.TargetEntity.Value.Unpack(ecsWorld.Value, out var tgtHeroEntity))
                return;

            var scoreFactorRange = config.RelationMatrix[info.SrcKindGroup][info.TgtKindGroup];
            var scoreFactor = scoreFactorRange.RandomValue;
            ref var scoreComp = ref GetScore(srcHeroEntity, info.TargetEntity.Value, out var scoreEntity);
            scoreComp.Value += scoreFactor;
            info.ScoreEntity = ecsWorld.Value.PackEntity(scoreEntity);
            info.ScoreDiff = scoreFactor;
            info.Score = scoreComp.Value;
        }

        private void PickEventTargetGroup(RelationTargetRuleConfig? targetRules, out Dictionary<HeroKindGroup, RelationTargetInfo> bonusRules, out HeroKindGroup winnerKindGroup)
        {
            bonusRules = targetRules.Value.KindGroupBonusRules;
            var totalRate = 0;
            var spareRate = 0; // will accumulate spawn rate of kind groups not present in the current team
            var buff = ListPool<Tuple<HeroKindGroup, int, int>>.Get();

            // 1. initial rate assignment (for groups present in a team)
            foreach (var ruleProbe in bonusRules)
            {
                var tgtKind = ruleProbe.Key;

                // 1.1. skip groups not present in a team
                if (CheckIfPresentInTeam(tgtKind, ruleProbe.Value.Rate, ref spareRate))
                    buff.Add(new(tgtKind, ruleProbe.Value.Rate, totalRate += ruleProbe.Value.Rate));
            }

            // 2. share spare rate (if any) among present groups
            if (spareRate > 0)
            {
                var share = (int)(spareRate / buff.Count);
                if (share > 0)
                {
                    totalRate = 0;
                    for (int i = 0; i < buff.Count; i++)
                    {
                        var old = buff[i];
                        var withShare = old.Item2 + share;
                        buff[i] = new(old.Item1, withShare, totalRate += withShare);
                    }
                }
            }

            // 3. decide on target group

            var probe = Random.Range(0, totalRate + 1);
            winnerKindGroup = HeroKindGroup.NA;
            foreach (var item in buff)
            {
                if (item.Item3 < probe)
                    continue;

                winnerKindGroup = item.Item1;
                break;
            }

            ListPool<Tuple<HeroKindGroup, int, int>>.Add(buff);
        }

        private void PickEventTargetHero(
            HeroRelationsConfig config,
            ref RelationsEventInfo info, 
            Dictionary<HeroKindGroup, RelationTargetInfo> bonusRules, 
            HeroKindGroup winnerKindGroup,
            out int tgtHeroEntity)
        {
            int[] winnerCandidates = winnerKindGroup switch
            {
                HeroKindGroup.Neutral => neutralTagFilter.Value.AllEntities(),
                HeroKindGroup.Spirit => spiritTagFilter.Value.AllEntities(),
                HeroKindGroup.Body => bodyTagFilter.Value.AllEntities(),
                _ => new int[] { },
            };

            tgtHeroEntity = -1;
            info.Rule = bonusRules[winnerKindGroup];
            if (winnerKindGroup == HeroKindGroup.Neutral)
            {
                tgtHeroEntity = winnerCandidates[Random.Range(0, winnerCandidates.Length)];
            }
            else
            {
                var rivals = ListPool<int>.Get();

                foreach (var tmEntity in winnerCandidates)
                {
                    var tmRSD = ecsWorld.Value.ReadIntValue<HeroKindRSDTag>(tmEntity);
                    var tmRSDAbs = Mathf.Abs(tmRSD);
                    if (info.Rule.Compared > 0 && tmRSDAbs > info.SrcRSDAbs)
                        rivals.Add(tmEntity);
                    else if (info.Rule.Compared < 0 && tmRSDAbs < info.SrcRSDAbs)
                        rivals.Add(tmEntity);
                    else if (info.Rule.Compared == 0 && tmRSDAbs == info.SrcRSDAbs)
                        rivals.Add(tmEntity);
                }

                if (rivals.Count > 0)
                {
                    tgtHeroEntity = rivals[Random.Range(0, rivals.Count)];
                }
                ListPool<int>.Add(rivals);
            }

            if (tgtHeroEntity < 0)
                return;

            info.TargetEntity = ecsWorld.Value.PackEntity(tgtHeroEntity);
            info.TgtRSD = ecsWorld.Value.ReadIntValue<HeroKindRSDTag>(tgtHeroEntity);
            info.TgtKindGroup = config.GetKindGroup(info.TgtRSD);

        }

        private void UpdateKindValues(ref RelationsEventInfo info)
        {
            if (!info.SourceEntity.Unpack(ecsWorld.Value, out var srcHeroEntity) ||
                info.TargetEntity == null ||
                !info.TargetEntity.Value.Unpack(ecsWorld.Value, out var tgtHeroEntity))
                return;

            var itemBuff = ListPool<RelationEventItemInfo>.Get();
            var index = new Dictionary<HeroKind, int>();
            foreach (var bonusKind in info.Rule.IncomingBonus.TargetKinds)
            {
                var diff = info.Rule.IncomingBonus.Bonus;
                var current = IncrementKindValue(srcHeroEntity, bonusKind, diff);
                if (index.TryGetValue(bonusKind, out var idx))
                {
                    var itemInfo = itemBuff[idx];
                    itemInfo.SrcCurrentValue = current;
                    itemInfo.SrcDiffValue = diff;
                    itemBuff[idx] = itemInfo;
                }
                else
                {
                    var itemInfo = new RelationEventItemInfo()
                    {
                        Kind = bonusKind,
                        SrcCurrentValue = current,
                        SrcDiffValue = diff,
                    };
                    itemBuff.Add(itemInfo);
                    index.Add(bonusKind, itemBuff.Count - 1);
                }
            }

            foreach (var bonusKind in info.Rule.OutgoingBonus.TargetKinds)
            {
                var dif = info.Rule.OutgoingBonus.Bonus;
                var current = IncrementKindValue(tgtHeroEntity, bonusKind, dif);
                if (index.TryGetValue(bonusKind, out var idx))
                {
                    var itemInfo = itemBuff[idx];
                    itemInfo.TgtCurrentValue = current;
                    itemInfo.TgtDiffValue = dif;
                    itemBuff[idx] = itemInfo;
                }
                else
                {
                    var itemInfo = new RelationEventItemInfo()
                    {
                        Kind = bonusKind,
                        TgtCurrentValue = current,
                        TgtDiffValue = dif,
                    };
                    itemBuff.Add(itemInfo);
                    index.Add(bonusKind, itemBuff.Count - 1);
                }

            }

            info.EventItems = itemBuff.ToArray();

            ListPool<RelationEventItemInfo>.Add(itemBuff);
        }

        private void ProduceViewData(
            HeroRelationsConfig config, 
            ref RelationsEventInfo info)
        {
            if (!info.SourceEntity.Unpack(ecsWorld.Value, out var srcHeroEntity) ||
                info.TargetEntity == null ||
                !info.TargetEntity.Value.Unpack(ecsWorld.Value, out var tgtHeroEntity))
                return;

            ref var srcName = ref namePool.Value.Get(srcHeroEntity);
            ref var srcIconName = ref iconNamePool.Value.Get(srcHeroEntity);

            ref var tgtName = ref namePool.Value.Get(tgtHeroEntity);
            ref var tgtIconName = ref iconNamePool.Value.Get(tgtHeroEntity);

            Debug.Log($"Relation event spawned between {srcName.Name} and {tgtName.Name}");

            // to show current value for positive diff we should 1st substract it so total bar value will 
            // represent a value _after_ increment. If the diff was negative we show current value as it is
            // and change color for the diff part to some negative color (yellow). same for items below.
            var score = GetScoreValueForBar(info.Score, info.ScoreDiff);
            var delta = Mathf.Abs(info.ScoreDiff);
            var scoreMax = 30;

            var relationState = config.GetRelationState(info.Score);
            info.EventTitle = $"Score {info.Score}: {relationState}";
            info.SrcIconName = srcIconName.Name;
            info.TgtIconName = tgtIconName.Name;
            info.ActionTitles = new[] { $"OK" };
            info.ScoreInfo = new()
            {
                ScoreSign = info.Score >= 0 ? 1 : -1,
                ScoreInfo = new()
                {
                    Title = $"{info.Score}",
                    Value = Mathf.Min(1, (float)score / scoreMax),
                    Delta = Mathf.Min(1, (float)delta / scoreMax),
                    Color = info.Score > 0 ? Color.green : Color.red,
                    DeltaColor = Color.yellow,
                }
            };
            var items = info.EventItems;
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                item.ItemTitle = item.Kind.Name();
                item.SrcBarInfo = new()
                {
                    Title = $"{item.SrcCurrentValue}",
                    Value = Mathf.Min(1, (float)GetScoreValueForBar(item.SrcCurrentValue, item.SrcDiffValue)
                        / scoreMax),
                    Delta = Mathf.Min(1, (float)Mathf.Abs(item.SrcDiffValue) / scoreMax),
                    Color = Color.red,
                    DeltaColor = item.SrcDiffValue > 0 ? Color.cyan : Color.yellow,
                };
                item.TgtBarInfo = new()
                {
                    Title = $"{item.TgtCurrentValue}",
                    Value = Mathf.Min(1, (float)GetScoreValueForBar(item.TgtCurrentValue, item.TgtDiffValue)
                        / scoreMax),
                    Delta = Mathf.Min(1, (float)Mathf.Abs(item.TgtDiffValue) / scoreMax),
                    Color = Color.red,
                    DeltaColor = item.TgtDiffValue > 0 ? Color.cyan : Color.yellow,
                };

                items[i] = item;
            }

            info.EventItems = items;

            static int GetScoreValueForBar(int current, int diff) =>
                Mathf.Sign(current) == Mathf.Sign(diff) ?
                    Mathf.Abs(current - diff) :
                    Mathf.Abs(current);
        }

        private int IncrementKindValue(int entity, HeroKind kind, int factor)
        {
            return kind switch
            {
                HeroKind.Asc => ecsWorld.Value.IncrementIntValue<HeroKindAscTag>(factor, entity),
                HeroKind.Spi => ecsWorld.Value.IncrementIntValue<HeroKindSpiTag>(factor, entity),
                HeroKind.Int => ecsWorld.Value.IncrementIntValue<HeroKindIntTag>(factor, entity),
                HeroKind.Cha => ecsWorld.Value.IncrementIntValue<HeroKindChaTag>(factor, entity),
                HeroKind.Tem => ecsWorld.Value.IncrementIntValue<HeroKindTemTag>(factor, entity),
                HeroKind.Con => ecsWorld.Value.IncrementIntValue<HeroKindConTag>(factor, entity),
                HeroKind.Str => ecsWorld.Value.IncrementIntValue<HeroKindStrTag>(factor, entity),
                HeroKind.Dex => ecsWorld.Value.IncrementIntValue<HeroKindDexTag>(factor, entity),
                _ => 0,
            };
        }

        private ref IntValueComp<RelationScoreTag> GetScore(
            int srcHeroEntity, 
            EcsPackedEntity tgtHeroPackedEntity, 
            out int scoreEntity)
        {
            scoreEntity = -1;
            
            ref var scoreRef = ref scoreRefPool.Value.Get(srcHeroEntity);
            if (!scoreRef.Parties.TryGetValue(tgtHeroPackedEntity, out var packed) ||
                !packed.Unpack(ecsWorld.Value, out scoreEntity))
                throw new Exception($"Can't get relations Score");
            
            ref var scoreComp = ref scorePool.Value.Get(scoreEntity);

            return ref scoreComp;            
        }
            

        private bool CheckIfPresentInTeam(HeroKindGroup tgtKind, int rate, ref int spareRate)
        {
            var retval = tgtKind switch
            {
                HeroKindGroup.Neutral => neutralTagFilter.Value.GetEntitiesCount() > 0,
                HeroKindGroup.Spirit => spiritTagFilter.Value.GetEntitiesCount() > 0,
                HeroKindGroup.Body => bodyTagFilter.Value.GetEntitiesCount() > 0,
                _ => false,
            };

            if (retval == false)
                spareRate += rate;
            
            return retval;
        }
    }
}