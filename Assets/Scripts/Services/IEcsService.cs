using Leopotam.EcsLite;
using UnityEngine.Events;

namespace Assets.Scripts.Services
{
    public interface IEcsService
    {
        public void RegisterTransformRef<T>(ITransform<T> transformRefOrigin);
        public void UnregisterTransformRef<T>(ITransform transformRefOrigin);

        public void RegisterEntityViewFactory<T>(EntityViewFactory<T> factory)
            where T: struct;

        public void UnregisterEntityViewFactory<T>()
            where T : struct;

        public void RegisterEntityView<T>(
            IEntityView<T> entityView)
            where T : struct;

        public void RegisterEntityView<T>(
            IEntityView<T> entityView, EcsPackedEntityWithWorld packedEntity)
            where T : struct;

        public bool TryGetEntityViewForPackedEntity<T, V>(
            EcsPackedEntityWithWorld? packed, out V view)
            where T : struct;

        public void RequestDetailsHover(EcsPackedEntityWithWorld? packed);
        public void DismissDetailsHover(EcsPackedEntityWithWorld? packed);

    }

}

