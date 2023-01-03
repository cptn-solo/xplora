using Assets.Scripts.UI.Data;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public delegate Transform Spawner(Vector2 pos, Hero hero);
    public partial class WorldService : MonoBehaviour
    {
        private Spawner unitSpawner;

        private Hero playerHero;
        public void SetPlayerHero(Hero hero)
        {
            playerHero = hero;
        }
        public void SetUnitSpawner(Spawner spawner)
        {
            unitSpawner = spawner;
        }

        public Transform SpawnPlayer()
        {
            if (playerHero.HeroType != HeroType.NA)
                return unitSpawner?.Invoke(Vector2.zero, playerHero);
            else
                return null;
        }

    }
}