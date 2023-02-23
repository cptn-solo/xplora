
using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.Battle
{
    public interface IHeroPosition //Slot
    {
        public Tuple<int, BattleLine, int> Position { get; }
        public void Put(Transform transform);
        public void Reset();
    }

}