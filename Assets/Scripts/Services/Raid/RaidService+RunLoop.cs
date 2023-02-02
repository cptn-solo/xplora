using UnityEngine;
using System.Collections;
using Assets.Scripts.Data;

namespace Assets.Scripts.Services
{
    public partial class RaidService // RunLoop
    {
        private Coroutine raidRunloopCoroutine = null;

        private readonly WaitForSeconds TickTimer = new(.5f);

        private IEnumerator RaidRunloopCoroutine()
        {
            while (true)
            {
                switch (State)
                {
                    case RaidState.AwaitingUnits:
                        SpawnUnits();
                        break;

                    case RaidState.PrepareBattle:
                        DestroyUnits(StartBattle);
                        break;

                    default:
                        break;
                }

                ecsRaidSystems?.Run();

                yield return TickTimer;

            }

        }

    }
}


