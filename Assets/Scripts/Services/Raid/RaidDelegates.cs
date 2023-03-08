using UnityEngine;
using Assets.Scripts.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.Services
{

    public delegate void UnitSpawnerCallback();
    public delegate void DestroyUnitsCallback();

    public delegate bool LibraryActionWithHero(Hero hero);
    public delegate void DeployWorldUnit(Unit unit, int cellId);
    public delegate UnitOverlay DeployWorldUnitOverlay(Unit unit);


    /// <summary>
    /// Spawns a unit appropriate for the specified Hero
    /// </summary>
    /// <param name="pos">World position for the unit to be spawned at</param>
    /// <param name="hero">Hero structure for the unit to be constructed with</param>
    /// <returns>A unit representing a hero in the world</returns>
    public delegate Unit UnitSpawner(Vector3 pos, Hero hero, bool isPlayer, UnitSpawnerCallback onSpawned);

    public delegate UnitOverlay UnitOverlaySpawner(Transform anchor);
}

