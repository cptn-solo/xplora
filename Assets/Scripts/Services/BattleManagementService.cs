using Assets.Scripts.UI.Data;
using System;
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

        private bool resetBattle;
        /// <summary>
        ///     Flag to let the battle screen know if it should reset the battle/queue
        /// </summary>
        public bool ResetBattle { 
            get => resetBattle;
            internal set {
                resetBattle = value;
                if (resetBattle)
                    CurrentTurn = -1;
            } 
        }

        public void BeginBattle()
        {
            CurrentTurn = 0;
        }

        public int CurrentTurn { get; private set; }

        public void PrepareTeam(HeroDict dict, int id)
        {
            var team = playerTeam.Id == id ? playerTeam : enemyTeam;
            
            for (int i = 0; i < 4; i++)
                team.BackLine[i] = Hero.Default;

            for (int i = 0; i < dict.Count; i++)
                team.FrontLine[i] = dict[i];
        }

        public void LoadData()
        {
            var hp = Asset.EmptyAsset(AssetType.Defence, "HP", "hp");
            var shield = Asset.EmptyAsset(AssetType.Defence, "Shield", "shield");
            var bomb = Asset.EmptyAsset(AssetType.Attack, "Bomb", "bomb");
            var shuriken = Asset.EmptyAsset(AssetType.Attack, "Shuriken", "shuriken");
            var power = Asset.EmptyAsset(AssetType.Attack, "Power", "power");

            hp.GiveAsset(playerTeam, 5);
            shield.GiveAsset(playerTeam, 5);
            bomb.GiveAsset(playerTeam, 5);
            shuriken.GiveAsset(playerTeam, 5);
            power.GiveAsset(playerTeam, 5);

            hp.GiveAsset(enemyTeam, 5);
            shield.GiveAsset(enemyTeam, 5);
            bomb.GiveAsset(enemyTeam, 5);
            shuriken.GiveAsset(enemyTeam, 5);
            power.GiveAsset(enemyTeam, 5);

        }

        internal void CompleteTurn()
        {
            CurrentTurn++;
        }
    }
}