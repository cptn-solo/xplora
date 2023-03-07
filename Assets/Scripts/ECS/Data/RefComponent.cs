﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.World;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    #region Tags

    public struct DraftTag { }
    public struct ProduceTag { }
    public struct UpdateTag { }
    public struct RetireTag { }
    public struct DestroyTag { }
    public struct GarbageTag { }

    public struct WorldPoiTag { } // to separate world (static) poi from raid poi
    public struct UsedTag { }

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
    public struct UnitRef
    {
        public Unit Unit { get; internal set; }
    }

    public struct PoiRef
    {
        public POI Poi { get; internal set; }
    }

    public struct UnitOverlayRef
    {
        public UnitOverlay Overlay;
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

    public struct RaidComp
    {
        public EcsPackedEntityWithWorld[] PlayerHeroConfigs { get; internal set; }
        public EcsPackedEntityWithWorld[] OpponentHeroConfigs { get; internal set; }
        public Asset[] Assets { get; set; }
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
        public int PrefCellIndex { get; set; }
        public int NextCellIndex { get; set; }
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
        public Tuple<int, BattleLine, int> Position { get; set; }
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


