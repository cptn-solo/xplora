using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Assets.Scripts.Services.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public partial class WorldService : MonoBehaviour
    {
        public event UnityAction<Unit, bool> OnUnitSpawned;

        public EcsPackedEntity PlayerEntity { get; set; }
        public EcsPackedEntity WorldEntity { get; set; }
        public EcsPackedEntity RaidEntity { get; set; }

        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;

        private WorldState worldState = WorldState.NA;

        public WorldState WorldState => worldState;

        private HeroLibraryManagementService libManagementService;
        private BattleManagementService battleManagementService;
        private MenuNavigationService menuNavigationService;


        private EcsWorld ecsWorld;
        private IEcsSystems ecsSystems;

        private bool isWorldStateLoopActive;
        

        public void AssignPlayerAndEnemies(Hero[] playerHeroes)
        {
            var playerAvatar = playerHeroes.Length > 0 ? playerHeroes[0] : Hero.Default;

            Hero[] freeHeroes = libManagementService.Library.NonPlayerTeamHeroes();

            // reassign avatar hero to player entity
            if (PlayerEntity.Unpack(ecsWorld, out var playerEntity))
            {
                var playerPool = ecsWorld.GetPool<PlayerComp>();
                ref var playerComp = ref playerPool.Get(playerEntity);
                playerComp.Hero = playerAvatar;
                playerComp.CellIndex = -1;
            }

            // clear opponents' entities
            var opponentFilter = ecsWorld.Filter<Inc<OpponentComp>>().End();
            foreach (var opponentEntity in opponentFilter)
            {
                ecsWorld.DelEntity(opponentEntity);
            }

            // create new ones marked with enemies' avatars
            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            foreach (var opponentHero in freeHeroes)
            {
                var opponentEntity = ecsWorld.NewEntity();
                ref var opponentComp = ref opponentPool.Add(opponentEntity);
                opponentComp.Hero = opponentHero;
                opponentComp.CellIndex = -1;
            }

        }

        public void StartWorldStateLoop()
        {
            ecsWorld = new EcsWorld();
            ecsSystems = new EcsSystems(ecsWorld);

            ecsSystems
                .Add(new WorldSystem())
                .Add(new RaidSystem())
                .Add(new PlayerSystem())
                .Add(new VisitSystem())
                .Add(new LeaveSystem())
                .Add(new RefillSystem())
                .Add(new DrainSystem())
                .Inject(this)
                .Init();

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

        private Unit SpawnPlayer()
        {
            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return null;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);

            if (playerComp.Hero.HeroType == HeroType.NA)
                return null;

            if (playerComp.CellIndex < 0)
                playerComp.CellIndex = Random.Range(0, width * height);

            var coord = CellCoordinatesResolver(playerComp.CellIndex);
            var pos = WorldPositionResolver(coord);

            var playerUnit = UnitSpawner?.Invoke(pos, playerComp.Hero, null);
            playerUnit.SetInitialCoordinates(coord);
            
            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            var unitPool = ecsWorld.GetPool<UnitComp>();
            ref var unitComp = ref unitPool.Add(playerEntity);
            unitComp.Unit = playerUnit;

            return playerUnit;
        }

        private void SpawnEnemies(UnitSpawnerCallback callback)
        {
            var opponentFilter = ecsWorld.Filter<Inc<OpponentComp>>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            var playerPool = ecsWorld.GetPool<PlayerComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            ref var playerComp = ref playerPool.Get(playerEntity);
            var usedCells = new List<int>();
            usedCells.Add(playerComp.CellIndex);

            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);

                var enemyHero = opponentComp.Hero;

                if (opponentComp.CellIndex < 0)
                {
                    var cellIndex = Random.Range(0, width * height);
                    while (usedCells.Contains(cellIndex))
                    {
                        cellIndex = Random.Range(0, width * height);
                    }
                    opponentComp.CellIndex = cellIndex;
                    usedCells.Add(cellIndex);
                }

                var coord = CellCoordinatesResolver(opponentComp.CellIndex);
                var pos = WorldPositionResolver(coord);

                Unit enemyUnit = null;
                if (enemyHero.HeroType != HeroType.NA)
                {
                    enemyUnit = UnitSpawner?.Invoke(pos, enemyHero, null);

                    OnUnitSpawned?.Invoke(enemyUnit, false);

                    var unitPool = ecsWorld.GetPool<UnitComp>();
                    ref var unitComp = ref unitPool.Add(opponentEntity);
                    unitComp.Unit = enemyUnit;
                }

            }

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

        private void InitiateBattle(WorldObject enemy)
        {
            worldState = WorldState.PrepareBattle;

            for (int i = 0; i < worldObjects.Count; i++)
            {
                var val = worldObjects[i];
                val.Unit = null;
                worldObjects[i] = val;
            }

            enemy.Unit = null;
            this.enemy = enemy;

            libManagementService.Library.MoveHero(
                enemy.Hero,
                new(
                    libManagementService.EnemyTeam.Id,
                    BattleLine.Front,
                    0
                    ));

            battleManagementService.ResetBattle();

            worldState = WorldState.InBattle;

            menuNavigationService.NavigateToScreen(Screens.Battle);
        }

        internal void ProcessAftermath(BattleInfo battle)
        {
            worldState = WorldState.Aftermath;

            if (battle.WinnerTeamId == libManagementService.PlayerTeam.Id)
                ProcessBattleWinned();
            else
                ProcessBattleLost();
        }

        private void ProcessBattleLost()
        {
            //TODO: remove player unit, return to the library
            libManagementService.Library.RetireHero(player.Hero);
            
            worldObjects.Remove(player);

            player = WorldObject.Default;

            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
        }

        private void ProcessBattleWinned()
        {
            if (enemy.ObjectType == WorldObjectType.NA)
                return;

            //TODO: remove enemy unit, show aftermath
            libManagementService.Library.RetireHero(enemy.Hero);

            worldObjects.Remove(player);
            player.CellIndex = enemy.CellIndex;
            worldObjects.Add(player);

            enemyHeroes.Remove(enemy.Hero);
            worldObjects.Remove(enemy);            
            enemy = WorldObject.Default;

            menuNavigationService.NavigateToScreen(Screens.Raid);
        }
        #endregion

        private void OnDestroy()
        {
            ecsSystems?.Destroy();
            ecsSystems = null;

            ecsWorld?.Destroy();
            ecsWorld = null;

        }
    }
}