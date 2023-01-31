using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public partial class WorldService : MonoBehaviour
    {
        public event UnityAction<Unit, bool> OnUnitSpawned;


        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;

        private WorldState worldState = WorldState.NA;

        public WorldState WorldState => worldState;

        private HeroLibraryManagementService libManagementService;
        private BattleManagementService battleManagementService;
        private MenuNavigationService menuNavigationService;

        private bool isWorldStateLoopActive;
        

        public void AssignPlayerAndEnemies(Hero[] playerHeroes)
        {
            var playerAvatar = playerHeroes.Length > 0 ? playerHeroes[0] : Hero.Default;

            Hero[] freeHeroes = libManagementService.Library.NonPlayerTeamHeroes();

            SyncEcsRaidParties(playerAvatar, freeHeroes);

        }

        public void StartWorldStateLoop()
        {
            StartEcsWorldContext();

            if (!isWorldStateLoopActive)
                StartCoroutine(WorldStateLoopCoroutine());
        }

        private void SpawnUnits()
        {
            worldState = WorldState.UnitsBeingSpawned;

            SpawnPlayer();
            SpawnEnemies(() => { 
                worldState = WorldState.UnitsSpawned;
            });
        }

        private Unit PlayerDeploymentCallback(int cellId, Hero hero)
        {
            var coord = CellCoordinatesResolver(cellId);
            var pos = WorldPositionResolver(coord);

            var playerUnit = UnitSpawner?.Invoke(pos, hero, null);
            playerUnit.SetInitialCoordinates(coord);

            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            return playerUnit;

        }

        private Unit OpponentDeploymentCallback(int cellId, Hero hero)
        {
            var coord = CellCoordinatesResolver(cellId);
            var pos = WorldPositionResolver(coord);

            var enemyUnit = UnitSpawner?.Invoke(pos, hero, null);

            OnUnitSpawned?.Invoke(enemyUnit, false);

            return enemyUnit;

        }

        private Unit SpawnPlayer()
        {
            var playerUnit = DeployEcsWorldPlayer(PlayerDeploymentCallback);

            return playerUnit;
        }

        private void SpawnEnemies(UnitSpawnerCallback callback)
        {
            DeployEcsWorldOpponents(OpponentDeploymentCallback);

            callback?.Invoke();
        }        

        internal void GenerateTerrain()
        {
            worldState = WorldState.TerrainBeingGenerated;

            TerrainProducer?.Invoke(width, height, () => { 
                worldState = WorldState.SceneReady;
            });
        }

        internal void AttachServices(
            HeroLibraryManagementService libManagementService,
            BattleManagementService battleManagementService,
            MenuNavigationService menuNavigationService)
        {
            this.libManagementService = libManagementService;
            this.battleManagementService = battleManagementService;
            this.menuNavigationService = menuNavigationService;
        }

        #region Battle

        private bool InitiateBattle(int cellId)
        {
            if (!CheckEcsWorldForOpponent(cellId, out var enemyHero))
                return false;

            worldState = WorldState.PrepareBattle;

            libManagementService.Library.MoveHero(
                enemyHero,
                new(
                    libManagementService.EnemyTeam.Id,
                    BattleLine.Front,
                    0
                    ));

            battleManagementService.ResetBattle();

            worldState = WorldState.InBattle;

            menuNavigationService.NavigateToScreen(Screens.Battle);

            return true;
        }       

        internal void ProcessAftermath(BattleInfo battle)
        {
            worldState = WorldState.Aftermath;

            if (battle.WinnerTeamId == libManagementService.PlayerTeam.Id)
                ProcessBattleWon();
            else
                ProcessBattleLost();

            TearDownEcsRaidBattle();
        }

        private void ProcessBattleLost()
        {
            if (!TryGetPlayerHero(out var hero))
                return;

            CleanupEcsRaid(libManagementService.Library.RetireHero);

            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
        }

        private void ProcessBattleWon()
        {
            RetireEcsRaidEnemy(libManagementService.Library.RetireHero);

            menuNavigationService.NavigateToScreen(Screens.Raid);
        }
        #endregion

        private void OnDestroy()
        {
            StopEcsWorldContext();
        }
    }
}