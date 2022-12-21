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
        private readonly WaitForSeconds defaultWait = new(.2f);
        private readonly WaitForSeconds waitOneSec = new(1f);
        
        [SerializeField] private EffectsContainer effectsContainer;
        [SerializeField] private Image piercedImage;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI effectText;               

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
            ResetOverlayInfo();
            initialized = true;
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

        private void ResetAnimations()
        {
            StopAllCoroutines();

            animator.SetBool(AnimBoolDeath, false);
            animator.SetBool(AnimBoolHit, false);
            animator.SetBool(AnimBoolAttack, false);

            animator.Play(AnimStageIdle);
        }

        private void ResetEffects()
        {
            effectsContainer.SetEffects(new DamageEffect[] { });
        }

        private void ResetOverlayInfo()
        {
            piercedImage.gameObject.SetActive(false);
            damageText.text = "";
            effectText.text = "";

            effectsContainer.SetEffects(new DamageEffect[] { });
        }

        public void SetOverlayInfo(TurnStageInfo info)
        {
            StartCoroutine(OverlayInfoCoroutine(info));
        }

        public void SetEffects(DamageEffect[] effects)
        {
            StartCoroutine(EffectsCoroutine(effects));
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

        private IEnumerator EffectsCoroutine(DamageEffect[] effects)
        {
            effectsContainer.SetEffects(effects);
            yield return waitOneSec;
            ResetEffects();
        }
        private IEnumerator OverlayInfoCoroutine(TurnStageInfo info)
        {
            piercedImage.gameObject.SetActive(info.IsPierced);
            damageText.text = info.Damage > 0 ? $"{info.Damage}" : "";
            effectText.text = info.EffectText;

            effectsContainer.SetEffects(info.Effect != DamageEffect.NA ?
                new DamageEffect[] { info.Effect } :
                new DamageEffect[] { });

            yield return waitOneSec;

            ResetOverlayInfo();
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
            var delta = sec;
            mt.localPosition = Vector3.zero;
            var move = position - transform.position;

            while (delta > 0f)
            {
                mt.position += move * Time.deltaTime;

                delta -= Time.deltaTime;

                yield return null;
            }
        }

        internal void MoveSpriteBack()
        {
            transform.parent.transform.localPosition = Vector3.zero;
        }
    }
}