using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public partial class BaseEntityView<T> : ITransform
        where T : struct
    {
        public Transform Transform => transform;

        public void OnGameObjectDestroy()
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            var pool = world.GetPool<EntityViewRef<T>>();
            if (!pool.Has(entity))
                return;

            ref var entityViewRef = ref pool.Get(entity);
            entityViewRef.EntityView = null;
            pool.Del(entity);
        }

    }
}
