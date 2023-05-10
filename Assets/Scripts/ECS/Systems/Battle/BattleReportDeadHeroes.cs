using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleReportDeadHeroes : IEcsRunSystem
    {
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> pool = default;
                
        private readonly EcsFilterInject<
            Inc<HeroInstanceOriginRefComp, ProcessedHeroTag, DeadTag>
            > filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var origin = ref pool.Value.Get(entity);
                if (!origin.Packed.Unpack(out var originWorld, out var originEntity))
                    throw new Exception("No Origin");

                var deadPool = originWorld.GetPool<DeadTag>();
                if (!deadPool.Has(originEntity))
                    deadPool.Add(originEntity);
                
                var relRefFilter = originWorld.Filter<RelationPartiesRef>().End();
                var relationRefPool = originWorld.GetPool<RelationPartiesRef>();               
                
                foreach (var relRefEntity in relRefFilter)
                {
                    var buff = ListPool<EcsPackedEntityWithWorld>.Get();
                    
                    ref var relRef = ref relationRefPool.Get(relRefEntity);

                    foreach (var party in relRef.Parties.Keys)
                        if (party.EqualsTo(origin.Packed))
                            buff.Add(party);
        
                    if (buff.Count > 0)
                        foreach (var key in buff)
                            relRef.Parties.Remove(key);

                    ListPool<EcsPackedEntityWithWorld>.Add(buff);
                }
            }
        }
    }
}
