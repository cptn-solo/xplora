using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueDiedHeroesSystem : IEcsRunSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<RetiredTag> retiredTagPool = default;
        private readonly EcsPoolInject<HeroInstanceRefComp> pool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundPool = default;

        private readonly EcsFilterInject<
            Inc<HeroInstanceRefComp, DeadTag>,
            Exc<RetiredTag>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in filter.Value)
            {
                ref var instanceRef = ref pool.Value.Get(entity);
                DequeueHero(instanceRef.Packed);
                retiredTagPool.Value.Add(entity);
            }
        }

        private void DequeueHero(EcsPackedEntityWithWorld heroInstancePackedEntity)
        {
            var filter = ecsWorld.Value
                .Filter<BattleRoundInfo>()
                .End();

            foreach (var roundEntity in filter)
            {
                ref var roundInfo = ref roundPool.Value.Get(roundEntity);

                var buffer = ListPool<RoundSlotInfo>.Get();

                buffer.AddRange(roundInfo.QueuedHeroes);
                var idx = buffer.FindIndex(x =>
                    x.HeroInstancePackedEntity.Equals(heroInstancePackedEntity));

                if (idx >= 0)
                    buffer.RemoveAt(idx);

                roundInfo.QueuedHeroes = buffer.ToArray();

                ListPool<RoundSlotInfo>.Add(buffer);
            }
        }
    }
}
