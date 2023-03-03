using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService // Runloop
    {
        protected Coroutine runloopCoroutine = null;

        protected WaitForSeconds TickTimer { get; set; } = new(.5f);

        protected bool runLoopActive = false;

        protected void StopRunloopCoroutine()
        {
            runLoopActive = false;

            if (runloopCoroutine != null)
                StopCoroutine(runloopCoroutine);

            runloopCoroutine = null;
        }

        protected IEnumerator RunloopCoroutine()
        {
            runLoopActive = true;

            while (runLoopActive)
            {

                ecsRunSystems?.Run();

                yield return TickTimer;

            }

        }

    }

}

