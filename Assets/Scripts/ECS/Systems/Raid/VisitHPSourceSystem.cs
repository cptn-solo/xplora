using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitHPSourceSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UpdateHPTag> updatePool;
        private readonly EcsPoolInject<HPComp> hpPool;
        private readonly EcsPoolInject<HealthComp> healthPool;

        private readonly EcsFilterInject<
            Inc<PlayerComp, VisitedComp<HPSourceComp>>> visitFilter;
        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, HPComp, HealthComp>,
            Exc<DeadTag>> teamMembersFilter;

        private readonly EcsCustomInject<AudioPlaybackService> audioService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                foreach (var teamMemberEntity in teamMembersFilter.Value)
                {
                    ref var healthComp = ref healthPool.Value.Get(teamMemberEntity);
                    ref var hpComp = ref hpPool.Value.Get(teamMemberEntity);

                    // just max out the hp value 
                    hpComp.Value = healthComp.Value;

                    if (!updatePool.Value.Has(teamMemberEntity))
                        updatePool.Value.Add(teamMemberEntity);
                }

                audioService.Value.Play(CommonSoundEvent.HPSource.SoundForEvent());
            }
        }
    }
}