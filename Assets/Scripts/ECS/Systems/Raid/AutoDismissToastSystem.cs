using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class AutoDismissToastSystem<T> : BaseEcsSystem
        where T : struct, IDismissTimer
    {
        private readonly EcsPoolInject<ToastTag<T>> toastPool = default;
        private readonly EcsPoolInject<T> infoPool = default;
        private readonly EcsPoolInject<EntityViewRef<T>> toastViewPool = default;
        
        private readonly EcsFilterInject<
            Inc<EntityViewRef<T>, ToastTag<T>>> eventToastFilter = default;
        private readonly EcsFilterInject<
            Inc<T, ToastTag<T>>> eventInfoFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in eventInfoFilter.Value)
            {
                ref var info = ref infoPool.Value.Get(entity);

                if (info.DismissTimer > Time.time)
                    continue;

                foreach (var modalEntity in eventToastFilter.Value)
                {
                    ref var modalViewRef = ref toastViewPool.Value.Get(modalEntity);
                    var view = (IEventDialog<T>)modalViewRef.EntityView;
                    view.Dismiss();

                    infoPool.Value.Del(entity);
                    toastPool.Value.Del(modalEntity);
                    toastPool.Value.Del(entity);
                }
            }
        }
    }
}