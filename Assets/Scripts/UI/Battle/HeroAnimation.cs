using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class HeroAnimation : MonoBehaviour, IHeroAnimation
    {
        private const string AnimBoolAttack = "attack";
        private const string AnimBoolRangeAttack = "range";
        private const string AnimBoolHit = "hit";
        private const string AnimBoolDeath = "death";
        private const string AnimBoolRun = "run";

        private Animator animator;
        private bool initialized;
        private readonly WaitForSeconds defaultWait = new(.2f);

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (initialized)
                ResetAnimations();
        }

        public void Initialize()
        {
            ResetAnimations();
            initialized = true;
        }

        private void ResetAnimations()
        {
            StopAllCoroutines();

            animator.SetBool(AnimBoolDeath, false);
            animator.SetBool(AnimBoolHit, false);
            animator.SetBool(AnimBoolAttack, false);
            animator.Play("Idle");
        }

        public void Attack(bool range = false)
        {
            StartCoroutine(TimedAnimationCorotine(AnimBoolAttack, range ? 1 : -1));
            if (range)
                StartCoroutine(TimedAnimationCorotine(AnimBoolRangeAttack, .5f, .6f));

        }
        public void Hit(bool lethal)
        {
            if (lethal)
                StartCoroutine(TimedAnimationCorotine(AnimBoolDeath));
            else
                StartCoroutine(TimedAnimationCorotine(AnimBoolHit));
        }
        public void Death()
        {
            StartCoroutine(TimedAnimationCorotine(AnimBoolHit));
        }

        private IEnumerator TimedAnimationCorotine(
            string animationCode, 
            float stopAfterSec = -1f, 
            float delaySec = -1f)
        {
            var stopAfter = defaultWait;
            
            if (stopAfterSec > 0f)
                stopAfter = new WaitForSeconds(stopAfterSec);
            
            if (delaySec > 0f)
                yield return new WaitForSeconds(delaySec);
            
            animator.SetBool(animationCode, true);
            yield return stopAfter;
            animator.SetBool(animationCode, false);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        internal void SetHero(Hero hero)
        {
            if (hero.HeroType == HeroType.NA)
                animator.runtimeAnimatorController = null;
            else
            {
                
                if (hero.TeamId == 1)
                {
                    var ls = transform.localScale;
                    ls.x = -1;
                    transform.localScale = ls;
                }

                var res = Resources.Load($"Hero_{hero.Id}");
                if (res != null)
                    animator.runtimeAnimatorController =
                        Instantiate(res) as RuntimeAnimatorController;
                else
                    animator.runtimeAnimatorController = null;
            }

        }

        internal void Run(float sec = -1f)
        {
            StartCoroutine(TimedAnimationCorotine(AnimBoolRun, sec));
        }
    }
}