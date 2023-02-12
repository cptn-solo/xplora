using System.Collections.Generic;
using Assets.Scripts.UI.Common;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct OutOfPowerTag { }

    public struct PowerComp
    {
        private List<BarInfo> barsInfo;
        private int currentValue;

        public int InitialValue { get; internal set; }
        public int CurrentValue
        {
            get => currentValue;
            internal set
            {
                currentValue = Mathf.Max(0, value);

                var relativeValue =
                    InitialValue <= 0 ? 0f :
                    (float)currentValue / InitialValue;
                var color = relativeValue > .5f ?
                    UnityEngine.Color.green : UnityEngine.Color.red;
                barsInfo = new() {
                    BarInfo.EmptyBarInfo(
                        0,
                        $"Stamina:{currentValue}",
                        color,
                        relativeValue)
                };
            }
        }
        public List<BarInfo> BarsInfo => barsInfo;
    } // value based on stamina
}


