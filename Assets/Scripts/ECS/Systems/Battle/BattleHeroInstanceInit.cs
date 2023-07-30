using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleHeroInstanceInit : BaseEcsSystem
    {
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;
        private readonly EcsPoolInject<HeroInstanceRef> heroInstanceRefPool = default;
        private readonly EcsPoolInject<HeroInstanceOriginRef> heroInstanceOriginRefPool = default;

        private readonly EcsPoolInject<FrontlineTag> frontlineTagPool = default;
        private readonly EcsPoolInject<BacklineTag> backlineTagPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default; // dynamic (hp)
        private readonly EcsPoolInject<BarsInfoComp> barsInfoPool = default; // mostly static (rates)
        private readonly EcsPoolInject<RangedTag> rangedTagPool = default;
        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<NameValueComp<IdleSpriteTag>> idleSpriteNamePool = default;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool = default;
        private readonly EcsPoolInject<DraftTag<BattleInfo>> draftBattleTagPool = default;
        private readonly EcsPoolInject<DraftTag<Hero>> draftHeroTagPool = default;

        private readonly EcsFilterInject<Inc<PositionComp, DraftTag<Hero>>> filter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleFieldComp, DraftTag<BattleInfo>>> battleFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            foreach (var entity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);

                InitBattleHeroInstance(
                    entity, battleWorld, heroConfigRef.HeroConfigPackedEntity);

                draftHeroTagPool.Value.Del(entity);
            }

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);
            battleInfo.State = BattleState.TeamsPrepared;

            roundShortageTagPool.Value.Add(battleEntity); // to create 1st rounds

            battleService.Value.NotifyBattleEventListeners(battleInfo);

            draftBattleTagPool.Value.Del(battleEntity);
        }

        private void InitBattleHeroInstance(
            int heroInstanceEntity,
            EcsWorld battleWorld,
            EcsPackedEntityWithWorld heroConfigPackedEntity)
        {

            if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero");

            ref var heroInstanceOriginRef = ref heroInstanceOriginRefPool.Value.Get(heroInstanceEntity);

            if (!heroInstanceOriginRef.Packed.Unpack(out var originWorld, out var originEntity))
                throw new Exception("No Origin");

            ref var heroInstanceRef = ref heroInstanceRefPool.Value.Add(heroInstanceEntity);
            heroInstanceRef.HeroInstancePackedEntity = battleWorld.PackEntityWithWorld(heroInstanceEntity);

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var heroConfig = ref libHeroPool.Get(heroConfigEntity);

            ref var position = ref positionPool.Value.Get(heroInstanceEntity);

            if (position.Position.Item2 == BattleLine.Front)
                frontlineTagPool.Value.Add(heroInstanceEntity);
            else
                backlineTagPool.Value.Add(heroInstanceEntity);

            ref var speedComp = ref CloneOrigin<IntValueComp<SpeedTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var defenceRateComp = ref CloneOrigin<IntValueComp<DefenceRateTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var critRateComp = ref CloneOrigin<IntValueComp<CritRateTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var accuracyRateComp = ref CloneOrigin<IntValueComp<AccuracyRateTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var dodgeRateComp = ref CloneOrigin<IntValueComp<DodgeRateTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var damageRangeComp = ref CloneOrigin<IntRangeValueComp<DamageRangeTag>, IntRange>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var healthComp = ref CloneOrigin<IntValueComp<HealthTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var hpComp = ref CloneOrigin<IntValueComp<HpTag>>(heroInstanceEntity,
                battleWorld, originWorld, originEntity);

            ref var relEffectsComp = ref relEffectsPool.Value.Add(heroInstanceEntity);
            relEffectsComp.CurrentEffects = new();

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Add(heroInstanceEntity);
            barsAndEffectsComp.HealthCurrent = hpComp.Value;
            barsAndEffectsComp.Health = healthComp.Value;
            barsAndEffectsComp.ActiveEffects = new();

            if (heroConfig.Ranged)
                rangedTagPool.Value.Add(heroInstanceEntity);

            ref var nameComp = ref namePool.Value.Add(heroInstanceEntity);
            nameComp.Name = heroConfig.Name;

            ref var iconNameComp = ref iconNamePool.Value.Add(heroInstanceEntity);
            iconNameComp.Name = heroConfig.IconName;

            ref var idleSpriteNameComp = ref idleSpriteNamePool.Value.Add(heroInstanceEntity);
            idleSpriteNameComp.Name = heroConfig.IdleSpriteName;

            ref var barsInfoComp = ref barsInfoPool.Value.Add(heroInstanceEntity);
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

        private ref T CloneOrigin<T>(int heroInstanceEntity, EcsWorld world, EcsWorld originWorld, int originEntity)
            where T : struct, IIntValue =>
            ref CloneOrigin<T, int>(heroInstanceEntity, world, originWorld, originEntity);

        private ref T CloneOrigin<T, V>(int heroInstanceEntity, EcsWorld world, EcsWorld originWorld, int originEntity)
            where T : struct, IValue<V>
        {
            var pool = world.GetPool<T>();
            ref var comp = ref pool.Add(heroInstanceEntity);
            ref var compOrigin = ref originWorld.GetPool<T>().Get(originEntity);
            comp.Value = compOrigin.Value;

            var buffPool = originWorld.GetPool<BuffComp<T>>();

            if (buffPool.Has(originEntity))
            {
                ref var buffComp = ref buffPool.Get(originEntity);
                comp.Combine(buffComp.Value / 100f);
            }

            return ref comp;
        }
    }
}
