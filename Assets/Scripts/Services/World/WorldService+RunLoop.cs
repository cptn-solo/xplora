using System.Collections;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.Services
{

    public partial class WorldService // RunLoop
    {
        private Coroutine worldRunloopCoroutine = null;

        private readonly WaitForSeconds TickTimer = new(.5f);

        private IEnumerator WorldStateLoopCoroutine()
        {

            while (true)
            {
                switch (worldState)
                {
                    case WorldState.NA:
                        break;
                    case WorldState.AwaitingTerrain:

                        if (TerrainProducer != null)
                        {
                            worldState = WorldState.TerrainBeingGenerated;
                            ProduceEcsWorld();
                        }

                        break;
                    case WorldState.TerrainBeingGenerated:
                        break;
                    case WorldState.SceneReady:
                        break;
                    case WorldState.AwaitingTerrainDestruction:
                        worldState = WorldState.TerrainBeingDestoyed;
                        DestroyEcsWorld();
                        break;
                    case WorldState.TerrainBeingDestoyed:
                        break;
                    default:
                        break;
                }

                ecsSystems.Run();

                yield return TickTimer;

            }

        }
    }
}