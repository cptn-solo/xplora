using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI;

namespace Assets.Scripts.ECS.Systems
{

    public class ShowToastSystem<T> : BaseEcsSystem
        where T : struct
    {
        private readonly EcsPoolInject<ToastTag<T>> toastPool = default;
        private readonly EcsPoolInject<T> infoPool = default;
        private readonly EcsPoolInject<EntityViewRef<T>> toastViewPool = default;

        private readonly EcsFilterInject<
            Inc<EntityViewRef<T>>> eventToastFilter = default;

        private readonly EcsFilterInject<
            Inc<T>> eventInfoFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in eventInfoFilter.Value)
            {
                if (!toastPool.Value.Has(entity))
                    toastPool.Value.Add(entity);

                foreach (var modalEntity in eventToastFilter.Value)
                {
                    ref var info = ref infoPool.Value.Get(entity);

                    if (!toastPool.Value.Has(modalEntity))
                        toastPool.Value.Add(modalEntity);

                    ref var modalViewRef = ref toastViewPool.Value.Get(modalEntity);
                    var view = (IEventDialog<T>)modalViewRef.EntityView;
                    view.SetEventInfo(info);                    
                }
            }
        }
    }
}