﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct OpponentTeamMemberSpawnConfig
    {
        public Dictionary<int, StrengthSpawnInfo> OveralStrengthLevels { get; set; }

        public static OpponentTeamMemberSpawnConfig DefaultConfig =>
            new()
            {
                OveralStrengthLevels = new () {
                    { 1, new () {
                        OveralStrenght = 1,
                        TintColor = new Color(77f/255f, 131f/255f, 207f/255f),
                        SpawnRate = 45,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), -20 },
                            { new (21, 30), -30 },
                        },
                    } },
                    { 2, new () {
                        OveralStrenght = 2,
                        TintColor = new Color(58f/255f, 76f/255f, 255f/255f),
                        SpawnRate = 30,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), -15 },
                            { new (21, 30), -20 },
                        },
                    } },
                    { 7, new () {
                        OveralStrenght = 7,
                        TintColor = new Color(104f/255f, 38f/255f, 229f/255f),
                        SpawnRate = 15,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), 20 },
                            { new (21, 30), 20 },
                        },
                    } },
                    { 10, new () {
                        OveralStrenght = 10,
                        TintColor = new Color(226f/255f, 104f/255f, 56f/255f),
                        SpawnRate = 10,
                        TeamStrengthWeightedSpawnRates = new () {
                            { new (1, 10), 0 },
                            { new (11, 20), 15 },
                            { new (21, 30), 30 },
                        },
                    } },
                }
            };
    }
}