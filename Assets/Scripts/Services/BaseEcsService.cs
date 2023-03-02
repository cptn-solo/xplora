using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts.ECS.Data;
using System;

namespace Assets.Scripts.Services
{
    public class BaseEcsService : MonoBehaviour
    {
        protected EcsWorld ecsWorld { get; set; }

        protected IEcsSystems ecsRunSystems { get; set; }
        protected IEcsSystems ecsInitSystems { get; set; }

        internal void RegisterTransformRef<T>(ITransform<T> transformRefOrigin)
        {
            if (ecsWorld == null)
                return;

            var entity = ecsWorld.NewEntity();
            ref var transformRef = ref ecsWorld.GetPool<TransformRef<T>>().Add(entity);
            transformRef.Transform = transformRefOrigin.Transform;
        }

        internal void UnregisterTransformRef<T>(ITransform transformRefOrigin)
        {
            if (ecsWorld == null)
                return;

            var filter = ecsWorld.Filter<TransformRef<T>>().End();
            var pool = ecsWorld.GetPool<TransformRef<T>>();
            var garbage = ecsWorld.GetPool<GarbageTag>();

            foreach (var entity in filter)
            {
                ref var transformRef = ref pool.Get(entity);
                transformRef.Transform = null;
                garbage.Add(entity); // so some other system can
                                     // remove other dependencies
                                     // (if any)

            }

        }

        internal void UnlinkCardRefs<T>()
        {
            var entityViewRefPool = ecsWorld.GetPool<EntityViewRef<T>>();
            var filter = ecsWorld.Filter<EntityViewRef<T>>().End();

            foreach (var entity in filter)
            {
                ref var entityViewRef = ref entityViewRefPool.Get(entity);
                entityViewRef.EntityView = null;
                entityViewRefPool.Del(entity);
            }
        }

        internal T GetDataForPackedEntity<T>(EcsPackedEntityWithWorld? packed)
            where T : struct
        {
            if (packed != null && packed.Value.Unpack(out var world, out var entity))
                return world.GetPool<T>().Get(entity);

            return default;
        }

        internal void EnqueueEntityViewUpdate<T>(EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.UpdateData();
        }

        internal void EnqueueEntityViewDestroy<T>(EcsPackedEntityWithWorld packedEntity)
            where T : struct
        {
            if (!packedEntity.Unpack(out var world, out var entity))
                throw new Exception("No entity");

            ref var viewRef = ref world.GetPool<EntityViewRef<T>>().Get(entity);
            viewRef.EntityView.Destroy();
        }



    }

}

