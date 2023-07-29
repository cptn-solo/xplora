using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class LibraryHandleHeroMoveSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<PositionComp> positionPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<UpdateTag<RelationsMatrixComp>> updateTagPool = default;

        private readonly EcsFilterInject<
            Inc<UpdateTag<MovedTag>, PositionComp>> moveFilter = default;

        private readonly EcsCustomInject<HeroLibraryService> libraryService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var moveEntity in moveFilter.Value)
            {
                ref var position = ref positionPool.Value.Get(moveEntity);
                var playerTeamId = libraryService.Value.PlayerTeam.Id;

                if (position.Position.Team != position.PrevPosition.Team &&
                    (playerTeamId == position.Position.Team ||
                    playerTeamId == position.PrevPosition.Team))
                {
                    if (playerTeamTagPool.Value.Has(moveEntity))
                        playerTeamTagPool.Value.Del(moveEntity);

                    if (position.Position.Team == playerTeamId)
                        playerTeamTagPool.Value.Add(moveEntity);

                    if (libraryService.Value.PlayerTeamEntity.Unpack(out _, out var playerTeamEntity) &&
                        !updateTagPool.Value.Has(playerTeamEntity))
                        updateTagPool.Value.Add(playerTeamEntity);
                }
            }
        }
    }
}

