using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI;

namespace Assets.Scripts.ECS.Systems
{
    public class ProcessDialogActionSystem<T> : BaseEcsSystem
        where T : struct
    {
        protected readonly EcsPoolInject<ModalDialogAction<T>> actionPool = default;
        protected readonly EcsPoolInject<ModalDialogTag> modalPool = default;
        protected readonly EcsPoolInject<T> infoPool = default;
        protected readonly EcsPoolInject<EntityViewRef<T>> modalViewPool = default;

        protected readonly EcsFilterInject<
            Inc<EntityViewRef<T>, ModalDialogTag>> eventModalFilter = default;

        protected readonly EcsFilterInject<
            Inc<ModalDialogTag, ModalDialogAction<T>>> actionFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in actionFilter.Value)
            {
                ref var action = ref actionPool.Value.Get(entity);
                ref var info = ref infoPool.Value.Get(entity);
                
                foreach (var modalEntity in eventModalFilter.Value)
                {                
                    ref var modalViewRef = ref modalViewPool.Value.Get(modalEntity);
                    var view = (IEventDialog<T>)modalViewRef.EntityView;
                    view.Dismiss();

                    ProcessAction(info, action);

                    modalPool.Value.Del(modalEntity);
                }

                modalPool.Value.Del(entity);
                infoPool.Value.Del(entity);
                actionPool.Value.Del(entity);
            }    
        }

        protected virtual void ProcessAction(T info, ModalDialogAction<T> action) { }
    }
}