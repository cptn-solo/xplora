using UnityEngine;
using System.Collections;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System;
using Assets.Scripts.World;

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

                ecsRunSystems?.Run();

                yield return TickTimer;

            }

        }
    }
}


