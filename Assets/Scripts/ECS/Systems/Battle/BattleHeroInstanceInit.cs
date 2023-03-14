using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleHeroInstanceInit : IEcsRunSystem
    {
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;
        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool = default;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool = default;
        private readonly EcsPoolInject<DamageRangeComp> damageRangeCompPool = default;
        private readonly EcsPoolInject<SpeedComp> speedPool = default;
        private readonly EcsPoolInject<FrontlineTag> frontlineTagPool = default;
        private readonly EcsPoolInject<BacklineTag> backlineTagPool = default;
        private readonly EcsPoolInject<HealthComp> healthPool = default;
        private readonly EcsPoolInject<HPComp> hpPool = default;
        private readonly EcsPoolInject<EffectsComp> effectsPool = default;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool = default;
        private readonly EcsPoolInject<RangedTag> rangedTagPool = default;
        private readonly EcsPoolInject<NameComp> namePool = default;
        private readonly EcsPoolInject<IconName> iconNamePool = default;
        private readonly EcsPoolInject<IdleSpriteName> idleSpriteNamePool = default;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool = default;
        private readonly EcsPoolInject<DraftTag<BattleInfo>> draftBattleTagPool = default;
        private readonly EcsPoolInject<DraftTag<Hero>> draftHeroTagPool = default;

        private readonly EcsFilterInject<Inc<PositionComp, DraftTag<Hero>>> filter = default;
        private readonly EcsFilterInject<Inc<BattleInfo, BattleFieldComp, DraftTag<BattleInfo>>> battleFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
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

            ref var speedComp = ref speedPool.Value.Add(heroInstanceEntity);
            ref var speedCompOrigin = ref originWorld.GetPool<SpeedComp>().Get(originEntity);
            speedComp.Value = speedCompOrigin.Value;

            ref var damageRangeComp = ref damageRangeCompPool.Value.Add(heroInstanceEntity);

            damageRangeComp.Min = heroConfig.DamageMin;
            damageRangeComp.Max = heroConfig.DamageMax;

            var damageRangeBuffPool = originWorld.GetPool<BuffComp<DamageRangeComp>>();

            if (damageRangeBuffPool.Has(originEntity))
            {
                ref var damageBuff = ref damageRangeBuffPool.Get(originEntity);
                damageRangeComp.Min *= damageBuff.Value / 100;
                damageRangeComp.Max *= damageBuff.Value / 100;
            }

            ref var healthComp = ref healthPool.Value.Add(heroInstanceEntity);
            ref var healthCompOrigin = ref originWorld.GetPool<HealthComp>().Get(originEntity);
            healthComp.Value = healthCompOrigin.Value;

            ref var hpComp = ref hpPool.Value.Add(heroInstanceEntity);
            ref var hpCompOrigin = ref originWorld.GetPool<HPComp>().Get(originEntity);
            hpComp.Value = hpCompOrigin.Value;

            ref var effectsComp = ref effectsPool.Value.Add(heroInstanceEntity);
            effectsComp.ActiveEffects = new();

            ref var barsAndEffectsComp = ref barsAndEffectsPool.Value.Add(heroInstanceEntity);
            barsAndEffectsComp.HealthCurrent = hpComp.Value;
            barsAndEffectsComp.Health = healthComp.Value;
            barsAndEffectsComp.ActiveEffects = effectsComp.ActiveEffects;

            if (heroConfig.Ranged)
                rangedTagPool.Value.Add(heroInstanceEntity);

            ref var nameComp = ref namePool.Value.Add(heroInstanceEntity);
            nameComp.Name = heroConfig.Name;

            ref var iconNameComp = ref iconNamePool.Value.Add(heroInstanceEntity);
            iconNameComp.Name = heroConfig.IconName;

            ref var idleSpriteNameComp = ref idleSpriteNamePool.Value.Add(heroInstanceEntity);
            idleSpriteNameComp.Name = heroConfig.IdleSpriteName;
        }
    }
}
