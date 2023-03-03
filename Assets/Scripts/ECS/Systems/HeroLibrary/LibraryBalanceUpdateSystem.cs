using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryBalanceUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<BagedIconInfo>> containerRefPool;
        private readonly EcsPoolInject<Team> teamPool;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updatePool;

        private readonly EcsFilterInject<
            Inc<Team, ItemsContainerRef<BagedIconInfo>, UpdateAssetBalanceTag>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var team = ref teamPool.Value.Get(entity);
                ref var containerRef = ref containerRefPool.Value.Get(entity);

                containerRef.Container.Reset();

                foreach (var asset in team.Assets)
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

