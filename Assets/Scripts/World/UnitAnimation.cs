using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class UnitAnimation : MonoBehaviour
    {
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

                if (overlay != null)
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

                if (overlay != null)
                    overlay.SetBarsEndEffectsInfo(
                        hero.BarsInfoBattle,
                        hero.ActiveEffects);
            }
        }

        private void ResetAnimations()
        {
            StopAllCoroutines();

            animator.Play(AnimStageIdle);
        }

        public void SetOverlayInfo(TurnStageInfo info)
        {
            StartCoroutine(OverlayInfoCoroutine(info));
        }

        internal void Run(bool toggle)
        {
            StartCoroutine(ToggleAnimationCorotine(AnimBoolRun, toggle));
        }

        internal void Zoom(float sec = -1f)
        {
            StartCoroutine(ZoomSpriteCoroutine(sec));
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

        private IEnumerator ToggleAnimationCorotine(
            string animationCode,
            bool toggle = true,
            float delaySec = -1f)
        {

            if (delaySec > 0f)
                yield return new WaitForSeconds(delaySec);

            animator.SetBool(animationCode, toggle);
        }


        private IEnumerator ZoomSpriteCoroutine(float sec = -1f)
        {
            var delta = 0f;

            scaleBeforeMove = transform.localScale;

            while (delta <= sec)
            {
                var targetScale = scaleBeforeMove * (1 + 1 * delta / sec);
                targetScale.z = 1;
                transform.localScale = targetScale;

                delta += Time.deltaTime;

                yield return null;
            }
        }
    }
}