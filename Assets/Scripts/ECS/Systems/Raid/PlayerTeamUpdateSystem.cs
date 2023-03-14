using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<UpdateHPTag> updateHPTagPool = default;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;
        private readonly EcsPoolInject<SpeedComp> speedCompPool = default;
        private readonly EcsPoolInject<HealthComp> healthCompPool = default;

        private readonly EcsFilterInject<Inc<PlayerTeamTag, HeroConfigRefComp, UpdateTag>> filter = default; 

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

                updateTagPool.Value.Del(playerTeamMemberEntity);
                updateHPTagPool.Value.Add(playerTeamMemberEntity);
            }
        }
    }
}