using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleProcessDamageSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<LethalTag> lethalPool = default;

        private readonly EcsFilterInject<
            Inc<HeroConfigRef,
                IntValueComp<DamageTag>,
                IntValueComp<HpTag>,
                IntValueComp<HealthTag>>
            > filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var entity in filter.Value)
            {
                var damage = world.ReadIntValue<DamageTag>(entity);
                var currentHP = world.IncrementIntValue<HpTag>(-damage, entity);

                if (currentHP <= 0)
                {
                    if (!lethalPool.Value.Has(entity))
                        lethalPool.Value.Add(entity);

                    world.SetIntValue<HpTag>(0, entity);
                }
            }
        }
    }
}
