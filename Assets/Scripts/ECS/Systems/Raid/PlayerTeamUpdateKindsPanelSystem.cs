using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class PlayerTeamUpdateKindsPanelSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<HeroKindBarInfo>> containerRefPool = default;
        private readonly EcsPoolInject<UpdateTag<HeroKindBarInfo>> updateTagPool = default;
        
        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, ItemsContainerRef<HeroKindBarInfo>, UpdateTag<HeroKindBarInfo>>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var containerRef = ref containerRefPool.Value.Get(entity);
                var world = filter.Value.GetWorld();

                var k1 = world.GetHeroKindBarInfo(entity, HeroKind.Asc);
                var k2 = world.GetHeroKindBarInfo(entity, HeroKind.Spi);
                var k3 = world.GetHeroKindBarInfo(entity, HeroKind.Int);
                var k4 = world.GetHeroKindBarInfo(entity, HeroKind.Cha);
                var k5 = world.GetHeroKindBarInfo(entity, HeroKind.Tem);
                var k6 = world.GetHeroKindBarInfo(entity, HeroKind.Con);
                var k7 = world.GetHeroKindBarInfo(entity, HeroKind.Str);
                var k8 = world.GetHeroKindBarInfo(entity, HeroKind.Dex);

                var infos = new HeroKindBarInfo[] {
                    k1, k2, k3, k4, k5, k6, k7, k8,
                };

                containerRef.Container.SetInfo(infos);

                updateTagPool.Value.Del(entity);
            }
        }

    }
}