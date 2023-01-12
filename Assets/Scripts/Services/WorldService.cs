using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public delegate Unit Spawner(Vector2 pos, Hero hero);
    public delegate HexCoordinates HexCoordResolver(HexCoordinates cell, HexDirection dir);
    public partial class WorldService : MonoBehaviour
    {
        private Spawner unitSpawner;
        private HexCoordResolver coordResolver;

        private Hero playerHero;
        private Unit playerUnit;



        public void SetPlayerHero(Hero hero)
        {
            playerHero = hero;
        }
        public void SetUnitSpawner(Spawner spawner)
        {
            unitSpawner = spawner;
        }
        public void SetCoordResolver(HexCoordResolver resolver)
        {
            coordResolver = resolver;
        }

        public Unit SpawnPlayer()
        {

            if (playerHero.HeroType != HeroType.NA)
                playerUnit = unitSpawner?.Invoke(Vector2.zero, playerHero);
            else
                playerUnit = null;

            return playerUnit;
        }
        public Unit SpawnEnemy()
        {

            if (playerHero.HeroType != HeroType.NA)
                playerUnit = unitSpawner?.Invoke(Vector2.zero, playerHero);
            else
                playerUnit = null;

            return playerUnit;
        }

        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {            
            playerUnit.MoveTo(coordinates);
        }
        public void ProcessMoveInput(Vector3 direction)
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
                var targetCoord = coordResolver(playerUnit.CurrentCoord, hexDir);
                playerUnit.MoveTo(targetCoord);
            }

            // var targetPos = playerUnit.transform.position + direction;            
            //playerUnit.MoveTo(targetPos);
        }

    }
}