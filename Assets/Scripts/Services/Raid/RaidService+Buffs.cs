using Leopotam.EcsLite;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Data;
using System;
using UnityEngine;
using Assets.Scripts.ECS;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Buffs
    {
        internal void BoostSpecOption(EcsPackedEntityWithWorld eventHero,
            SpecOption specOption, int factor)
        {
            if (!eventHero.Unpack(out var world, out var entity))
                return;

            var _ = specOption switch
            {
                SpecOption.DamageRange => world.IncrementValue<IntRangeValueComp<DamageRangeTag>, IntRange>(factor, entity),
                SpecOption.CritRate => world.IncrementIntValue<CritRateTag>(factor, entity),
                SpecOption.DefenceRate => world.IncrementIntValue<DefenceRateTag>(factor, entity),
                SpecOption.AccuracyRate => world.IncrementIntValue<AccuracyRateTag>(factor, entity),
                SpecOption.DodgeRate => world.IncrementIntValue<DodgeRateTag>(factor, entity),
                SpecOption.Health => world.IncrementIntValue<HealthTag>(factor, entity),
                SpecOption.Speed => world.IncrementIntValue<SpeedTag>(factor, entity),
                _ => false
            };
        }

        internal void BoostTraitOption(EcsPackedEntityWithWorld eventHero,
            HeroTrait traitOption, int factor)
        {
            if (!eventHero.Unpack(out var world, out var entity))
                return;

            var _ = traitOption switch
            {
                HeroTrait.Hidden => world.IncrementIntValue<TraitHiddenTag>(factor, entity),
                HeroTrait.Purist => world.IncrementIntValue<TraitPuristTag>(factor, entity),
                HeroTrait.Shrumer => world.IncrementIntValue<TraitShrumerTag>(factor, entity),
                HeroTrait.Scout => world.IncrementIntValue<TraitScoutTag>(factor, entity),
                HeroTrait.Tidy => world.IncrementIntValue<TraitTidyTag>(factor, entity),
                HeroTrait.Soft => world.IncrementIntValue<TraitSoftTag>(factor, entity),
                _ => false
            };
        }        

        private void BoostEcsTeamMemberSpecOption(
            EcsPackedEntityWithWorld heroEntity, SpecOption specOption, int factor)
        {
            BoostSpecOption(
                currentEventInfo.Value.HeroEntity,
                specOption,
                factor);

            if (!heroEntity.Unpack(out var world, out var entity))
                throw new Exception("No Hero instance");

            var pool = world.GetPool<UpdateTag>();
            if (!pool.Has(entity))
                pool.Add(entity);
        }

        private void BoostEcsNextBattleSpecOption(
            EcsPackedEntityWithWorld heroEntity, SpecOption specOption, int factor)
        {
            // TODO: Add Spec Option Boost for the next battle
            //1. pick player team hero for eventHero
            if (!RaidEntity.Unpack(ecsWorld, out var raidEntity))
                throw new Exception("No Raid");

            if (!heroEntity.Unpack(out var world, out var entity))
                throw new Exception("No Hero instance");

            switch (specOption)
            {
                case SpecOption.DamageRange:
                    {
                        var updatePool = world.GetPool<UpdateBuffsTag<IntRangeValueComp<DamageRangeTag>>>();
                        //buff*=2
                        var pool = world.GetPool<BuffComp<IntRangeValueComp<DamageRangeTag>>>();
                        if (!pool.Has(entity))
                            pool.Add(entity);

                        ref var buff = ref pool.Get(entity);
                        buff.Value += ((buff.Value == 0 ? 100 : 0) + factor);
                        buff.Icon = BundleIcon.Sword;
                        buff.IconColor = Color.cyan;

                        if (!updatePool.Has(entity))
                            updatePool.Add(entity);
                    }
                    break;
                case SpecOption.Health:
                    {
                        var updatePool = world.GetPool<UpdateTag>();
                        //hp = max(health, hp*=2)

                        var hpPool = world.GetPool<IntValueComp<HpTag>>();
                        ref var hpComp = ref hpPool.Get(entity);

                        //HP buff changed from x2 to full HP recovery on event:
                        //hpComp.Value = Mathf.Min(healthComp.Value, hpComp.Value * 2);
                        hpComp.Value = world.ReadIntValue<HealthTag>(entity);

                        if (!updatePool.Has(entity))
                            updatePool.Add(entity);
                    }
                    break;
                case SpecOption.UnlimitedStaminaTag:
                    {
                        var updatePool = world.GetPool<UpdateTag>();

                        var pool = world.GetPool<BuffComp<NoStaminaDrainBuffTag>>();
                        if (!PlayerEntity.Unpack(out _, out var playerEntity))
                            throw new Exception("No Player entity");

                        if (!pool.Has(playerEntity))
                            pool.Add(playerEntity);

                        ref var comp = ref pool.Get(playerEntity);
                        comp.Usages += factor; // 3 turns without stamina usage

                        if (!updatePool.Has(playerEntity))
                            updatePool.Add(playerEntity);

                    }
                    break;
                case SpecOption.DefenceRate:
                case SpecOption.CritRate:
                case SpecOption.AccuracyRate:
                case SpecOption.DodgeRate:
                case SpecOption.Speed:
                default:
                    break;
            }

            //2. add boostComponent of specOption to the raid hero entitity instance
            // 

        }
    }
}

