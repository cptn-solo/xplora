using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateUnitOverlaySystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool = default;
        private readonly EcsPoolInject<BuffComp<NoStaminaDrainBuffTag>> staminaBuffPool = default;
        /// <summary>
        /// BarInfo shouldn't be messed up with BarSInfo wich is parent empty
        /// entity view that now contains only bars child
        /// </summary>
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> overlayPool = default;

        private readonly EcsFilterInject<
            Inc<UpdateTag, ItemsContainerRef<BarInfo>, PowerComp>> updateFilter = default;


        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var powerComp = ref powerPool.Value.Get(entity);
                ref var overlayRef = ref overlayPool.Value.Get(entity);

                powerComp.StaminaBuff = staminaBuffPool.Value.Has(entity);

                overlayRef.Container.SetInfo(powerComp.BarsInfo);
            }

        }
    }
}