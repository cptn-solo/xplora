using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public partial class BattlePrepareRelEffectVisualSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<RelEffectProbeComp> probePool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationEffectInfo>> updatePool = default;
       
        // trying to catch effects here so both source and target are obvious
        private readonly EcsPoolInject<RelationEffectsPendingComp> pendingPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
                RelEffectProbeComp,
                EffectInstanceInfo
                >> filter = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag>,
            Exc<DeadTag>
            > playerTeamFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        public void Run(IEcsSystems systems)
        {
            if (!battleManagementService.Value.BattleEntity.Unpack(out _, out var battleEntity))
                throw new Exception("No battle!");
            
            foreach (var entity in filter.Value)
            {
                ref var probe = ref probePool.Value.Get(entity);
                ref var effect = ref pool.Value.Get(entity);
                ref var mappings = ref mappingsPool.Value.Get(battleEntity);

                var sourcePacked = mappings.OriginToBattleMapping[probe.SourceOrigPacked];
                if (!sourcePacked.Unpack(out var battleWorld, out var sourceParty))
                    throw new Exception("Stale effect source entity");

                var targetPacked = mappings.OriginToBattleMapping[probe.TargetOrigPacked];
                if (!targetPacked.Unpack(out _, out var targetParty))
                    throw new Exception("Stale effect target entity");                

                switch (effect.Rule.Key.RelationsEffectType)
                {                    
                    case RelationsEffectType.AlgoRevenge:
                        PrepareVisualForEffect(targetParty, sourceParty, ref effect);
                        break;
                    case RelationsEffectType.AlgoTarget:
                        {
                            // here we should add visual to all team mates as they are affected by the effect caster
                            foreach (var teammateEntity in playerTeamFilter.Value)
                                PrepareVisualForEffect(sourceParty, teammateEntity, ref effect);
                        }
                        break;
                    default:
                        PrepareVisualForEffect(sourceParty, targetParty, ref effect);
                        break;
                }
                effect.UsageLeft--;
            }
        }

        private void PrepareVisualForEffect(
            int sourceParty,
            int affectedParty, 
            ref EffectInstanceInfo effect)
        {
            ref var heroIcon = ref iconNamePool.Value.Get(sourceParty);
            var info = effect.Rule.DraftEffectInfo(effect.Rule.GetHashCode(), heroIcon.Name);
            effect.EffectInfo = info;

            ref var relEffects = ref relEffectsPool.Value.Get(affectedParty);
            relEffects.SetEffect(effect.Rule.Key, effect);

            var world = pendingPool.Value.GetWorld();
            var pendingVisualEntity = world.NewEntity();
            ref var pendingVisual = ref pendingPool.Value.Add(pendingVisualEntity);

            var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(sourceParty);
            var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(affectedParty);
            Debug.Log($"Pending move from {nameSource} to {nameSubject}");

            pendingVisual.EffectSource = world.PackEntityWithWorld(sourceParty);
            pendingVisual.EffectTarget = world.PackEntityWithWorld(affectedParty);
            pendingVisual.EffectInfo = info;

            if (!updatePool.Value.Has(affectedParty))
                updatePool.Value.Add(affectedParty);

        }
    }
}
