using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Battle
{
    public partial class HeroAnimation : MonoBehaviour, IHeroAnimation
    {
        private const string AnimBoolAttack = "attack";
        private const string AnimBoolRangeAttack = "range";
        private const string AnimBoolHit = "hit";
        private const string AnimBoolDeath = "death";
        private const string AnimBoolRun = "run";
        
        private const string AnimStageIdle = "Idle";

        private Animator animator;

        private bool initialized;
        private readonly WaitForSeconds defaultWait = new(.35f);
        private readonly WaitForSeconds waitOneSec = new(1f);
        
        private Overlay overlay;
        private Vector3 scaleBeforeMove = Vector3.zero;

        public void SetOverlay(Overlay overlay)
        {
            this.overlay = overlay;
            overlay.Attach(transform);
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (initialized)
                ResetAnimations();
        }
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        public void Initialize()
        {
            ResetAnimations();
            overlay.ResetOverlayInfo();
            initialized = true;
        }

        public void HideOverlay()
        {
            overlay.ResetOverlayInfo();
            overlay.ResetBarsAndEffects();
        }

        internal void SetHero(Hero hero)
        {
            if (hero.HeroType == HeroType.NA)
            {    
                animator.runtimeAnimatorController = null;
                HideOverlay();
            }
            else
            {
                if (hero.TeamId == 1)
                {
                    var lr = transform.localRotation;
                    lr.y = 180f;
                    transform.localRotation = lr;
                }

                var res = Resources.Load($"Hero_{hero.Id}");
                if (res != null)
                    animator.runtimeAnimatorController =
                        Instantiate(res) as RuntimeAnimatorController;
                else
                    animator.runtimeAnimatorController = null;

                overlay.SetBarsEndEffectsInfo(
                    hero.BarsInfoBattle,
                    hero.ActiveEffects);
            }

        }

        private void ResetAnimations()
        {
            StopAllCoroutines();

            animator.SetBool(AnimBoolDeath, false);
            animator.SetBool(AnimBoolHit, false);
            animator.SetBool(AnimBoolAttack, false);

            animator.Play(AnimStageIdle);
        }

        public void SetOverlayInfo(TurnStageInfo info)
        {
            StartCoroutine(OverlayInfoCoroutine(info));
        }

        public void FlashEffect(DamageEffect effect)
        {
            overlay.FlashEffect(effect);
        }
        internal void Run(float sec = -1f, Vector3 position = default)
        {
            StartCoroutine(TimedAnimationCorotine(AnimBoolRun, sec));
            StartCoroutine(MoveSpriteCoroutine(position, sec));
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

        private IEnumerator OverlayInfoCoroutine(TurnStageInfo info)
        {
            overlay.SetOverlayInfo(info);

            yield return waitOneSec;

            overlay.ResetOverlayInfo();
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

        private IEnumerator MoveSpriteCoroutine(Vector3 position = default, float sec = -1f)
        {
            var mt = transform.parent.transform;
            var delta = 0f;
            mt.localPosition = Vector3.zero;
            var move = position - transform.position;
            
            scaleBeforeMove = transform.localScale;

            while (delta <= sec)
            {
                mt.position += move * Time.deltaTime;
                
                var targetScale = scaleBeforeMove * (1 + 2 * delta / sec);
                targetScale.z = 1;
                transform.localScale = targetScale;
                
                delta += Time.deltaTime;

                yield return null;
            }
        }

        internal void MoveSpriteBack()
        {
            transform.parent.transform.localPosition = Vector3.zero;
            
            if (scaleBeforeMove != Vector3.zero)
                transform.localScale = scaleBeforeMove;
        }
    }
}