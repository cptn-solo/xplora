using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{

    public class PlayerTeamUpdateHPSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<HPComp> hpCompPool;
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> containerRefPool;
        private readonly EcsPoolInject<UpdateHPTag> updateTagPool;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<BarInfo>, UpdateHPTag>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var hpComp = ref hpCompPool.Value.Get(entity);
                ref var healthComp = ref healthCompPool.Value.Get(entity);
                ref var containerRef = ref containerRefPool.Value.Get(entity);
                var infos = new BarInfo[] {
                    BarInfo.EmptyBarInfo(0, $"HP: {hpComp.Value}", Color.red, (float)hpComp.Value / healthComp.Value),
                };
                containerRef.Container.SetItems(infos);

                updateTagPool.Value.Del(entity);
            }
        }
    }
}