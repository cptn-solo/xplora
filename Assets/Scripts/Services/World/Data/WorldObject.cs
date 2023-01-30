using Assets.Scripts.UI.Data;
using Assets.Scripts.World;

namespace Assets.Scripts.Services.Data
{
    public struct WorldObject
    {
        public WorldObjectType ObjectType { get; private set; }
        public int CellIndex { get; set; }
        public Hero Hero { get; set; }
        public Unit Unit { get; set; }

        internal static WorldObject Default { get; } = 
            new WorldObject {
                ObjectType = WorldObjectType.NA,
                CellIndex = -1,
                Hero = default,
                Unit = null,
            };

        internal static WorldObject Create(int cellIndex, Hero hero, Unit unit)
        {
            WorldObject obj = default;

            obj.ObjectType = WorldObjectType.Unit;
            obj.CellIndex = cellIndex;
            obj.Hero = hero;
            obj.Unit = unit;
            
            return obj;
        }
    }
}