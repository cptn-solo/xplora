using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{

    public class BattlePrepareRevengeEffectSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<RelEffectProbeComp> probePool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<PrepareRevengeComp> revengePool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEffectsFocusPendingComp>> draftTagPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var effect = ref pool.Value.Get(entity);
                ref var mappings = ref ecsWorld.Value.GetHeroInstanceMappings();

                if (effect.Rule.Key.RelationsEffectType == RelationsEffectType.AlgoRevenge)
                {
                    ref var probe = ref probePool.Value.Get(entity);

                    if (!probe.TurnEntity.Unpack(systems.GetWorld(), out var turnEntity))
                        throw new Exception("Stale Turn Entity");

                    // registering effect for the hero affected (in the battle world, to make it handy when needed) 
                    ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                    var revengeEntity = ecsWorld.Value.NewEntity();
                    var revengerPacked = mappings.OriginToBattleMapping[probe.SourceOrigPacked];

                    if (!revengerPacked.Unpack(out var world, out var revenger))
                        throw new Exception("Stale actor for focus");

                    if (focusPool.Value.Has(revenger))
                    {   // instantly removing the existing focus effect as revenge is a high priority
                        ref var oldFocus = ref focusPool.Value.Get(revenger);
                        if (!oldFocus.EffectEntity.Unpack(out _, out var oldEffectEntity))
                            throw new Exception("Stale old focus effect entity");

                        world.RemoveRelEffectByType(revenger, oldFocus.EffectKey.RelationsEffectType, out var decrement);

                        if (decrement != null)
                        {
                            foreach (var item in decrement)
                                if (item.Unpack(out var origWorld, out var origEntity))
                                    origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                        }
                    }

                    ref var revengeComp = ref revengePool.Value.Add(revengeEntity);
                    revengeComp.Focus = attackerRef.Packed;
                    revengeComp.RevengeBy = revengerPacked;
                    revengeComp.RevengeFor = mappings.OriginToBattleMapping[probe.TargetOrigPacked];


                    // remember who is focused: (focus may exest from prev. turn, so will be overriden)
                    if (!focusPool.Value.Has(revenger))
                        focusPool.Value.Add(revenger);
                    
                    ref var focus = ref focusPool.Value.Get(revenger);                    
                    focus.EffectKey = effect.Rule.Key;
                    focus.Focused = attackerRef.Packed;
                    focus.Actor = revengeComp.RevengeBy;
                    focus.EffectEntity = world.PackEntityWithWorld(entity);

                    focus.TurnsActive = effect.UsageLeft;

                    draftTagPool.Value.Add(revenger);
                }
            }
        }
    }
}
