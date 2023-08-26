using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleRelEffectProbeSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelEffectProbeComp> pool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> effectsPool = default;
        private readonly EcsPoolInject<DraftTag<EffectInstanceInfo>> draftTagPool = default;        
        
        private readonly EcsFilterInject<Inc<RelEffectProbeComp>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;
        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                TryCastRelationEffect(entity);
            }
        }

        /// <summary>
        /// Will add relation effect if rules applied will allow to
        /// </summary>
        /// <param name="effectProbeEntity">Entity of an effect probe between two heroes</param>
        /// <returns>true if new effect was casted</returns>
        private bool TryCastRelationEffect(int effectProbeEntity)
        {
            ref var probe = ref pool.Value.Get(effectProbeEntity);

            if (!probe.SourceOrigPacked.Unpack(out var origWorld, out var effeectSourceEntity))
                throw new Exception("Stale Other Guy Entity (probably dead already)");

            if (!probe.P2PEntityPacked.Unpack(out _, out var p2pEntity))
                throw new Exception("Stale Score Entity");

            var relationsConfig = heroLibraryService.Value.HeroRelationsConfigProcessor();
            var effectRules = heroLibraryService.Value.HeroRelationEffectsLibProcessor();

            // respect spawn rate from AdditionalEffectSpawnRate:
            if (!origWorld.TrySpawnAdditionalEffect(p2pEntity, effectRules))
                return false;

            if (!origWorld.GetEffectConfigForProbe(p2pEntity, effectRules, relationsConfig, probe, out var scope))
                return false; // no effect configured for current state or heroes of the current probe

            var ruleType = scope.EffectRule.EffectType;

            if (ruleType.EffectClass() != RelationsEffectClass.Battle)
                return false; // this system process only battle effects
            
            // all checks passed, effect spawn confirmed, queueing for instantiating:

            draftTagPool.Value.Add(effectProbeEntity);

            // 1. add component to the entity of the current score data;
            // 2. keep all spawned effects (EffectRules) in that component;
            // 3. count of already spawned effects used as a wheight for spawn rate (key for 
            // AdditioinalEffectSpawnRate queries);

            var rule = (IBattleEffectRule)scope.EffectRule;
            ref var currentRound = ref battleService.Value.CurrentRound;
            ref var currentTurn = ref battleService.Value.CurrentTurn;


            // remove existing effect of type:
            var world = pool.Value.GetWorld();
            var effectTargetOrig = rule.Key.RelationsEffectType == RelationsEffectType.AlgoRevenge ?
                probe.SourceOrigPacked : probe.TargetOrigPacked;
            var effectTargetBattle = world.GetHeroInstanceMappings().OriginToBattleMapping[effectTargetOrig];
            if (!effectTargetBattle.Unpack(out _, out var effectTarget))
                throw new Exception("Stale effect target");

            var effectEntity = effectProbeEntity;
            var existingEffectEntity = -1;

            if (rule.EffectType == RelationsEffectType.AlgoTarget)
            {
                // we need to remove any existing target effect prior to cast new ones or there will be an error of adding several effects of the same type
                // (target effect is copied to all teammates in the BattlePrepareTargetEffectSystem)
                var targetEffectFilter = world.Filter<EffectInstanceInfo>().End();
                var targetEffectPool = world.GetPool<EffectInstanceInfo>();
                foreach (var targetEffectEntity in targetEffectFilter)
                {
                    ref var targetEffectInstance = ref targetEffectPool.Get(targetEffectEntity);
                    if (targetEffectInstance.Rule.EffectType == RelationsEffectType.AlgoTarget)
                        world.DelEntity(targetEffectEntity);
                }
            }

            var en = world.SubjectEffectsOfFullKeyEntities(effectTarget, rule.Key);
            while (en.MoveNext())
            {
                existingEffectEntity = en.Current;
                break;
            }

            if (existingEffectEntity >= 0)
                effectEntity = existingEffectEntity;
            else
                effectsPool.Value.Add(effectEntity);

            ref var effect = ref effectsPool.Value.Get(effectEntity);
            effect.StartRound = currentRound.Round;
            effect.StartTurn = currentTurn.Turn;
            effect.UsageLeft = rule.TurnsCount;
            effect.Rule = rule;
            if (rule.Key.RelationsEffectType == RelationsEffectType.AlgoRevenge)
            {
                // inversion for revenge effect as it is spawned in return (from the attacked hero to the revenger)
                // so it is easyer to keep this direction from the very beginning
                effect.EffectSource = probe.TargetOrigPacked;
                effect.EffectTarget = probe.SourceOrigPacked;
            }
            else
            {
                effect.EffectSource = probe.SourceOrigPacked;
                effect.EffectTarget = probe.TargetOrigPacked;
            }
            effect.EffectP2PEntity = probe.P2PEntityPacked;

            origWorld.IncrementIntValue<RelationEffectsCountTag>(1, p2pEntity);

            return true;
        }
    }
}
