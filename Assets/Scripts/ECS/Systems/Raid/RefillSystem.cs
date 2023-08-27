using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Refills power from source
    /// </summary>
    public class RefillSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool = default;
        private readonly EcsPoolInject<RefillComp> refillPool = default;
        private readonly EcsPoolInject<UpdateTag> updateTagPool = default;

        private readonly EcsFilterInject<Inc<RefillComp, PowerComp>> refillFilter = default;

        private readonly EcsCustomInject<AudioPlaybackService> audioService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach(var entity in refillFilter.Value)
            {
                ref var refillComp = ref refillPool.Value.Get(entity);
                ref var powerComp = ref powerPool.Value.Get(entity);

                //TODO: implement partial refill
                powerComp.CurrentValue = powerComp.InitialValue;

                audioService.Value.Play(CommonSoundEvent.StaminaSource.SoundForEvent());

                if (!updateTagPool.Value.Has(entity))
                    updateTagPool.Value.Add(entity);
            }
        }
    }
}