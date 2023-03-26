using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class RelationsEventSystem : IEcsRunSystem
    {        
        private readonly EcsPoolInject<RelationsEventInfo> eventInfoPool = default;
        
        private readonly EcsFilterInject<
            Inc<PlayerComp>,
            Exc<DummyBuff>> triggerFilter = default; // test

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in triggerFilter.Value) {
                CreateEvent(entity);
                // test:
                systems.GetWorld().GetPool<DummyBuff>().Add(entity);
            }           
        }
        private void CreateEvent(int entity) // test implementation
        {
            ref var info = ref eventInfoPool.Value.Add(entity);
            var score = 5;
            var delta = 1;
            var scoreMax = 30;
            var items = new Tuple<HeroKind, int, int, int, int>[] {
                new (HeroKind.Tem, 5, 1, 0, 1),
                new (HeroKind.Con, 3, 2, 10, 0),
                };
            info.EventTitle = $"Score {score}: Good";
            info.SrcIconName = "Heroes/Icons/Мечник";
            info.TgtIconName = "Heroes/Icons/Копейщица";
            info.ActionTitles = new[] { $"OK" };
            info.ScoreInfo = new() { 
                ScoreSign = -1 , 
                ScoreInfo = new() { 
                    Title = $"{score}",
                    Value = Mathf.Min(1, (float)score/scoreMax),
                    Delta = Mathf.Min(1, (float)delta/scoreMax),
                    Color = Color.green,
                    DeltaColor = Color.yellow,
                } };
            var eventItems = new RelationEventItemInfo[2];

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                eventItems[i] = new()
                {
                    ItemTitle = item.Item1.Name(),
                    SrcBarInfo = new()
                    {
                        Title = $"{item.Item2}",
                        Value = Mathf.Min(1, (float)item.Item2 / scoreMax),
                        Delta = Mathf.Min(1, (float)item.Item3 / scoreMax),
                        Color = Color.red,
                        DeltaColor = Color.cyan,
                    },
                    TgtBarInfo = new()
                    {
                        Title = $"{item.Item4}",
                        Value = Mathf.Min(1, (float)item.Item4 / scoreMax),
                        Delta = Mathf.Min(1, (float)item.Item5 / scoreMax),
                        Color = Color.red,
                        DeltaColor = Color.cyan,
                    }

                };
                
            }

            info.EventItems = eventItems;

        }
    }
}