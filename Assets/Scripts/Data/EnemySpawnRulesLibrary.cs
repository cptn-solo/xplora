using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct EnemySpawnRulesLibrary
    {
        public OpponentSpawnConfig OpponentSpawnConfig { get; set; }
        public OpponentTeamMemberSpawnConfig OpponentTeamMemberSpawnConfig { get; set; }

        public static EnemySpawnRulesLibrary EmptyLibrary()
        {

            EnemySpawnRulesLibrary result = default;

            result.OpponentSpawnConfig = new();
            result.OpponentTeamMemberSpawnConfig = new();

            return result;
        }

        internal void SetOpponentSpawnConfig(OpponentSpawnConfig opConfig)
        {
            OpponentSpawnConfig = opConfig;
        }

        internal void SetOpponentTeamMemberSpawnConfig(OpponentTeamMemberSpawnConfig teamConfig)
        {
            OpponentTeamMemberSpawnConfig = teamConfig;
        }
    }
}