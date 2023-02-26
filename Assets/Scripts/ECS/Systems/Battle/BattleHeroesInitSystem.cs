using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.ECS.Systems
{
    using HeroPosition = Tuple<int, BattleLine, int>;
    using Random = UnityEngine.Random;

    public class BattleHeroesInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<HealthComp> healthPool;
        private readonly EcsPoolInject<HPComp> hpPool;
        private readonly EcsPoolInject<EffectsComp> effectsPool;
        private readonly EcsPoolInject<BarsAndEffectsInfo> barsAndEffectsPool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<EnemyTeamTag> enemyTeamTagPool;
        private readonly EcsPoolInject<PositionComp> positionPool;
        private readonly EcsPoolInject<SpeedComp> speedPool;
        private readonly EcsPoolInject<FrontlineTag> frontlineTagPool;
        private readonly EcsPoolInject<BacklineTag> backlineTagPool;        
        private readonly EcsPoolInject<RangedTag> rangedTagPool;        
        private readonly EcsPoolInject<NameComp> namePool;
        private readonly EcsPoolInject<IconName> iconNamePool;
        private readonly EcsPoolInject<IdleSpriteName> idleSpriteNamePool;
        private readonly EcsPoolInject<RoundShortageTag> roundShortageTagPool;

        private readonly EcsCustomInject<BattleManagementService> battleService;
        private readonly EcsCustomInject<HeroLibraryService> libraryService;

        public void Init(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);
            battleInfo.State = BattleState.PrepareTeams;
            battleService.Value.NotifyBattleEventListeners(battleInfo);

            EcsPackedEntityWithWorld? lastUsedEnemyConfig = null;

            var enemyTeamId = battleInfo.EnemyTeam.Id;
            var posBuffer = ListPool<HeroPosition>.Get();
            posBuffer.AddRange(battleService.Value.BattleFieldSlotsPositions.
                Where(x => x.Item1 == enemyTeamId));

            foreach (var heroConfigPackedEntity in libraryService.Value.HeroConfigEntities)
            {
                //clone hero config into a new entity in the battle context
                //so we could use same config for several enemies for example
                if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                    throw new Exception("No Hero");

                var libPositionPool = libWorld.GetPool<PositionComp>();
                ref var libPosition = ref libPositionPool.Get(heroConfigEntity);

                if (libPosition.Position.Item1 == -1)
                {
                    continue;
                }
                else if (libPosition.Position.Item1 == battleInfo.PlayerTeam.Id)
                {
                    InitBattleHeroInstance<PlayerTeamTag>(
                        battleWorld, battleInfo, heroConfigPackedEntity, libPosition.Position);
                }
                else
                {
                    InitBattleHeroInstance<EnemyTeamTag>(
                        battleWorld, battleInfo, heroConfigPackedEntity, libPosition.Position);
                    lastUsedEnemyConfig = heroConfigPackedEntity;

                    posBuffer.Remove(libPosition.Position);
                }
            }

            if (battleWorld.Filter<EnemyTeamTag>().End().GetEntitiesCount() is int enemyTeamCount &&
                enemyTeamCount > 0 &&
                battleWorld.Filter<PlayerTeamTag>().End().GetEntitiesCount() is int playerTeamCount &&
                playerTeamCount > 0)
            {
                battleInfo.State = BattleState.TeamsPrepared;
                battleService.Value.NotifyBattleEventListeners(battleInfo);

                roundShortageTagPool.Value.Add(battleEntity); // to create 1st rounds
            }

            if (battleInfo.State == BattleState.TeamsPrepared)
            {
                for (int i = 0; i < Random.Range(0, posBuffer.Count + 1); i++)
                {
                    var pos = posBuffer[Random.Range(0, posBuffer.Count)];
                    InitBattleHeroInstance<EnemyTeamTag>(
                        battleWorld, battleInfo, lastUsedEnemyConfig.Value, pos);
                    posBuffer.Remove(pos);
                }
            }


            ListPool<HeroPosition>.Add(posBuffer);
        }

        private void InitBattleHeroInstance<T>(
            EcsWorld battleWorld,
            BattleInfo battleInfo,
            EcsPackedEntityWithWorld heroConfigPackedEntity,
            HeroPosition sourcePosition) where T: struct
        {

            if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero");

            var heroInstanceEntity = battleWorld.NewEntity();

            ref var heroConfigRef = ref heroConfigRefPool.Value.Add(heroInstanceEntity);
            heroConfigRef.HeroConfigPackedEntity = heroConfigPackedEntity;

            battleWorld.GetPool<T>().Add(heroInstanceEntity);

            ref var heroInstanceRef = ref heroInstanceRefPool.Value.Add(heroInstanceEntity);
            heroInstanceRef.HeroInstancePackedEntity = battleWorld.PackEntityWithWorld(heroInstanceEntity);

            var libHeroPool = libWorld.GetPool<Hero>();
            ref var heroConfig = ref libHeroPool.Get(heroConfigEntity);

            ref var position = ref positionPool.Value.Add(heroInstanceEntity);
            position.Position = sourcePosition;

            if (position.Position.Item2 == BattleLine.Front)
                frontlineTagPool.Value.Add(heroInstanceEntity);
            else
                backlineTagPool.Value.Add(heroInstanceEntity);

            ref var speedComp = ref speedPool.Value.Add(heroInstanceEntity);
            speedComp.Value = heroConfig.Speed;

            ref var healthComp = ref healthPool.Value.Add(heroInstanceEntity);
            healthComp.Value = heroConfig.Health;

            ref var hpComp = ref hpPool.Value.Add(heroInstanceEntity);
            hpComp.Value = heroConfig.Health;

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
