using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldProcessVisitorSystem<T> : BaseEcsSystem
        where T: struct
    {
        protected readonly EcsPoolInject<VisitorComp> visitorPool;
        protected readonly EcsPoolInject<UsedTag> usedTagPool;
        protected readonly EcsPoolInject<UpdateTag> updateTagPool;
        protected readonly EcsPoolInject<T> visitedPool;

        protected readonly EcsFilterInject<
            Inc<T, FieldCellComp, VisitorComp>,
            Exc<UsedTag>> filter;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var visitor = ref visitorPool.Value.Get(entity);

                if (!visitor.Packed.Unpack(out var world, out var visitorEntity))
                    throw new Exception("No Visitor entity");

                if (!ValidateVisitor(world, visitorEntity))
                    return;

                ref var visitedComp = ref visitedPool.Value.Get(entity);

                var pool = world.GetPool<VisitedComp<T>>();
                if (!pool.Has(visitorEntity))
                    pool.Add(visitorEntity);

                ref var visited = ref pool.Get(visitorEntity);
                visited.Info = visitedComp;

                if (!usedTagPool.Value.Has(entity))
                    usedTagPool.Value.Add(entity);

                if (!updateTagPool.Value.Has(entity))
                    updateTagPool.Value.Add(entity);


            }
        }

        protected virtual bool ValidateVisitor(EcsWorld world, int visitorEntity)
        {
            return true;
        }
    }
}