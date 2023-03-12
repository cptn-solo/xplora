﻿using Leopotam.EcsLite;

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

        public bool TryGetEntityViewForPackedEntity<T, V>(
            EcsPackedEntityWithWorld? packed, out V view)
            where T : struct;
    }

}

