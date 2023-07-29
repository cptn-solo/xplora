using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
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
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEffectsFocusPendingComp>> draftTagPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;

        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (!battleManagementService.Value.BattleEntity.Unpack(out _, out var battleEntity))
                throw new Exception("No battle!");

            foreach (var entity in filter.Value)
            {
                ref var effect = ref pool.Value.Get(entity);
                ref var mappings = ref mappingsPool.Value.Get(battleEntity);

                if (effect.Rule.Key.RelationsEffectType == RelationsEffectType.AlgoRevenge)
                {
                    ref var probe = ref probePool.Value.Get(entity);

                    if (!probe.TurnEntity.Unpack(systems.GetWorld(), out var turnEntity))
                        throw new Exception("Stale Turn Entity");

                    // registering effect for the hero affected (in the battle world, to make it handy when needed) 
                    ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

                    var revengeEntity = ecsWorld.Value.NewEntity();
                    ref var revengeComp = ref revengePool.Value.Add(revengeEntity);
                    revengeComp.Focus = attackerRef.Packed;
                    revengeComp.RevengeBy = mappings.OriginToBattleMapping[probe.SourceOrigPacked];
                    revengeComp.RevengeFor = mappings.OriginToBattleMapping[probe.TargetOrigPacked];


                    if (!revengeComp.RevengeBy.Unpack(out _, out var revenger))
                        throw new Exception("Stale actor for focus");

                    // remember who is focused: (focus may exest from prev. turn, so will be overriden)
                    if (!focusPool.Value.Has(revenger))
                        focusPool.Value.Add(revenger);
                    
                    ref var focus = ref focusPool.Value.Get(revenger);                    
                    focus.EffectKey = effect.Rule.Key;
                    focus.Focused = attackerRef.Packed;
                    focus.Actor = revengeComp.RevengeBy;
                    focus.TurnEntity = probe.TurnEntity;
                    focus.EndRound = effect.EndRound;

                    draftTagPool.Value.Add(revenger);
                }
            }
        }
    }
}
