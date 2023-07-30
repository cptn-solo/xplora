using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

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
                var health = world.ReadIntValue<HealthTag>(entity);
                var currentHP = Mathf.Max((health - damage), 0);
                if (currentHP <= 0)
                {
                    if (!lethalPool.Value.Has(entity))
                        lethalPool.Value.Add(entity);

                    world.SetIntValue<HpTag>(0, entity);
                }
                world.SetIntValue<HpTag>(currentHP, entity);
            }
        }
    }
}
