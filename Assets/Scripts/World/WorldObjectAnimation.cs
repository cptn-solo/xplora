using Assets.Scripts.UI.Data;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class WorldObjectAnimation : MonoBehaviour
    {

        protected Animator animator;

        private bool initialized;
        private readonly WaitForSeconds defaultWait = new(.35f);
        private readonly WaitForSeconds waitOneSec = new(1f);

        protected virtual string AnimStageIdle { get; } = "Idle";

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
            initialized = true;
        }
        private void ResetAnimations()
        {
            StopAllCoroutines();

            animator.Play(AnimStageIdle);
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

        protected IEnumerator ToggleAnimationCorotine(
            string animationCode,
            bool toggle = true,
            float delaySec = -1f)
        {

            if (delaySec > 0f)
                yield return new WaitForSeconds(delaySec);

            animator.SetBool(animationCode, toggle);
        }

    }
}