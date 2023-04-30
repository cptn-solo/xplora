using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Data;
using UnityEngine;
using Assets.Scripts.Services;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateUnitStrengthSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<IntValueComp<StrengthTag>> strengthPool = default;
        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<DataViewRef<BagedIconInfo>> pool = default;
        private readonly EcsPoolInject<UpdateTag<StrengthTag>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<IntValueComp<StrengthTag>, DataViewRef<BagedIconInfo>,
                UpdateTag<StrengthTag>>> filter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            if (filter.Value.GetEntitiesCount() == 0)
                return;

            if (!raidService.Value.RaidEntity.Unpack(ecsWorld.Value, out var raidEntity))
                throw new Exception("No Raid");

            ref var raidComp = ref raidPool.Value.Get(raidEntity);
            var config = raidComp.OppenentMembersSpawnConfig;

            foreach (var entity in filter.Value)
            {
                ref var strength = ref strengthPool.Value.Get(entity);
                ref var dataViewRef = ref pool.Value.Get(entity);
                var info = new BagedIconInfo()
                {
                    Icon = BundleIcon.Power,
                    IconColor = Color.yellow, 
                    BadgeText = $"{strength.Value}",
                    BackgroundColor = null,
                };

                foreach (var item in config.SortedSpawnRateInfo)
                {
                    if (item.Key <= strength.Value)
                    {
                        info.BackgroundColor = item.Value.TintColor;
                        break;
                    }
                }
                dataViewRef.DataView.SetInfo(info);

                updateTagPool.Value.Del(entity);
            }
        }
    }
}