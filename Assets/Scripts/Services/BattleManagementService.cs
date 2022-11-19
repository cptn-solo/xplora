using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    using HeroDict = Dictionary<int, Hero>;

    public class BattleManagementService : MonoBehaviour
    {
        private readonly Team playerTeam = Team.EmptyTeam(0, "Player");
        private readonly Team enemyTeam = Team.EmptyTeam(1, "Enemy");

        public Team PlayerTeam => playerTeam;
        public Team EnemyTeam => enemyTeam;

        public void PrepareTeam(HeroDict dict, int id)
        {
            var team = playerTeam.Id == id ? playerTeam : enemyTeam;
            
            for (int i = 0; i < 4; i++)
                team.BackLine[i] = Hero.Default;

            for (int i = 0; i < dict.Count; i++)
                team.FrontLine[i] = dict[i];
        }

    }
}