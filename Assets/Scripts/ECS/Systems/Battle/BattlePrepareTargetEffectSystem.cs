using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePrepareTargetEffectSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<DraftTag<EffectInstanceInfo>> effectDraftPool = default;
        private readonly EcsPoolInject<RelEffectProbeComp> probePool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEffectsFocusPendingComp>> draftTagPool = default;
        private readonly EcsPoolInject<DeadTag> deadTagPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag>,
            Exc<DeadTag>> teammateFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (!battleManagementService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                throw new Exception("No battle!");

            foreach (var entity in filter.Value)
            {
                ref var effect = ref pool.Value.Get(entity);

                if (effect.Rule.Key.RelationsEffectType == RelationsEffectType.AlgoTarget)
                {
                    ref var probe = ref probePool.Value.Get(entity);
                    if (!probe.TurnEntity.Unpack(systems.GetWorld(), out var turnEntity))
                        throw new Exception("Stale Turn Entity");

                    // registering effect for the hero affected (in the battle world, to make it handy when needed) 
                    ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                    
                    SpreadEffectToAllTeammates(probe, entity, effect, attackerRef.Packed);
                }
            }
        }

        /// <summary>
        /// Adding extra effects for focus for remaining (alive) teammates
        /// </summary>
        /// <param name="probe"></param>
        /// <param name="effect"></param>
        /// <param name="focused"></param>
        /// <exception cref="Exception"></exception>
        private void SpreadEffectToAllTeammates(RelEffectProbeComp probe, int effectEntity, EffectInstanceInfo effect, EcsPackedEntityWithWorld focused)
        {
            if (!probe.SourceOrigPacked.Unpack(out var origWorld, out var sourceOrig))
                throw new Exception("Stale source origin");

            if (!battleManagementService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                throw new Exception("No battle!");

            ref var mappings = ref mappingsPool.Value.Get(battleEntity);
           
            var matrixFilter = origWorld.Filter<RelationsMatrixComp>().End();
            var matrixPool = origWorld.GetPool<RelationsMatrixComp>();

            foreach (var matrixEntity in matrixFilter)
            {
                ref var matrixComp = ref matrixPool.Get(matrixEntity);
                foreach (var item in matrixComp.Matrix)
                {
                    // matching only one side of the key to avoid duplicates
                    if (!item.Key.Item1.EqualsTo(probe.SourceOrigPacked))
                        continue;

                    if (item.Value.EqualsTo(effect.EffectP2PEntity))
                    {
                        var initialTargetPacked = mappings.OriginToBattleMapping[item.Key.Item2];
                        if (!initialTargetPacked.Unpack(out _, out var initialTarget))
                            throw new Exception("Stale battle target");

                        if (deadTagPool.Value.Has(initialTarget))
                            continue;

                        AddFocus(
                            battleWorld.PackEntityWithWorld(effectEntity),
                            effect,
                            focused,
                            initialTargetPacked);

                        continue; // existing one, that was initially spawned
                    }

                    if (!item.Key.Item2.Unpack(out _, out var effectTargetOrig))
                        throw new Exception("Stale effect target");

                    var extraTargetPacked = mappings.OriginToBattleMapping[item.Key.Item2];
                    if (!extraTargetPacked.Unpack(out _, out var extraTarget))
                        throw new Exception("Stale battle target");

                    if (deadTagPool.Value.Has(extraTarget))
                        continue;

                    var extraEffectEntity = battleWorld.NewEntity();

                    effectDraftPool.Value.Add(extraEffectEntity);
                    ref var extraEffect = ref pool.Value.Add(extraEffectEntity);

                    extraEffect.EffectInfo = effect.EffectInfo;
                    extraEffect.EffectSource = effect.EffectSource;
                    extraEffect.Rule = effect.Rule;
                    extraEffect.EffectP2PEntity = item.Value;

                    AddFocus(
                        battleWorld.PackEntityWithWorld(extraEffectEntity), 
                        extraEffect, 
                        focused, 
                        extraTargetPacked);
                }
            }
        }

        private void AddFocus(
            EcsPackedEntityWithWorld effectEntity,
            EffectInstanceInfo effect,
            EcsPackedEntityWithWorld focused,
            EcsPackedEntityWithWorld actor)
        {
            if (!actor.Unpack(out _, out var actorEntity))
                throw new Exception("Stale actor entity");
            if (!focusPool.Value.Has(actorEntity))
                focusPool.Value.Add(actorEntity);

            ref var focus = ref focusPool.Value.Get(actorEntity);
            focus.EffectKey = effect.Rule.Key;
            focus.Focused = focused;
            focus.Actor = actor;
            focus.EffectEntity = effectEntity;

            focus.TurnsActive = effect.UsageLeft;

            draftTagPool.Value.Add(actorEntity);
        }

    }

}
