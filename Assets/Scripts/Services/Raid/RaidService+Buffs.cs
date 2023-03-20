using Leopotam.EcsLite;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Data;
using System;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Buffs
    {
        internal void BoostSpecOption(EcsPackedEntityWithWorld eventHero,
            SpecOption specOption, int factor)
        {
            if (!eventHero.Unpack(out var world, out var entity))
                return;

            switch (specOption)
            {
                case SpecOption.DamageRange:
                    {
                        ref var comp = ref world.GetPool<IntRangeValueComp<DamageRangeTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.CritRate:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<CritRateTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.DefenceRate:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<DefenceRateTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.AccuracyRate:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<AccuracyRateTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.DodgeRate:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<DodgeRateTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.Health:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<HealthTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                case SpecOption.Speed:
                    {
                        ref var comp = ref world.GetPool<IntValueComp<SpeedTag>>().Get(entity);
                        comp.Value += factor;
                        break;
                    }
                default:
                    break;
            }

        }

        internal void BoostTraitOption(EcsPackedEntityWithWorld eventHero,
            HeroTrait traitOption, int factor)
        {
            if (!eventHero.Unpack(out var world, out var entity))
                return;

            switch (traitOption)
            {
                case HeroTrait.Hidden:
                    IncrementTraitLevel<TraitHiddenTag>(factor, world, entity);
                    break;
                case HeroTrait.Purist:
                    IncrementTraitLevel<TraitPuristTag>(factor, world, entity);
                    break;
                case HeroTrait.Shrumer:
                    IncrementTraitLevel<TraitShrumerTag>(factor, world, entity);
                    break;
                case HeroTrait.Scout:
                    IncrementTraitLevel<TraitScoutTag>(factor, world, entity);
                    break;
                case HeroTrait.Tidy:
                    IncrementTraitLevel<TraitTidyTag>(factor, world, entity);
                    break;
                case HeroTrait.Soft:
                    IncrementTraitLevel<TraitSoftTag>(factor, world, entity);
                    break;
                default:
                    break;
            }
        }

        private static void IncrementTraitLevel<T>(int factor, EcsWorld world, int entity)
        {
            var pool = world.GetPool<IntValueComp<T>>();

            if (!pool.Has(entity))
                pool.Add(entity);

            ref var comp = ref pool.Get(entity);
            comp.Value += factor;
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
                        buff.Icon = BundleIcon.ShieldCrossed;
                        buff.IconColor = Color.cyan;

                        if (!updatePool.Has(entity))
                            updatePool.Add(entity);
                    }
                    break;
                case SpecOption.Health:
                    {
                        var updatePool = world.GetPool<UpdateHPTag>();
                        //hp = max(health, hp*=2)
                        var healthPool = world.GetPool<IntValueComp<HealthTag>>();
                        ref var healthComp = ref healthPool.Get(entity);

                        var hpPool = world.GetPool<IntValueComp<HpTag>>();
                        ref var hpComp = ref hpPool.Get(entity);

                        //HP buff changed from x2 to full HP recovery on event:
                        //hpComp.Value = Mathf.Min(healthComp.Value, hpComp.Value * 2);
                        hpComp.Value = healthComp.Value;

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
                        comp.Usages += 3; // 3 turns without stamina usage

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

