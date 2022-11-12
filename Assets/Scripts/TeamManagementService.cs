using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using UnityEngine;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts
{
    public partial class TeamManagementService : MonoBehaviour
    {
        private Team team = Team.EmptyTeam(0, "A Team");

        public Team Team => team;
        private Team LoadTeamData()
        {
            var hp = Asset.EmptyAsset(AssetType.Defence, "HP", "hp");
            var shield = Asset.EmptyAsset(AssetType.Defence, "Shield", "shield");
            var bomb = Asset.EmptyAsset(AssetType.Attack, "Bomb", "bomb");
            var shuriken = Asset.EmptyAsset(AssetType.Attack, "Shuriken", "shuriken");
            var power = Asset.EmptyAsset(AssetType.Attack, "Power", "power");

            Team team = Team.EmptyTeam(0, "A Team");

            hp.GiveAsset(team, 5);
            shield.GiveAsset(team, 5);
            bomb.GiveAsset(team, 5);
            shuriken.GiveAsset(team, 5);
            power.GiveAsset(team, 5);

            var heroes = new List<Hero>();
            for (int i = 0; i < 4; i++)
                heroes.Add(Hero.EmptyHero(i, $"Hero {i}"));

            hp.GiveAsset(heroes[0], 2);
            hp.GiveAsset(heroes[2], 2);
            shield.GiveAsset(heroes[2], 2);
            shield.GiveAsset(heroes[3], 3);
            bomb.GiveAsset(heroes[3], 2);
            bomb.GiveAsset(heroes[0], 2);
            shuriken.GiveAsset(heroes[0], 1);
            shuriken.GiveAsset(heroes[1], 3);
            power.GiveAsset(heroes[1], 2);
            power.GiveAsset(heroes[2], 1);

            team.GiveHero(heroes[0], BattleLine.Front);
            team.GiveHero(heroes[1], BattleLine.Front);
            team.GiveHero(heroes[2], BattleLine.Front);
            team.GiveHero(heroes[3], BattleLine.Back);

            return team;
        }

        public void LoadData()
        {
            team = LoadTeamData();
        }


    }
}