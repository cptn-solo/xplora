using Assets.Scripts.Services;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.UI.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitHPSourceSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UpdateTag<HpTag>> updatePool = default;
        private readonly EcsPoolInject<IntValueComp<HpTag>> hpPool = default;
        private readonly EcsPoolInject<IntValueComp<HealthTag>> healthPool = default;

        private readonly EcsFilterInject<
            Inc<PlayerComp, VisitedComp<HPSourceComp>>> visitFilter = default;
        private readonly EcsFilterInject<
            Inc<PlayerTeamTag, IntValueComp<HpTag>, IntValueComp<HealthTag>>,
            Exc<DeadTag>> teamMembersFilter = default;

        private readonly EcsCustomInject<AudioPlaybackService> audioService = default;

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