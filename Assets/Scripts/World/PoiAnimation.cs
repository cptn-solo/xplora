using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class PoiAnimation : WorldObjectAnimation
    {
        private const string AnimStageActive = "Active";
        private const string AnimStageInactive = "Inactive";

        private string currentSgate = AnimStageActive;

        protected override string AnimStageIdle => currentSgate;

        internal void SetActive(bool toggle)
        {
            currentSgate = toggle ? AnimStageActive : AnimStageInactive;

            Initialize();
        }

        internal int PoiTypeIndexForType<T>()
        {
            if (typeof(T) == typeof(PowerSourceComp))
                return 0;
            else if (typeof(T) == typeof(HPSourceComp))
                return 1;
            else if (typeof(T) == typeof(WatchTowerComp))
                return 2;
            else return 0;
        }

        internal void SetRuntimeAnimator<T>()
        {
            var idx = PoiTypeIndexForType<T>();
            
            var res = Resources.Load($"Animators/Poi_{idx}");
            if (res != null)
                animator.runtimeAnimatorController =
                    Instantiate(res) as RuntimeAnimatorController;
            else
                animator.runtimeAnimatorController = null;
        }
    }
}