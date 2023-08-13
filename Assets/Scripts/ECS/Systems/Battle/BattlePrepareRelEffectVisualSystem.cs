using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public partial class BattlePrepareRelEffectVisualSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
       
        // trying to catch effects here so both source and target are obvious
        private readonly EcsPoolInject<RelationEffectsPendingComp> pendingPool = default;

        private readonly EcsFilterInject<
            Inc<
                DraftTag<EffectInstanceInfo>,
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

                var sourcePacked = mappings.OriginToBattleMapping[effect.EffectSource];
                if (!sourcePacked.Unpack(out var battleWorld, out var sourceParty))
                    throw new Exception("Stale effect source entity");

                var targetPacked = mappings.OriginToBattleMapping[effect.EffectTarget];
                if (!targetPacked.Unpack(out _, out var targetParty))
                    throw new Exception("Stale effect target entity");                

                PrepareVisualForEffect(sourceParty, targetParty, ref effect, entity);                
            }
        }

        private void PrepareVisualForEffect(
            int sourceParty,
            int affectedParty, 
            ref EffectInstanceInfo effect,
            int entity)
        {
            var world = pendingPool.Value.GetWorld();

            ref var heroIcon = ref iconNamePool.Value.Get(sourceParty);
            var info = effect.Rule.DraftEffectInfo(effect.Rule.GetHashCode(), heroIcon.Name);
            effect.EffectInfo = info;

            var pendingVisualEntity = world.NewEntity();
            ref var pendingVisual = ref pendingPool.Value.Add(pendingVisualEntity);

            var nameSource = world.ReadValue<NameValueComp<NameTag>, string>(sourceParty);
            var nameSubject = world.ReadValue<NameValueComp<NameTag>, string>(affectedParty);
            Debug.Log($"Pending move from {nameSource} to {nameSubject}");

            pendingVisual.EffectSource = world.PackEntityWithWorld(sourceParty);
            pendingVisual.EffectTarget = world.PackEntityWithWorld(affectedParty);
            pendingVisual.EffectInfo = info;
        }
    }
}
