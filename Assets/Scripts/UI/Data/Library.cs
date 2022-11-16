using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    using AssetDict = Dictionary<int, Asset>;
    using HeroDict = Dictionary<int, Hero>;

    public struct HeroesLibrary
    {
        private HeroDict heroes;
        private HeroDict playerTeam;
        private HeroDict enemyTeam;

        private AssetDict inventory; // index, item
        public AssetDict Inventory => inventory; // index, item
        public HeroDict Heroes => heroes;
        public HeroDict PlayerTeam => playerTeam;
        public HeroDict EnemyTeam => enemyTeam;

        public static HeroesLibrary EmptyLibrary()
        {
            HeroesLibrary library = default;
            library.inventory = DefaultInventory();
            library.heroes = DefaultHeroes(24);
            library.playerTeam = DefaultHeroes(4);
            library.enemyTeam = DefaultHeroes(4);

            return library;
        }

        public int GiveHero(Hero hero, int index = -1) =>
            heroes.PutHero(hero, index);
        public Hero MoveHero(Hero hero, HeroDict from, int idxFrom, HeroDict to, int idxTo)
        {
            from.TakeHero(idxFrom);
            to.PutHero(hero, idxTo);
            return hero;
        }

        private static AssetDict DefaultInventory() {
            AssetDict dict = new();
            for (int i = 0; i < 15; i++)
                dict[i] = default;

            return dict;    
        }

        private static HeroDict DefaultHeroes(int num) {
            HeroDict dict = new();
            for (int i = 0; i < num; i++)
                dict[i] = Hero.Default;

            return dict;    
        }
    }


}