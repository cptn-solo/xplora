using System;
using System.Collections;
using Assets.Scripts.World.HexMap;

namespace Assets.Scripts.Services
{

    public partial class WorldService
    {
        private IEnumerator WorldStateLoopCoroutine()
        {
            isWorldStateLoopActive = true;

            while (isWorldStateLoopActive)
            {
                if (worldState != WorldState.PrepareBattle &&
                    worldState != WorldState.InBattle &&
                    worldState != WorldState.Aftermath)
                {
                    if (TerrainProducer == null && UnitSpawner == null)
                        worldState = WorldState.NA;
                }

                yield return TickTimer;

                switch (worldState)
                {
                    case WorldState.NA:
                    case WorldState.Aftermath:

                        if (TerrainProducer != null && UnitSpawner != null)
                            worldState = WorldState.DelegatesAttached;

                        break;
                    case WorldState.DelegatesAttached:
                        
                        GenerateTerrain();
                        
                        break;
                    case WorldState.TerrainBeingGenerated:
                        break;
                    case WorldState.SceneReady:

                        SpawnUnits();

                        break;
                    case WorldState.UnitsBeingSpawned:
                        break;
                    case WorldState.UnitsSpawned:
                        break;
                    case WorldState.PrepareBattle:
                    case WorldState.InBattle:
                        break;

                    default:
                        break;
                }

            }

        }
    }
}