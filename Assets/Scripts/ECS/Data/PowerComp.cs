using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct OutOfPowerTag { }

    public struct PowerComp
    {
        private BarInfo[] barsInfo;
        private int currentValue;
        private bool staminaBuff;

        public bool StaminaBuff {
            get => staminaBuff;
            internal set
            {
                staminaBuff = value;
                CurrentValue = currentValue;
            }
        }

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
                var color = StaminaBuff ?
                    Color.white : 
                    relativeValue > .5f ?
                    Color.green : Color.red;
                barsInfo = new BarInfo[] {
                    BarInfo.EmptyBarInfo(
                        0,
                        $"Stamina:{currentValue}",
                        color,
                        relativeValue)
                };
            }
        }
        public BarInfo[] BarsInfo => barsInfo;
    } // value based on stamina
}


