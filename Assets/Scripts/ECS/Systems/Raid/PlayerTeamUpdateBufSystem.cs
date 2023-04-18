using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateBufSystem<T> : IEcsRunSystem
        where T : struct
    {
        private readonly EcsPoolInject<BuffComp<T>> buffPool = default;
        private readonly EcsPoolInject<ItemsContainerRef<BagedIconInfo>> containerRefPool = default;
        private readonly EcsPoolInject<UpdateBuffsTag<T>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<BagedIconInfo>,
                BuffComp<T>, UpdateBuffsTag<T>>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var buff = ref buffPool.Value.Get(entity);
                ref var containerRef = ref containerRefPool.Value.Get(entity);
                var info = new BagedIconInfo {
                        BadgeText = $"x{buff.Value/100}",
                        IconColor = buff.IconColor,
                        Icon = buff.Icon,
                        Id = buff.Id,
                };
                containerRef.Container.SetItemInfo(info);

                updateTagPool.Value.Del(entity);
            }
        }
    }
}