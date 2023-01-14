using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public enum WorldObjectType
    {
        NA = 0,
        Unit = 100,
    }
    public struct WorldObject
    {
        public WorldObjectType ObjectType { get; private set; }
        public int CellIndex { get; set; }
        public Hero Hero { get; set; }
        public Unit Unit { get; set; }

        internal static WorldObject Default { get; } = 
            new WorldObject {
                ObjectType = WorldObjectType.NA,
                CellIndex = -1,
                Hero = default,
                Unit = null,
            };

        internal static WorldObject Create(int cellIndex, Hero hero, Unit unit)
        {
            WorldObject obj = default;

            obj.ObjectType = WorldObjectType.Unit;
            obj.CellIndex = cellIndex;
            obj.Hero = hero;
            obj.Unit = unit;
            
            return obj;
        }
    }
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

        private Hero playerHero;
        private List<Hero> enemyHeroes;
        
        /// <summary>
        /// Cell index used as an id
        /// </summary>
        private readonly List<WorldObject> worldObjects = new();

        private bool isWorldStateLoopActive;
        
        private WorldObject enemy = WorldObject.Default;
        private WorldObject player = WorldObject.Default;
        private int playerObjIndex;

        public void AssignPlayerAndEnemies(Hero[] playerHeroes)
        {
            var playerAvatar = playerHeroes.Length > 0 ? playerHeroes[0] : Hero.Default;
            playerHero = playerAvatar;

            Hero[] freeHeroes = libManagementService.Library.NonPlayerTeamHeroes();
            SetEnemies(freeHeroes);
            
        }

        public void SetEnemies(Hero[] enemies)
        {
            enemyHeroes = new(enemies);
        }

        public void StartWorldStateLoop()
        {
            if (!isWorldStateLoopActive)
                StartCoroutine(WorldStateLoopCoroutine());
        }

        private void SpawnUnits()
        {
            worldState = WorldState.UnitsBeingSpawned;

            SpawnPlayer();
            
            StartCoroutine(SpawnEnemiesCoroutine(() => { 
                worldState = WorldState.UnitsSpawned;
            }));
        }

        private Unit SpawnPlayer()
        {            
            if (playerHero.HeroType == HeroType.NA)
                return null;

            var cellIndex = player.ObjectType == WorldObjectType.Unit ?
                player.CellIndex : Random.Range(0, width * height);
            var coord = CellCoordinatesResolver(cellIndex);
            var pos = WorldPositionResolver(coord);

            var playerUnit = UnitSpawner?.Invoke(pos, playerHero, null);
            OnUnitSpawned?.Invoke(playerUnit, true);

            playerUnit.OnArrivedToCoordinates += PlayerUnit_OnArrivedToCoordinates;

            if (worldObjects.Contains(player))
                worldObjects.Remove(player);

            player = WorldObject.Create(cellIndex, playerHero, playerUnit);
            
            worldObjects.Add(player);

            return playerUnit;
        }

        private IEnumerator SpawnEnemiesCoroutine(UnitSpawnerCallback callback)
        {
            for (int i = 0; i < enemyHeroes.Count; i++)
            {
                var enemyHero = enemyHeroes[i];
                var enemy = worldObjects.FirstOrDefault(x => 
                    x.ObjectType == WorldObjectType.Unit &&
                    x.Hero.HeroType != HeroType.NA &&
                    x.Hero.Id == enemyHero.Id);
                var cellIndex = -1;
                if (enemy.ObjectType == WorldObjectType.Unit)
                {
                    cellIndex = enemy.CellIndex;
                }
                else {
                    cellIndex = Random.Range(0, width * height);
                    while (worldObjects.Exists(x =>
                        x.ObjectType == WorldObjectType.Unit &&
                        x.CellIndex == cellIndex
                    ))
                    {
                        cellIndex = Random.Range(0, width * height);
                        yield return null;
                    }
                }

                var coord = CellCoordinatesResolver(cellIndex);
                var pos = WorldPositionResolver(coord);

                Unit enemyUnit = null;
                if (enemyHero.HeroType != HeroType.NA)
                {
                    enemyUnit = UnitSpawner?.Invoke(pos, enemyHero, null);

                    if (enemy.ObjectType != WorldObjectType.NA)
                        worldObjects.Remove(enemy);

                    worldObjects.Add(WorldObject.Create(cellIndex, enemyHero, enemyUnit));

                    OnUnitSpawned?.Invoke(enemyUnit, false);
                }

                yield return null;
            }

            callback?.Invoke();
        }
        private void PlayerUnit_OnArrivedToCoordinates(HexCoordinates coordinates, Unit unit)
        {
            var cellId = CellIndexResolver(coordinates);
            if (worldObjects.FirstOrDefault(x => x.CellIndex == cellId) is WorldObject obj &&
                obj.ObjectType == WorldObjectType.Unit &&
                obj.Unit != null &&
                obj.Hero.HeroType != HeroType.NA)
            {
                InitiateBattle(obj);
            }
            else
            {
                worldObjects.Remove(player);
                
                player.CellIndex = cellId;
                
                worldObjects.Add(player);
            }
        }

        public void ProcessTargetCoordinatesSelection(HexCoordinates coordinates)
        {
            CoordHighlighter?.Invoke(coordinates);

            //TODO: add a decision button or such
            ProcessMoveToHexCoordinates(coordinates);
        }

        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {
            player.Unit.SetMoveTargetCoordinates(coordinates);
            player.Unit.MoveToTargetCoordinates();
        }

        public void ProcessDirectionSelection(Vector3 direction)
        {
            HexDirection hexDir;
            if (direction.x > 0 && direction.z > 0)
                hexDir = HexDirection.NE;
            else if (direction.x > 0 && direction.z < 0)
                hexDir = HexDirection.SE;
            else if (direction.x > 0)
                hexDir = HexDirection.E;
            else if (direction.x < 0 && direction.z > 0)
                hexDir = HexDirection.NW;
            else if (direction.x < 0 && direction.z < 0)
                hexDir = HexDirection.SW;
            else if (direction.x < 0)
                hexDir = HexDirection.W;
            else if (direction.z > 0)
                hexDir = HexDirection.W;
            else
                hexDir = HexDirection.NA; // can't move to south or north or default should be set

            if (hexDir != HexDirection.NA)
            {
                // TODO: decide on move rules etc.
                var targetCoord = CoordResolver(player.Unit.CurrentCoord, hexDir);
                ProcessTargetCoordinatesSelection(targetCoord);
            }

            // var targetPos = playerUnit.transform.position + direction;            
            //playerUnit.MoveTo(targetPos);
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

    }
}