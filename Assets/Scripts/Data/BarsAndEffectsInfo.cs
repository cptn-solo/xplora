using System.Collections.Generic;
using Assets.Scripts.UI.Common;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BarsAndEffectsInfo
    {
        // effects state carries attackers hp and effects
        // while other states - targets hp and effects
        public int HealthCurrent { get; set; }
        public int Health { get; set; }
        public int Speed { get; set; }

        public BarInfo[] BarsInfoBattle => new BarInfo[] {
            BarInfo.EmptyBarInfo(0, $"HP: {HealthCurrent}", Color.red, (float)HealthCurrent / Health),
            BarInfo.EmptyBarInfo(1, $"Speed: {Speed}", null, Speed / Mathf.Max(Speed, 10f)),
        };

        public Dictionary<DamageEffect, int> ActiveEffects { get; internal set; }

    }

}