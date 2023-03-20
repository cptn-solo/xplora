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

        private readonly EcsFilterInject<Inc<PlayerTeamTag, UpdateTag>> filter = default; 

        public void Run(IEcsSystems systems)
        {
            foreach (var playerTeamMemberEntity in filter.Value)
            {
                updateTagPool.Value.Del(playerTeamMemberEntity);
                updateHPTagPool.Value.Add(playerTeamMemberEntity);
            }
        }
    }
}