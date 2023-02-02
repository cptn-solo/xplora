using UnityEngine;
using System.Collections;
using Assets.Scripts.Data;

namespace Assets.Scripts.Services
{
    public partial class RaidService // RunLoop
    {
        private Coroutine raidRunloopCoroutine = null;

        private readonly WaitForSeconds TickTimer = new(.5f);

        private bool runLoopActive = false;

        private IEnumerator RaidRunloopCoroutine()
        {
            runLoopActive = true;

            while (runLoopActive)
            {
                switch (State)
                {
                    default:
                        break;
                }

                ecsRaidSystems?.Run();

                yield return TickTimer;

            }

        }

    }
}


