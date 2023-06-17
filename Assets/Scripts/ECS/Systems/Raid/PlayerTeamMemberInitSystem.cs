using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamMemberInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;

        private readonly EcsPoolInject<IntRangeValueComp<DamageRangeTag>> damageRangeCompPool = default;
        private readonly EcsPoolInject<IntValueComp<DefenceRateTag>> defenceRateCompPool = default;
        private readonly EcsPoolInject<IntValueComp<CritRateTag>> critRateCompPool = default;
        private readonly EcsPoolInject<IntValueComp<AccuracyRateTag>> accuracyRateCompPool = default;
        private readonly EcsPoolInject<IntValueComp<DodgeRateTag>> dodgeRateCompPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthCompPool = default;
        private readonly EcsPoolInject<IntValueComp<SpeedTag>> speedCompPool = default;
        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;

        private readonly EcsPoolInject<IntValueComp<HpTag>> hpCompPool = default;

        private readonly EcsPoolInject<BarsInfoComp> barsInfoPool = default; // mostly static (rates)

        private readonly EcsFilterInject<Inc<HeroConfigRef, PlayerTeamTag>> filter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                ref var damageRangeComp = ref damageRangeCompPool.Value.Add(entity);
                damageRangeComp.Value = new IntRange(heroConfig.DamageMin, heroConfig.DamageMax);

                ref var defenceRateComp = ref defenceRateCompPool.Value.Add(entity);
                defenceRateComp.Value = heroConfig.DefenceRate;

                ref var critRateComp = ref critRateCompPool.Value.Add(entity);
                critRateComp.Value = heroConfig.CriticalHitRate;

                ref var accuracyRateComp = ref accuracyRateCompPool.Value.Add(entity);
                accuracyRateComp.Value = heroConfig.AccuracyRate;

                ref var dodgeRateComp = ref dodgeRateCompPool.Value.Add(entity);
                dodgeRateComp.Value = heroConfig.DodgeRate;

                ref var healthComp = ref healthCompPool.Value.Add(entity);
                healthComp.Value = heroConfig.Health;

                ref var speedComp = ref speedCompPool.Value.Add(entity);
                speedComp.Value = heroConfig.Speed;

                ref var hpComp = ref hpCompPool.Value.Add(entity);
                hpComp.Value = heroConfig.Health;

                ref var nameComp = ref namePool.Value.Add(entity);
                nameComp.Name = heroConfig.Name;

                ref var iconNameComp = ref iconNamePool.Value.Add(entity);
                iconNameComp.Name = heroConfig.IconName;

                ref var barsInfoComp = ref barsInfoPool.Value.Add(entity);
                barsInfoComp.Name = heroConfig.Name;
                barsInfoComp.Health = healthComp.Value;
                barsInfoComp.Speed = speedComp.Value;
                barsInfoComp.DamageMax = damageRangeComp.Value.MaxRate;
                barsInfoComp.DefenceRate = defenceRateComp.Value;
                barsInfoComp.AccuracyRate = accuracyRateComp.Value;
                barsInfoComp.DodgeRate = dodgeRateComp.Value;
                barsInfoComp.CriticalHitRate = critRateComp.Value;

                barsInfoComp.Generate();


            }
        }
    }
}