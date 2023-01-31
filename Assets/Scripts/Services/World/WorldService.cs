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
        public EcsPackedEntity BattleEntity { get; set; }

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
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
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
#if UNITY_EDITOR
        .Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem())
#endif
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
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
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

        private void UpdatePlayerCellId(int cellId)
        {
            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);
            playerComp.CellIndex = cellId;
        }

        private bool InitiateBattle(int cellId)
        {
            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();

            int enemyEntity = -1;
            Hero enemyHero = default;
            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);
                if (opponentComp.CellIndex == cellId)
                {
                    enemyEntity = opponentEntity;
                    enemyHero = opponentComp.Hero;
                    break;
                }
            }

            if (enemyEntity == -1)
                return false;

            worldState = WorldState.PrepareBattle;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            var battleEntity = ecsWorld.NewEntity();

            ref var battleComp = ref battlePool.Add(battleEntity);
            battleComp.EnemyPackedEntity = ecsWorld.PackEntity(enemyEntity);

            BattleEntity = ecsWorld.PackEntity(battleEntity);

            var unitPool = ecsWorld.GetPool<UnitComp>();
            var unitFilter = ecsWorld.Filter<UnitComp>().End();

            foreach (var unitEntity in unitFilter)
                unitPool.Del(unitEntity);

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
                ProcessBattleWinned();
            else
                ProcessBattleLost();

            if (!BattleEntity.Unpack(ecsWorld, out var battleEntity))
                return;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            battlePool.Del(battleEntity);
        }

        private void ProcessBattleLost()
        {
            if (!TryGetPlayerHero(out var hero))
                return;

            CleanupEcsRaid();

            menuNavigationService.NavigateToScreen(Screens.HeroesLibrary);
        }

        private void CleanupEcsRaid()
        {
            var unitPool = ecsWorld.GetPool<UnitComp>();

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            if (PlayerEntity.Unpack(ecsWorld, out var playerEntity))
            {
                ref var playerComp = ref playerPool.Get(playerEntity);

                libManagementService.Library.RetireHero(playerComp.Hero);

                playerComp.CellIndex = -1;
                playerComp.Hero = default;

                unitPool.Del(playerEntity);
            }

            var opponentFilter = ecsWorld.Filter<OpponentComp>().End();
            var opponentPool = ecsWorld.GetPool<OpponentComp>();

            foreach (var opponentEntity in opponentFilter)
            {
                ref var opponentComp = ref opponentPool.Get(opponentEntity);
                libManagementService.Library.RetireHero(opponentComp.Hero);

                opponentPool.Del(opponentEntity);
                unitPool.Del(opponentEntity);
            }
        }

        private void ProcessBattleWinned()
        {
            if (!BattleEntity.Unpack(ecsWorld, out var battleEntity))
                return;

            var battlePool = ecsWorld.GetPool<BattleComp>();
            ref var battleComp = ref battlePool.Get(battleEntity);

            if (!battleComp.EnemyPackedEntity.Unpack(ecsWorld, out var enemyEntity))
                return;

            var opponentPool = ecsWorld.GetPool<OpponentComp>();
            ref var opponentComp = ref opponentPool.Get(enemyEntity);

            //TODO: remove enemy unit, show aftermath
            libManagementService.Library.RetireHero(opponentComp.Hero);

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            ref var playerComp = ref playerPool.Get(playerEntity);

            playerComp.CellIndex = opponentComp.CellIndex;

            opponentPool.Del(enemyEntity);

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

        private bool TryGetPlayerUnit(out Unit unit, out int cellId)
        {
            unit = null;
            cellId = -1;

            var playerPool = ecsWorld.GetPool<PlayerComp>();
            var unitPool = ecsWorld.GetPool<UnitComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return false;

            if (!playerPool.Has(playerEntity) ||
                !unitPool.Has(playerEntity))
                return false;

            ref var unitComp = ref unitPool.Get(playerEntity);
            unit = unitComp.Unit;

            ref var playerComp = ref playerPool.Get(playerEntity);
            cellId = playerComp.CellIndex;

            return true;
        }

        private bool TryGetPlayerHero(out Hero hero)
        {
            hero = default;
            var playerPool = ecsWorld.GetPool<PlayerComp>();

            if (!PlayerEntity.Unpack(ecsWorld, out var playerEntity))
                return false;

            if (!playerPool.Has(playerEntity))
                return false;

            ref var playerComp = ref playerPool.Get(playerEntity);
            hero = playerComp.Hero;

            return false;
        }
    }
}