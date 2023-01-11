using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public delegate Unit Spawner(Vector2 pos, Hero hero);
    public partial class WorldService : MonoBehaviour
    {
        private Spawner unitSpawner;

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

        public Unit SpawnPlayer()
        {

            if (playerHero.HeroType != HeroType.NA)
                playerUnit = unitSpawner?.Invoke(Vector2.zero, playerHero);
            else
                playerUnit = null;

            return playerUnit;
        }
        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {
            var targetPos = coordinates.ToPosition();
            
            playerUnit.MoveTo(targetPos);
        }
        public void ProcessMoveInput(Vector3 direction)
        {
            // TODO: decide on move rules etc.
            var targetPos = playerUnit.transform.position + direction;
            
            playerUnit.MoveTo(targetPos);
        }

    }
}