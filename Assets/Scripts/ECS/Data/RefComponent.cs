using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    #region Tags

    public struct DraftTag { }
    public struct DraftTag<T> { }
    public struct ProduceTag { }
    public struct UpdateTag { }
    public struct UpdateTag<T> { }
    public struct RetireTag { }
    public struct DestroyTag { }
    public struct GarbageTag { }
    public struct SelectedTag { }
    public struct SelectedTag<T> { }

    public struct WorldPoiTag { } // to separate world (static) poi from raid poi
    public struct UsedTag { }

    public struct UnveilCellsTag { } // marker for cells to unhide contents of
    public struct VeilCellsTag { } // marker for cells to unhide contents of

    public struct VisibleTag { } // for units and terrain
    public struct ExploredTag { } // for terrain
    public struct NonPassableTag { } // for terrain

    public struct VisibilityUpdateTag { }
    public struct NoStaminaDrainBuffTag { }

    public struct UpdateHPTag { }
    public struct UpdateAssetBalanceTag { }
    public struct UpdateBuffsTag<T> { }
    public struct DebuffTag<T> { }

    #endregion

    #region Refs

    public struct EntityViewFactoryRef<T>
        where T: struct
    {
        public EntityViewFactory<T> FactoryRef;
    }
    public struct PackedEntityRef
    {
        public EcsPackedEntityWithWorld PackedEntity { get; internal set; }
    }

    public struct VisibilityRef
    {
        public IVisibility visibility;
    }

    public struct EntityViewRef<T>
    {
        public IEntityView<T> EntityView;
    }

    public struct ItemsContainerRef<T>
    {
        public IItemsContainer<T> Container;        
    }

    public struct TransformRef<T>
    {
        public Transform Transform;
    }

#endregion

#region Comps

    public struct RaidComp
    {
        public EcsPackedEntityWithWorld[] PlayerHeroConfigs { get; internal set; }
        public EcsPackedEntityWithWorld[] OpponentHeroConfigs { get; internal set; }
        public Asset[] Assets { get; set; }
    }

    public struct BattlePotComp
    {
        public Asset[] PotAssets { get; set; } // the winner takes it all
    }

    public struct BattleAftermathComp
    {
        public bool Won { get; internal set; }
        public Asset[] Trophy { get; internal set; }
    }

    public struct PlayerComp
    {
    }

    public struct OpponentComp
    {
    }

    public struct HeroComp : IPackedWithWorldRef
    {
        public EcsPackedEntityWithWorld Hero { get; internal set; }

        public EcsPackedEntityWithWorld Packed => Hero;
    }
    public struct TeamComp { } // temp: reference to a team of heroes
    public struct StaminaComp { } // can be filled and drained
    public struct SightRangeComp {
        public int Range { get; internal set; }
    }
    public struct PowerSourceComp { } // 
    public struct HPSourceComp { } // 
    public struct WatchTowerComp { } //  

    public struct VisitCellComp {
        public int CellIndex { get; internal set; }
    } // triggers systems aware of player being visiting a cell

    public struct VisitorComp: IPackedWithWorldRef {
        public EcsPackedEntityWithWorld Packed { get; set; }
        public int PrevCellIndex { get; set; }
        public int NextCellIndex { get; set; }
    }

    public struct ActiveTraitHeroComp<T>
    {
        public EcsPackedEntityWithWorld PackedHeroInstanceEntity { get; set; }
        public Hero Hero { get; set; }
        public int MaxLevel { get; set; }
        public HeroTrait Trait { get; set; }
    }

    public struct VisitedComp<T> where T: struct
    {
        public T Info;
    }

    public struct RefillComp
    {
        public int Value { get; internal set; }
    }

    public struct DrainComp
    {
        public int Value { get; internal set; }
    }

    public struct FieldCellComp // cell on the world's map
    {
        public int CellIndex { get; internal set; }
    }

    public struct TerrainTypeComp
    {
        public TerrainType TerrainType { get; set; }
    }

    public struct TerrainAttributeComp
    {
        public TerrainAttribute TerrainAttribute { get; set; }
    }

    public struct POIComp // any non-player object on the field
    {
    }

    /// <summary>
    /// Assigns Hero to a position on the library or battle screen (shared, player team or
    /// enemy team + slot index
    /// </summary>
    public struct PositionComp
    {
        /// <summary>
        /// team id + battle line + slot index 
        /// </summary>
        public HeroPosition Position { get; set; }
    }

    public struct LibraryFieldComp
    {
        public Dictionary<HeroPosition, IHeroPosition> Slots { get; set; }
        public HeroPosition[] SlotPositions => Slots.Keys.ToArray();
    }

    public struct BattleFieldComp {
        public Dictionary<HeroPosition, IHeroPosition> Slots { get; set; }
        public HeroPosition[] SlotPositions => Slots.Keys.ToArray();
    }

    public struct HostileComp { } // hostile unit/poi
    public struct FriendlyComp { } // friendly unit/poi
    public struct NeutralComp { } // neutral unit/poi

    public struct DummyBuff : IIntValue
    {
        public int Value { get; set; }
    }

    public struct BuffComp<T> : IIntValue
    {
        public int Value { get; set; }
        public int Usages { get; set; }
        public Color IconColor { get; set; }
        public BundleIcon Icon { get; set; }
    }

    public struct DamageRangeComp
    {
        public int Max { get; set; }
        public int Min { get; set; }

        public int RandomDamage => UnityEngine.Random.Range(Min, Max + 1);

    }

    #endregion

    public struct RefComponent
    {
    }
}


