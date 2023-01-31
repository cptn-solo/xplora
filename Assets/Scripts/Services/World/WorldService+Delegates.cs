using Assets.Scripts.UI.Data;
using System;
using UnityEngine;

namespace Assets.Scripts.Services
{

    public partial class WorldService
    {
        public Spawner UnitSpawner { get; set; }
        public HexCoordResolver CoordResolver { get; set; }
        public CellCoordinatesResolver CellCoordinatesResolver { get; set; }
        public CellIndexResolver CellIndexResolver { get; set; }
        public WorldPositionResolver WorldPositionResolver { get; set; }

        public HexCoordAccessor CoordHoverer { get; set; }
        public HexCoordAccessor CoordSelector { get; set; }

        public TerrainProducer TerrainProducer { get; set; }
        
    }
}