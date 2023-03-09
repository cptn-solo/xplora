using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.World.HexMap;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System;
using System.Linq;

namespace Assets.Scripts.ECS.Systems
{

    public class WorldVisitorSightUpdateSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<VisitorComp> visitorPool;

        private readonly EcsFilterInject<Inc<FieldCellComp, VisitorComp>> filter;

        private readonly EcsCustomInject<WorldService> worldService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var visitor = ref visitorPool.Value.Get(entity);

                if (!visitor.Packed.Unpack(out var world, out var visitorEntity))
                    throw new Exception("No Visitor entity");

                var sightPool = world.GetPool<SightRangeComp>();
                ref var sightRange = ref sightPool.Get(visitorEntity);

                PrepareVisibilityUpdate(visitor.PrevCellIndex, visitor.NextCellIndex, sightRange.Range);
            }
        }

        internal void PrepareVisibilityUpdate(int prevCellIndex, int nextCellIndex, int range)
        {
            if (!worldService.Value.TryGetCellEntity(nextCellIndex, out var cellEntity, out var world))
                return;

            var width = worldService.Value.WorldWidth;
            var height = worldService.Value.WorldHeight;

            var coordNext = worldService.Value.CellCoordinatesResolver(nextCellIndex);
            coordNext.RangeFromCoordinates(range, new Vector4(width, height), out var rangeCoordNext);

            var toHide = ListPool<int>.Get();
            var toShow = ListPool<int>.Get();

            if (prevCellIndex >= 0 &&
                prevCellIndex != nextCellIndex &&
                worldService.Value.TryGetCellEntity(prevCellIndex, out var cellEntityPrev, out var _))
            {
                var coordPrev = worldService.Value.CellCoordinatesResolver(prevCellIndex);
                coordPrev.RangeFromCoordinates(range, new Vector4(width, height), out var rangeCoordPrev);

                toHide.AddRange(rangeCoordPrev.Except(rangeCoordNext));
                toShow.AddRange(rangeCoordNext.Except(rangeCoordPrev));
            }
            else
            {
                toShow.AddRange(rangeCoordNext);
            }

            var veilTagPool = world.GetPool<VeilCellsTag>();
            foreach (var refIndex in toHide)
            {
                if (!worldService.Value.TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!veilTagPool.Has(refCellEntity))
                    veilTagPool.Add(refCellEntity);
            }

            var unveilTagPool = world.GetPool<UnveilCellsTag>();
            foreach (var refIndex in toShow)
            {
                if (!worldService.Value.TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!unveilTagPool.Has(refCellEntity))
                    unveilTagPool.Add(refCellEntity);
            }

            ListPool<int>.Add(toHide);
            ListPool<int>.Add(toShow);
        }

    }
}