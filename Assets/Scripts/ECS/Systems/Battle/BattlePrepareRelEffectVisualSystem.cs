using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public partial class BattlePrepareRelEffectVisualSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceMapping> mappingsPool = default;
        private readonly EcsPoolInject<EffectInstanceInfo> pool = default;
        private readonly EcsPoolInject<RelEffectProbeComp> probePool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationEffectInfo>> updatePool = default;

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
            if (!battleManagementService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
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
                        PrepareVisualForEffect(probe.TargetOrigPacked, sourceParty, ref effect);
                        break;
                    case RelationsEffectType.AlgoTarget:
                        {
                            // here we should add visual to all team mates as they are affected by the effect caster
                            foreach (var teammateEntity in playerTeamFilter.Value)
                                PrepareVisualForEffect(probe.SourceOrigPacked, teammateEntity, ref effect);
                        }
                        break;
                    default:
                        PrepareVisualForEffect(probe.SourceOrigPacked, targetParty, ref effect);
                        break;
                }
                effect.UsageLeft--;
            }
        }

        private void PrepareVisualForEffect(EcsPackedEntityWithWorld origIconParty, int affectedParty, ref EffectInstanceInfo effect)
        {
            if (!origIconParty.Unpack(out var origWorld, out var iconEntity))
                throw new Exception("Stale origin effect source!");

            var heroIconPool = origWorld.GetPool<NameValueComp<IconTag>>();
            ref var heroIcon = ref heroIconPool.Get(iconEntity);
            var info = effect.Rule.DraftEffectInfo(effect.Rule.GetHashCode(), heroIcon.Name);
            effect.EffectInfo = info;

            ref var relEffects = ref relEffectsPool.Value.Get(affectedParty);
            relEffects.SetEffect(effect.Rule.Key, effect);

            if (!updatePool.Value.Has(affectedParty))
                updatePool.Value.Add(affectedParty);

        }
    }
}
