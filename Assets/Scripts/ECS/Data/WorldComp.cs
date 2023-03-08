using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{

    public struct WorldComp
    {
        public EcsPackedEntity[] CellPackedEntities;
        public int PowerSourceCount { get; internal set; }
        public int HPSourceCount { get; internal set; }
        public int WatchTowerCount { get; internal set; }

        internal int POICountForType<T>()
        {
            if (typeof(T) == typeof(PowerSourceComp))
                return PowerSourceCount;
            else if (typeof(T) == typeof(HPSourceComp))
                return HPSourceCount;
            else if (typeof(T) == typeof(WatchTowerComp))
                return WatchTowerCount;
            else return 0;
        }
    }
}


