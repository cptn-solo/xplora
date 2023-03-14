using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class RaidBalanceUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<BagedIconInfo>> containerRefPool = default;
        private readonly EcsPoolInject<RaidComp> raidPool = default;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updatePool = default;

        private readonly EcsFilterInject<
            Inc<RaidComp, ItemsContainerRef<BagedIconInfo>, UpdateAssetBalanceTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var raidComp = ref raidPool.Value.Get(entity);
                ref var containerRef = ref containerRefPool.Value.Get(entity);

                containerRef.Container.Reset();

                foreach (var asset in raidComp.Assets)
                {
                    var item = new BagedIconInfo
                    {
                        Icon = asset.AssetType.Icon(),
                        BadgeText = $"{asset.Count}",
                        IconColor = Color.yellow
                    };
                    containerRef.Container.SetItemInfo(item);
                }


                updatePool.Value.Del(entity);
            }
        }
    }
}