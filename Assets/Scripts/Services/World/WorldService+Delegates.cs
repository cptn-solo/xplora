namespace Assets.Scripts.Services
{

    public partial class WorldService // Delegates
    {
        public HexCoordResolver CoordResolver { get; internal set; }
        public CellCoordinatesResolver CellCoordinatesResolver { get; internal set; }
        public CellIndexResolver CellIndexResolver { get; internal set; }
        public WorldPositionResolver WorldPositionResolver { get; internal set; }

        public HexCoordAccessor CoordHoverer { get; internal set; }
        public HexCoordAccessor CoordBeforeSelector { get; internal set; }

        public TerrainProducer TerrainProducer { get; internal set; }
    }
}