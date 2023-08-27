using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamUpdateSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;
        private readonly EcsPoolInject<UpdateTag<HpTag>> updateHPTagPool = default;
        private readonly EcsPoolInject<UpdateTag<BarsInfoComp>> updateBarsInfoPool = default;


        private readonly EcsFilterInject<Inc<PlayerTeamTag, UpdateTag>> filter = default; 

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                updateTagPool.Value.Del(entity);

                updateHPTagPool.Value.Add(entity);
                updateBarsInfoPool.Value.Add(entity);
            }
        }
    }
}