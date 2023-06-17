using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePrepareTargetEffectSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<RelEffectProbeComp> probePool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<PrepareTargetComp> targetPool = default;
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;
        
        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        public void Run(IEcsSystems systems)
        {
            if (!battleManagementService.Value.BattleEntity.Unpack(out _, out var battleEntity))
                throw new Exception("No battle!");

            foreach (var entity in filter.Value)
            {
                ref var effect = ref pool.Value.Get(entity);
                ref var mappings = ref mappingsPool.Value.Get(battleEntity);

                if (effect.Rule.Key.RelationsEffectType == RelationsEffectType.AlgoTarget)
                {
                    ref var probe = ref probePool.Value.Get(entity);

                    if (!probe.TurnEntity.Unpack(systems.GetWorld(), out var turnEntity))
                        throw new Exception("Stale Turn Entity");

                    // registering effect for the hero affected (in the battle world, to make it handy when needed) 

                    ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);
                    
                    var targetEntity = ecsWorld.Value.NewEntity();
                    ref var targetComp = ref targetPool.Value.Add(targetEntity);
                    targetComp.Focus = attackerRef.Packed;
                    targetComp.TargetBy = mappings.OriginToBattleMapping[probe.SourceOrigPacked];
                    targetComp.TargetFor = mappings.OriginToBattleMapping[probe.TargetOrigPacked];

                    var focusEntity = ecsWorld.Value.NewEntity();
                    // remember who is focused:
                    ref var focus = ref focusPool.Value.Add(focusEntity);
                    focus.EffectKey = effect.Rule.Key;
                    focus.Focused = attackerRef.Packed;
                    focus.Actor = targetComp.TargetFor;
                    focus.EndRound = effect.EndRound;

                }
            }
        }
    }
}
