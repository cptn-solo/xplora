using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Assets.Scripts.UI;

namespace Assets.Scripts.ECS.Systems
{
    public class ShowDialogSystem<T> : IEcsRunSystem
        where T : struct
    {
        private readonly EcsPoolInject<ModalDialogTag> modalPool = default;
        private readonly EcsPoolInject<T> infoPool = default;
        private readonly EcsPoolInject<EntityViewRef<T>> modalViewPool = default;

        private readonly EcsFilterInject<
            Inc<ModalDialogTag>> modalFilter = default;
        
        private readonly EcsFilterInject<
            Inc<EntityViewRef<T>>,
            Exc<ModalDialogTag>> eventModalFilter = default;

        private readonly EcsFilterInject<
            Inc<T>,
            Exc<ModalDialogTag>> eventInfoFilter = default;

        public void Run(IEcsSystems systems)
        {
            if (modalFilter.Value.GetEntitiesCount() > 0)
                return; // only 1 modal allowed at a time

            foreach (var entity in eventInfoFilter.Value)
            { 
                foreach (var modalEntity in eventModalFilter.Value)
                {
                    ref var info = ref infoPool.Value.Get(entity);

                    ref var modalViewRef = ref modalViewPool.Value.Get(modalEntity);
                    var view = (IEventDialog<T>)modalViewRef.EntityView;
                    view.SetEventInfo(info);

                    modalPool.Value.Add(modalEntity);
                }
                modalPool.Value.Add(entity);
            }                         
        }
    }
}