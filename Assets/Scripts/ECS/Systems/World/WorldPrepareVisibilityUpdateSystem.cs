using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.World.HexMap;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldPrepareVisibilityUpdateSystem : IEcsRunSystem
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

                UpdateVisibilityInRange(visitor.PrevCellIndex, visitor.NextCellIndex, sightRange.Range);
            }
        }

        internal void UpdateVisibilityInRange(int prevCellIndex, int nextCellIndex, int range)
        {
            if (!worldService.Value.TryGetCellEntity(nextCellIndex, out var cellEntity, out var world))
                return;

            var width = worldService.Value.WorldWidth;
            var height = worldService.Value.WorldHeight;

            var coordNext = worldService.Value.CellCoordinatesResolver(nextCellIndex);
            var rangeCoordNext = coordNext.RangeFromCoordinates(range, new Vector4(width, height));

            var toHide = ListPool<HexCoordinates>.Get();
            var toShow = ListPool<HexCoordinates>.Get();

            toShow.AddRange(rangeCoordNext);

            if (prevCellIndex >= 0 &&
                prevCellIndex != nextCellIndex &&
                worldService.Value.TryGetCellEntity(prevCellIndex, out var cellEntityPrev, out var _))
            {
                var coordPrev = worldService.Value.CellCoordinatesResolver(prevCellIndex);
                var rangeCoordPrev = coordPrev.RangeFromCoordinates(range, new Vector4(width, height));

                toHide.AddRange(rangeCoordPrev);

                foreach (var check in rangeCoordNext)
                    toHide.Remove(check);

                foreach (var check in rangeCoordPrev)
                    toShow.Remove(check);
            }

            var visibilityUpdateTagPool = world.GetPool<VisibilityUpdateTag>();
            var visibilityRefPool = world.GetPool<VisibilityRef>();
            var visibleTagPool = world.GetPool<VisibleTag>();
            var exploredTagPool = world.GetPool<ExploredTag>();

            foreach (var refCoord in toHide)
            {
                var refIndex = worldService.Value.CellIndexResolver(refCoord);

                if (!worldService.Value.TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!visibilityRefPool.Has(refCellEntity))
                    continue;

                ref var visibilityRef = ref visibilityRefPool.Get(refCellEntity);
                visibilityRef.visibility.DecreaseVisibility();

                if (visibleTagPool.Has(refCellEntity))
                    visibleTagPool.Del(refCellEntity);

                if (!visibilityUpdateTagPool.Has(refCellEntity))
                    visibilityUpdateTagPool.Add(refCellEntity);
            }


            foreach (var refCoord in toShow)
            {
                var refIndex = worldService.Value.CellIndexResolver(refCoord);

                if (!worldService.Value.TryGetCellEntity(refIndex, out var refCellEntity, out var _))
                    continue;

                if (!visibilityRefPool.Has(refCellEntity))
                    continue;

                ref var visibilityRef = ref visibilityRefPool.Get(refCellEntity);
                visibilityRef.visibility.IncreaseVisibility();

                if (!visibleTagPool.Has(refCellEntity))
                    visibleTagPool.Add(refCellEntity);

                if (!exploredTagPool.Has(refCellEntity))
                    exploredTagPool.Add(refCellEntity); // will stay explored after hide

                if (!visibilityUpdateTagPool.Has(refCellEntity))
                    visibilityUpdateTagPool.Add(refCellEntity);
            }

            ListPool<HexCoordinates>.Add(toHide);
            ListPool<HexCoordinates>.Add(toShow);
        }

    }
}