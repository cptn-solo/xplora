using Assets.Scripts.Data;
using Leopotam.EcsLite;
using Assets.Scripts.UI.Data;
using Assets.Scripts.ECS.Data;
using System;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : BaseEcsService
    {
        private MenuNavigationService menuNavigationService;
        private BattleManagementService battleManagementService;

        public ref Team PlayerTeam => ref GetEcsPlayerTeam();
        public ref Team EnemyTeam => ref GetEcsEnemyTeam();

        public EcsPackedEntityWithWorld[] PlayerHeroes => GetEcsTeamHeroInstances(PlayerTeamEntity);
        public EcsPackedEntityWithWorld[] EnemyHeroes => GetEcsTeamHeroInstances(EnemyTeamEntity);
        public EcsPackedEntityWithWorld[] EnemyDomainHeroes => GetEcsEnemyDomainHeroInstances();

        public void Init(
            MenuNavigationService menuNavigationService,
            BattleManagementService battleManagementService)
        {
            StartEcsContext();
            InitConfigLoading();

            this.menuNavigationService = menuNavigationService;
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;

            this.battleManagementService = battleManagementService;
            battleManagementService.OnBattleComplete += BattleManagementService_OnBattleComplete;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens current, Screens prev)
        {
        }

        private void BattleManagementService_OnBattleComplete(bool won, Asset[] pot)
        {
            if (!won)
                return;

            if (!PlayerTeamEntity.Unpack(out var world, out var entity))
                return;
            
            var buffer = ListPool<Asset>.Get();
            ref var team = ref PlayerTeam;

            buffer.AddRange(team.Assets);

            foreach (var trophy in pot)
            {
                if (buffer.FindIndex(x => x.AssetType == trophy.AssetType) is var idx &&
                    idx >= 0)
                {
                    var val = buffer[idx];
                    val.Count += trophy.Count;
                    buffer[idx] = val;
                }
                else
                {
                    buffer.Add(new Asset()
                    {
                        AssetType = trophy.AssetType,
                        Count = trophy.Count
                    });
                }
            }

            team.Assets = buffer.ToArray();

            ListPool<Asset>.Add(buffer);

            var pool = ecsWorld.GetPool<UpdateAssetBalanceTag>();
            if (!pool.Has(entity))
                pool.Add(entity);
        }

        private void OnDestroy()
        {
            StopEcsContext();
            menuNavigationService.OnBeforeNavigateToScreen -= MenuNavigationService_OnBeforeNavigateToScreen;
            battleManagementService.OnBattleComplete -= BattleManagementService_OnBattleComplete;
        }

        internal void MoveHero(EcsPackedEntityWithWorld hero, HeroPosition pos) =>
            MoveEcsHeroToPosition(hero, pos);

        internal void SetRelationScore(EcsPackedEntityWithWorld hero, float value)
        {
            SetEcsRelationScore(hero, value);
        }

        internal void SetSelectedHero(EcsPackedEntityWithWorld hero) =>
            SetEcsSelectedHero(hero);

        internal void PrepareInstantBattle(out EcsPackedEntityWithWorld[] playerHeroes, out EcsPackedEntityWithWorld[] enemyHeroes)
        {
            var filter = ecsWorld.Filter<HeroConfigRef>().End();
            var pool = ecsWorld.GetPool<HeroConfigRef>();
            var configPool = ecsWorld.GetPool<Hero>();
            var playerTeamBuff =    ListPool<EcsPackedEntityWithWorld>.Get();
            var enemyTeamBuff =     ListPool<EcsPackedEntityWithWorld>.Get();
            
            // 1. pick heroes for the instant battle:
            
            foreach (var entity in filter)
            {
                ref var configRef = ref pool.Get(entity);
                if (!configRef.Packed.Unpack(out _, out var configEntity))
                    throw new Exception("Stale config ref");
                
                ref var config = ref configPool.Get(configEntity);
                if (config.Domain == HeroDomain.Hero && playerTeamBuff.Count < 2)
                {
                    playerTeamBuff.Add(EcsWorld.PackEntityWithWorld(entity));
                    MoveHero(playerTeamBuff[playerTeamBuff.Count - 1], new HeroPosition(0, BattleLine.Front, playerTeamBuff.Count -1));
                }
                if (config.Domain == HeroDomain.Enemy && enemyTeamBuff.Count < 2 && config.OveralStrength == 2)
                {
                    enemyTeamBuff.Add(EcsWorld.PackEntityWithWorld(entity));
                    MoveHero(enemyTeamBuff[enemyTeamBuff.Count - 1], new HeroPosition(1, BattleLine.Front, enemyTeamBuff.Count -1));
                }
                
            }

            ecsRunSystems.Run();

            // 2. assign some relations (for tests, so relations can be assigned according to the testing purpose):

            foreach (var item in playerTeamBuff)
            {
                SetSelectedHero(item);
                foreach (var item1 in playerTeamBuff)
                {
                    if (!item.EqualsTo(item1))
                        SetRelationScore(item1, 8);                    
                }
            }

            playerHeroes = playerTeamBuff.ToArray();
            enemyHeroes = enemyTeamBuff.ToArray();

            ListPool<EcsPackedEntityWithWorld>.Add(playerTeamBuff);
            ListPool<EcsPackedEntityWithWorld>.Add(enemyTeamBuff);
        }
    }
}