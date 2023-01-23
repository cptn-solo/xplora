﻿using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public delegate void UnitSpawnerCallback();
    public delegate void TerrainProducerCallback();

    /// <summary>
    /// Spawns a unit appropriate for the specified Hero
    /// </summary>
    /// <param name="pos">World position for the unit to be spawned at</param>
    /// <param name="hero">Hero structure for the unit to be constructed with</param>
    /// <returns>A unit representing a hero in the world</returns>
    public delegate Unit Spawner(Vector3 pos, Hero hero, UnitSpawnerCallback onSpawned);

    /// <summary>
    /// Resolves HexCoordinates of a heighboring cell
    /// </summary>
    /// <param name="coordinates">Cell HexCoordinates</param>
    /// <param name="dir">HexDirection of the desired neighbor</param>
    /// <returns>HexCoordinates of the desired neighbor or null</returns>
    public delegate HexCoordinates HexCoordResolver(
        HexCoordinates coordinates,
        HexDirection dir = HexDirection.NA);

    /// <summary>
    /// Resolves world coordinates for a given hex coordinates (cell center usually)
    /// </summary>
    /// <param name="coordinates">Hex coordinates of a cell</param>
    /// <param name="dir">Optional hex direction to the heighboring cell to be resolved instead of the given coordinates</param>
    /// <returns>World position of a given hex coordinates or a neighboring coordinates</returns>
    public delegate Vector3 WorldPositionResolver(
        HexCoordinates coordinates,
        HexDirection dir = HexDirection.NA);

    /// <summary>
    /// Resolves cell Index for the given hex coordinates
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns></returns>
    public delegate int CellIndexResolver(
        HexCoordinates coordinates);

    /// <summary>
    /// Resolves cell coordinates for the given cell index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public delegate HexCoordinates CellCoordinatesResolver(
        int index);

    /// <summary>
    /// To Highlight selected coordinates (instead of using event)
    /// </summary>
    /// <param name="coordinates"></param>
    public delegate void HexCoordAccessor(
        HexCoordinates? coordinates);
    
    /// <summary>
    /// Produces the terrain for given dimensions
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="onComplete">Callback to call when done so the world state can be updated</param>
    public delegate void TerrainProducer(
        int width,
        int height,
        TerrainProducerCallback onComplete);
}