using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
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
        private readonly EcsPoolInject<HeroInstanceOriginRef> heroInstanceOriginRefPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEffectsFocusPendingComp>> draftTagPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, HeroInstanceOriginRef>,            
            Exc<DeadTag>> teammateFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
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

            var world = pool.Value.GetWorld();

            ref var mappings = ref world.GetHeroInstanceMappings();
            ref var matrixComp = ref origWorld.GetRelationsMatrix();

            foreach (var teammateEntity in teammateFilter.Value)
            {
                ref var teammate = ref heroInstanceOriginRefPool.Value.Get(teammateEntity);
                
                if (teammate.Packed.EqualsTo(effect.EffectTarget))
                {
                    // original effect gets focus:
                    var targetPacked = mappings.OriginToBattleMapping[effect.EffectTarget];
                    AddFocus(
                        world.PackEntityWithWorld(effectEntity),
                        effect,
                        focused,
                        targetPacked);
                    continue; // no extra effect needed
                }
                
                // we need one extra effect for the target effect source himself, so pick the right p2p entity:
                var p2pEntity =
                    teammate.Packed.EqualsTo(effect.EffectSource) ?
                        // mirrored effect, same p2p used:
                        effect.EffectP2PEntity : 
                        // extra effect with new p2p, so find the applicable one:
                        matrixComp.Matrix[new RelationsMatrixKey(
                            effect.EffectSource,
                            teammate.Packed)];

                ExtraEffect(world, p2pEntity, mappings, teammate.Packed, effect, focused);
                
            }
        }

        private void ExtraEffect(EcsWorld battleWorld, 
            EcsPackedEntityWithWorld p2pEntity,
            HeroInstanceMapping mappings,
            EcsPackedEntityWithWorld extraTargetOrig,
            EffectInstanceInfo effect,
            EcsPackedEntityWithWorld focused)
        {
            {
                var extraEffectEntity = battleWorld.NewEntity();

                effectDraftPool.Value.Add(extraEffectEntity);
                ref var extraEffect = ref pool.Value.Add(extraEffectEntity);

                extraEffect.EffectInfo = effect.EffectInfo;
                extraEffect.EffectSource = effect.EffectSource;
                extraEffect.UsageLeft = effect.UsageLeft;
                extraEffect.EffectTarget = extraTargetOrig;
                extraEffect.Rule = effect.Rule;
                // one effect will be assigned to the effect source himself, but others will get it's p2p entity from the matrix:
                extraEffect.EffectP2PEntity = p2pEntity;

                var actor = mappings.OriginToBattleMapping[extraTargetOrig];

                AddFocus(
                    battleWorld.PackEntityWithWorld(extraEffectEntity),
                    extraEffect,
                    focused,
                    actor);
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

            if (!draftTagPool.Value.Has(actorEntity))
                draftTagPool.Value.Add(actorEntity);
        }

    }

}
