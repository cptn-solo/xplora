using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.Data;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateUnitStrengthSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<StrengthComp> strengthPool;
        private readonly EcsPoolInject<DataViewRef<BagedIconInfo>> pool;
        private readonly EcsPoolInject<UpdateTag<StrengthComp>> updateTagPool;

        private readonly EcsFilterInject<
            Inc<StrengthComp, DataViewRef<BagedIconInfo>,
                UpdateTag<StrengthComp>>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var strength = ref strengthPool.Value.Get(entity);
                ref var dataViewRef = ref pool.Value.Get(entity);
                var info = new BagedIconInfo()
                {
                    Icon = BundleIcon.Power,
                    IconColor = Color.yellow, 
                    BadgeText = $"{strength.Value}",
                    BackgroundColor = null
                };
                var config = OpponentTeamMemberSpawnConfig.DefaultConfig;
                foreach (var item in config.OveralStrengthLevels.OrderByDescending(x => x.Key))
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