using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateDebufSystem<T> : IEcsRunSystem
        where T : struct
    {
        private readonly EcsPoolInject<ItemsContainerRef<BagedIconInfo>> containerRefPool;
        private readonly EcsPoolInject<BuffComp<T>> buffPool;
        private readonly EcsPoolInject<DebuffTag<T>> debuffTagPool;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<BagedIconInfo>,
                BuffComp<T>, DebuffTag<T>>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var buff = ref buffPool.Value.Get(entity);
                ref var containerRef = ref containerRefPool.Value.Get(entity);
                var info = new BagedIconInfo
                {
                    Icon = buff.Icon
                };
                containerRef.Container.RemoveItem(info);

                buffPool.Value.Del(entity);
                debuffTagPool.Value.Del(entity);
            }
        }
    }
}