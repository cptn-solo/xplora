
using System;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Battle
{
    public interface IHeroPosition //Slot
    {
        public HeroPosition Position { get; }
        public void Put(Transform transform);
        public void Reset();
        public void ToggleVisual(bool toggle);
    }

}