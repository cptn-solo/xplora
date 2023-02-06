using Assets.Scripts.UI.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.World
{

    public class UnitAnimation : WorldObjectAnimation
    {
        private const string AnimBoolRun = "run";

        internal void SetHero(Hero hero)
        {
            if (hero.HeroType == HeroType.NA)
            {
                animator.runtimeAnimatorController = null;
            }
            else
            {
                if (hero.TeamId == 1)
                {
                    var lr = transform.localRotation;
                    lr.y = 180f;
                    transform.localRotation = lr;
                }

                var res = Resources.Load($"Animators/Hero_{hero.Id}");
                if (res != null)
                    animator.runtimeAnimatorController =
                        Instantiate(res) as RuntimeAnimatorController;
                else
                    animator.runtimeAnimatorController = null;
            }
        }

        internal void Run(bool toggle)
        {
            StartCoroutine(ToggleAnimationCorotine(AnimBoolRun, toggle));
        }

    }
}