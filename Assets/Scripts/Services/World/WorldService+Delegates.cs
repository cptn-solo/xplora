using Assets.Scripts.UI.Data;
using System;
using UnityEngine;

namespace Assets.Scripts.Services
{

    public partial class WorldService : MonoBehaviour
    {
        public Spawner UnitSpawner { get; set; }
        public HexCoordResolver CoordResolver { get; set; }
        public CellCoordinatesResolver CellCoordinatesResolver { get; set; }
        public CellIndexResolver CellIndexResolver { get; set; }
        public WorldPositionResolver WorldPositionResolver { get; set; }
        public HexCoordHighlighter CoordHighlighter { get; set; }

        public TerrainProducer TerrainProducer { get; set; }
        
        private readonly WaitForSeconds TickTimer = new(1f);

    }
}