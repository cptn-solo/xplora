using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UpdateTag> updateTagPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<SpeedComp> speedCompPool;
        private readonly EcsPoolInject<HealthComp> healthCompPool;
        private readonly EcsPoolInject<HPComp> hpCompPool;

        private readonly EcsFilterInject<Inc<PlayerTeamTag, HeroConfigRefComp, UpdateTag>> filter; 

        public void Run(IEcsSystems systems)
        {

            foreach (var playerTeamMemberEntity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(playerTeamMemberEntity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                ref var speedComp = ref speedCompPool.Value.Get(playerTeamMemberEntity);
                speedComp.Value = heroConfig.Speed;

                ref var healthComp = ref healthCompPool.Value.Get(playerTeamMemberEntity);
                healthComp.Value = heroConfig.Health;

                //ref var hpComp = ref hpCompPool.Value.Get(playerTeamMemberEntity);
                //hpComp.Value = heroConfig.Health;

                updateTagPool.Value.Del(playerTeamMemberEntity);
            }
        }
    }
}