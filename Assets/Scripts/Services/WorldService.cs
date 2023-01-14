using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{

    public partial class WorldService : MonoBehaviour
    {
        #region Delegates
        public Spawner UnitSpawner { get; set; }
        public HexCoordResolver CoordResolver { get; set; }
        public CellCoordinatesResolver CellCoordinatesResolver { get; set; }
        public CellIndexResolver CellIndexResolver { get; set; }
        public WorldPositionResolver WorldPositionResolver { get; set; }
        public HexCoordHighlighter CoordHighlighter { get; set; }

        public TerrainProducer TerrainProducer { get; set; }
        
        private readonly WaitForSeconds TickTimer = new(1f);

        #endregion

        public event UnityAction<Unit, bool> OnUnitSpawned;

        [SerializeField] private int width = 6;
        [SerializeField] private int height = 6;

        private WorldState worldState = WorldState.NA;

        private HeroLibraryManagementService libManagementService;

        private Hero playerHero;
        private Hero[] enemyHeroes;

        private Unit playerUnit;
        /// <summary>
        /// Cell index used as an id
        /// </summary>
        private Dictionary<int, Unit> enemyUnits = new();
        private bool isWorldStateLoopActive;

        public void AssignPlayerAndEnemies(Hero[] playerHeroes)
        {
            var playerAvatar = playerHeroes.Length > 0 ? playerHeroes[0] : Hero.Default;
            playerHero = playerAvatar;

            Hero[] freeHeroes = libManagementService.Library.NonPlayerTeamHeroes();
            SetEnemies(freeHeroes);
            
        }

        public void SetEnemies(Hero[] enemies)
        {
            enemyHeroes = enemies;
        }

        public void StartWorldStateLoop()
        {
            if (!isWorldStateLoopActive)
                StartCoroutine(WorldStateLoopCoroutine());
        }

        private IEnumerator WorldStateLoopCoroutine()
        {
            isWorldStateLoopActive = true;

            while (isWorldStateLoopActive)
            {
                if (TerrainProducer == null && UnitSpawner == null)
                    worldState = WorldState.NA;


                yield return TickTimer;

                switch (worldState)
                {
                    case WorldState.NA:

                        if (TerrainProducer != null && UnitSpawner != null)
                            worldState = WorldState.DelegatesAttached;

                        break;
                    case WorldState.DelegatesAttached:
                        
                        GenerateTerrain();
                        
                        break;
                    case WorldState.TerrainBeingGenerated:
                        break;
                    case WorldState.SceneReady:

                        SpawnUnits();

                        break;
                    case WorldState.UnitsBeingSpawned:
                        break;
                    case WorldState.UnitsSpawned:
                        break;
                    default:
                        break;
                }

            }

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
            var cellIndex = Random.Range(0, width * height);
            var coord = CellCoordinatesResolver(cellIndex);
            var pos = WorldPositionResolver(coord);
            if (playerHero.HeroType != HeroType.NA)
                playerUnit = UnitSpawner?.Invoke(pos, playerHero, null);
            else
                playerUnit = null;
            
            OnUnitSpawned?.Invoke(playerUnit, true);
            
            return playerUnit;
        }

        private IEnumerator SpawnEnemiesCoroutine(UnitSpawnerCallback callback)
        {
            var playerCoord = playerUnit.CurrentCoord;
            var playerCellIdx = CellIndexResolver(playerCoord);

            for (int i = 0; i < enemyHeroes.Length; i++)
            {
                var enemyHero = enemyHeroes[i];
                var cellIndex = Random.Range(0, width * height);
                while (cellIndex == playerCellIdx ||
                    enemyUnits.ContainsKey(cellIndex))
                {
                    cellIndex = Random.Range(0, width * height);
                    yield return null;
                }

                var coord = CellCoordinatesResolver(cellIndex);
                var pos = WorldPositionResolver(coord);

                Unit enemyUnit = null;
                if (enemyHero.HeroType != HeroType.NA)
                {
                    enemyUnit = UnitSpawner?.Invoke(pos, enemyHero, null);
                    enemyUnits.Add(cellIndex, enemyUnit);

                    OnUnitSpawned?.Invoke(enemyUnit, false);
                }

                yield return null;
            }

            callback?.Invoke();
        }
        
        public void ProcessTargetCoordinatesSelection(HexCoordinates coordinates)
        {
            CoordHighlighter?.Invoke(coordinates);

            //TODO: add a decision button or such
            ProcessMoveToHexCoordinates(coordinates);
        }

        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {
            playerUnit.SetMoveTargetCoordinates(coordinates);
            playerUnit.MoveToTargetCoordinates();
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
                var targetCoord = CoordResolver(playerUnit.CurrentCoord, hexDir);
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

        internal void AttachToLibraryService(HeroLibraryManagementService libManagementService)
        {
            this.libManagementService = libManagementService;
        }
    }
}