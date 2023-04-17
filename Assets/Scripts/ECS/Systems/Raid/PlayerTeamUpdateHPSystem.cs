using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateHPSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> containerRefPool = default;
        private readonly EcsPoolInject<UpdateTag<HpTag>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<BarInfo>, UpdateTag<HpTag>>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var containerRef = ref containerRefPool.Value.Get(entity);
                var world = filter.Value.GetWorld();
                var hp = world.ReadIntValue<HpTag>(entity);
                var health = world.ReadIntValue<HealthTag>(entity);
                var infos = new BarInfo[] {
                    BarInfo.EmptyBarInfo(0, $"HP: {hp}", Color.red, (float)hp / health),
                };
                containerRef.Container.SetInfo(infos);

                updateTagPool.Value.Del(entity);
            }
        }
    }
}