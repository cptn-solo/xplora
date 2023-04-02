using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class RelationsEventTriggerSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<RelationsEventInfo> eventInfoPool = default;
        private readonly EcsPoolInject<DraftTag<RelationEventItemInfo>> draftTagPool = default;

        private readonly EcsFilterInject<
            Inc<IntValueComp<HeroKindRSDTag>, NameValueComp<NameTag>>> filter = default;
        private readonly EcsFilterInject<Inc<FieldCellComp, VisitCellComp>> visitFilter = default;
        private readonly EcsFilterInject<Inc<RelationsEventInfo>> existingFilter = default;

        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;

        public void Run(IEcsSystems systems)
        {
            if (existingFilter.Value.GetEntitiesCount() > 0)
                return;

            foreach (var visitEntity in visitFilter.Value)
            {
                ref var relationsConfig = ref heroLibraryService.Value.HeroRelationsConfig;

                var buff = ListPool<Tuple<int, float>>.Get();
                var total = 0f;
                foreach (var entity in filter.Value)
                {
                    var rsd = systems.GetWorld().ReadIntValue<HeroKindRSDTag>(entity);
                    rsd = Mathf.Abs(rsd);
                
                    var rate = relationsConfig.TriggerSpawnRate(rsd);
                
                    if (rate > 0f) // only non-zero rates are counted
                        buff.Add(new Tuple<int, float>(entity, total += rate));
                }

                var probe = Random.Range(0f, 100f);
                var winner = -1;
                foreach (var item in buff)
                {
                    if (item.Item2 < probe)
                        continue;

                    winner = item.Item1;
                
                    break;
                }
            
                if (winner >= 0)
                {
                    
                    ref var info = ref eventInfoPool.Value.Add(visitEntity);
                    info.SourceEntity = systems.GetWorld().PackEntity(winner);
                    
                    draftTagPool.Value.Add(visitEntity);
                    
                    ref var nameComp = ref namePool.Value.Get(winner);
                    Debug.Log($"Relations event triggered by {nameComp.Name}");
                }
                else
                {
                    Debug.Log($"Relations event missed");
                }

                ListPool<Tuple<int, float>>.Add(buff);

            }

            
        }
    }
}