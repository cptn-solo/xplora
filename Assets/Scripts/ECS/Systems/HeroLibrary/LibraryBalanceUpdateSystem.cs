using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryBalanceUpdateSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<BagedIconInfo>> containerRefPool = default;
        private readonly EcsPoolInject<Team> teamPool = default;
        private readonly EcsPoolInject<UpdateAssetBalanceTag> updatePool = default;

        private readonly EcsFilterInject<
            Inc<Team, ItemsContainerRef<BagedIconInfo>, UpdateAssetBalanceTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
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
                        IconColor = Color.yellow,
                        Id = asset.Id,
                    };
                    containerRef.Container.SetItemInfo(item);
                }


                updatePool.Value.Del(entity);
            }
        }
    }
}

