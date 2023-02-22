using System;
using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleHeroesInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool;
        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;
        private readonly EcsPoolInject<HealthComp> healthPool;
        private readonly EcsPoolInject<HPComp> hpPool;
        private readonly EcsPoolInject<EffectsComp> effectsPool;
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

            var playerCount = 0;
            var enemyCount = 0;
            foreach (var heroConfigPackedEntity in libraryService.Value.HeroConfigEntities)
            {
                //clone hero config into a new entity in the battle context
                //so we could use same config for several enemies for example
                if (!heroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                    throw new Exception("No Hero");

                var libPositionPool = libWorld.GetPool<PositionComp>();
                ref var libPosition = ref libPositionPool.Get(heroConfigEntity);

                if (libPosition.Position.Item1 == -1)
                    continue;

                var heroInstanceEntity = battleWorld.NewEntity();

                if (libPosition.Position.Item1 == battleInfo.PlayerTeam.Id)
                {
                    playerTeamTagPool.Value.Add(heroInstanceEntity);
                    playerCount++;
                }
                else
                {
                    enemyTeamTagPool.Value.Add(heroInstanceEntity);
                    enemyCount++;
                }

                ref var heroInstanceRef = ref heroInstanceRefPool.Value.Add(heroInstanceEntity);
                heroInstanceRef.HeroInstancePackedEntity = battleWorld.PackEntityWithWorld(heroInstanceEntity);

                var libHeroPool = libWorld.GetPool<Hero>();
                ref var heroConfig = ref libHeroPool.Get(heroConfigEntity);

                ref var position = ref positionPool.Value.Add(heroInstanceEntity);
                position.Position = libPosition.Position;

                if (position.Position.Item2 == BattleLine.Front)
                    frontlineTagPool.Value.Add(heroInstanceEntity);
                else
                    backlineTagPool.Value.Add(heroInstanceEntity);

                ref var speed = ref speedPool.Value.Add(heroInstanceEntity);
                speed.Value = heroConfig.Speed;

                ref var healthComp = ref healthPool.Value.Add(heroInstanceEntity);
                healthComp.Value = heroConfig.Health;

                ref var hpComp = ref hpPool.Value.Add(heroInstanceEntity);
                hpComp.HP = heroConfig.Health;

                ref var effectsComp = ref effectsPool.Value.Add(heroInstanceEntity);
                effectsComp.ActiveEffects = new();


                if (heroConfig.Ranged)
                    rangedTagPool.Value.Add(heroInstanceEntity);

                ref var nameComp = ref namePool.Value.Add(heroInstanceEntity);
                nameComp.Name = heroConfig.Name;

                ref var iconNameComp = ref iconNamePool.Value.Add(heroInstanceEntity);
                iconNameComp.Name = heroConfig.IconName;

                ref var idleSpriteNameComp = ref idleSpriteNamePool.Value.Add(heroInstanceEntity);
                idleSpriteNameComp.Name = heroConfig.IdleSpriteName;
            }

            if (playerCount > 0 && enemyCount > 0)
            {
                battleInfo.State = BattleState.TeamsPrepared;
                battleService.Value.NotifyBattleEventListeners(battleInfo);

                roundShortageTagPool.Value.Add(battleEntity); // to create 1st rounds
            }

        }
    }
}
