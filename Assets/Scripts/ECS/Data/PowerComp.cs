﻿using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct OutOfPowerTag { }

    public struct PowerComp
    {
        private BarInfo[] barsInfo;
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


