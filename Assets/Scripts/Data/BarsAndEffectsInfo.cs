using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BarsAndEffectsInfo
    {
        // effects state carries attackers hp and effects
        // while other states - targets hp and effects
        public int HealthCurrent { get; set; }
        public int Health { get; set; }

        public BarInfo[] BarsInfoBattle => new BarInfo[] {
            BarInfo.EmptyBarInfo(0, $"HP: {HealthCurrent}", Color.red, (float)HealthCurrent / Health),
        };

        public Dictionary<DamageEffect, int> ActiveEffects { get; internal set; }

    }

}