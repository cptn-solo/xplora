using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleHeroInstanceInit : IEcsInitSystem
    {
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> heroInstanceOriginRefPool;
        private readonly EcsPoolInject<SpeedComp> speedPool;
        private readonly EcsPoolInject<FrontlineTag> frontlineTagPool;
        private readonly EcsPoolInject<BacklineTag> backlineTagPool;
        private readonly EcsPoolInject<HealthComp> healthPool;
        private readonly EcsPoolInject<HPComp> hpPool;
        private readonly EcsPoolInject<EffectsComp> effectsPool;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool;
        private readonly EcsPoolInject<RangedTag> rangedTagPool;
        private readonly EcsPoolInject<NameComp> namePool;
        private readonly EcsPoolInject<IconName> iconNamePool;
        private readonly EcsPoolInject<IdleSpriteName> idleSpriteNamePool;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool;

        private readonly EcsFilterInject<Inc<PositionComp>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Init(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            foreach (var entity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);

                InitBattleHeroInstance(
                    entity, battleWorld, battleInfo, heroConfigRef.HeroConfigPackedEntity);                
            }

            battleInfo.State = BattleState.TeamsPrepared;

            roundShortageTagPool.Value.Add(battleEntity); // to create 1st rounds

            battleService.Value.NotifyBattleEventListeners(battleInfo);

        }

        private void InitBattleHeroInstance(
            int heroInstanceEntity,
            EcsWorld battleWorld,
            BattleInfo battleInfo,
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
            barsAndEffectsComp.Speed = speedComp.Value;
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
