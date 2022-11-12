using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    using AssetDict = Dictionary<int, Asset>;
    using HeroDict = Dictionary<int, Hero>;

    public struct Team : IEntity
    {
        private int id;
        private string name;
        private AssetDict inventory; // index, item
        private HeroDict backLine;
        private HeroDict frontLine;

        public int Id => id;
        public string Name => name;
        public AssetDict Inventory => inventory; // index, item
        public HeroDict FrontLine => frontLine;
        public HeroDict BackLine => backLine;

        public static Team EmptyTeam(int id, string name)
        {
            Team team = default;
            team.id = id;
            team.name = name;
            team.inventory = DefaultInventory();
            team.backLine = DefaultFrontLine();
            team.frontLine = DefaultBackLine();

            return team;
        }

        public int GiveAsset(Asset asset, int index = -1) =>
            inventory.PutAsset(asset, index);

        public Asset TakeAsset(AssetType assetType, int count)
        {
            throw new System.NotImplementedException();
        }
        public Asset TakeAsset(int index, int count)
        {
            return inventory.TakeAsset(index);
        }

        public int GiveHero(Hero hero,
            BattleLine line = BattleLine.Front, int index = -1) =>
            line switch
            {
                BattleLine.Front => frontLine.PutHero(hero, index),
                BattleLine.Back => backLine.PutHero(hero, index),
                _ => -1
            };
        public Hero MoveHero(Hero hero, HeroDict from, int idxFrom, HeroDict to, int idxTo)
        {
            from.TakeHero(idxFrom);
            to.PutHero(hero, idxTo);
            return hero;
        }
        private static AssetDict DefaultInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
            {10, default}, {11, default}, {12, default}, {13, default}, {14, default},
        };
        private static HeroDict DefaultFrontLine() => new() {
            {0, default}, {1, default}, {2, default}, {3, default},
        };
        private static HeroDict DefaultBackLine() => new() {
            {0, default}, {1, default}, {2, default}, {3, default},
        };
    }


}