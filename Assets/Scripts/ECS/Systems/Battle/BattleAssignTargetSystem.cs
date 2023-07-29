using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleAssignTargetSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleTurnInfo> turnPool = default;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool = default;
        private readonly EcsPoolInject<TargetRef> targetRefPool = default;
        private readonly EcsPoolInject<RangedTag> rangedTagPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;
        private readonly EcsPoolInject<UsedFocusEntityTag> usedFocusPool = default;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>, Exc<SkippedTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignTarget(entity);
        }

        private void AssignTarget(int turnEntity)
        {
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerInstanceEntity))
                throw new Exception("No Attacker");

            var isPlayerTeam = playerTeamTagPool.Value.Has(attackerInstanceEntity);
            var ranged = rangedTagPool.Value.Has(attackerInstanceEntity);

            if (isPlayerTeam && world.GetAlgoTargetFocus(attackerInstanceEntity, out var tgtfocus, out var focusEntity) &&
                tgtfocus.HasValue && tgtfocus.Value.Unpack(out _, out int targetEntity))
            {
                Debug.Log("Target was overriden by relation AlgoTarget event");
                MarkForExpire(focusEntity);
            }
            else if (isPlayerTeam && world.GetAlgoRevengeFocus(attackerInstanceEntity, out var rvgFocus, out focusEntity) &&
                rvgFocus.HasValue && rvgFocus.Value.Unpack(out _, out targetEntity))
            {
                Debug.Log("Target was overriden by relation AlgoRevenge event");
                MarkForExpire(focusEntity);
            }
            else
            {
                if (ranged)
                {
                    var filter = isPlayerTeam ?
                        TeamTagFilter<EnemyTeamTag>(world) :
                        TeamTagFilter<PlayerTeamTag>(world);
                    var targets = filter.AllEntities();
                    targetEntity = targets.Length > 0 ?
                        targets[Random.Range(0, targets.Length)] :
                        -1;
                }
                else
                {
                    var filterFront = isPlayerTeam ?
                        TeamTagFilter<EnemyTeamTag, FrontlineTag>(world) :
                        TeamTagFilter<PlayerTeamTag, FrontlineTag>(world);

                    var filterBack = isPlayerTeam ?
                        TeamTagFilter<EnemyTeamTag, BacklineTag>(world) :
                        TeamTagFilter<PlayerTeamTag, BacklineTag>(world);

                    var frontTargets = filterFront.AllEntities();
                    var backTargets = filterBack.AllEntities();

                    // TODO: consider range (not yet imported/parsed)

                    targetEntity = frontTargets.Length > 0 ?
                        frontTargets[Random.Range(0, frontTargets.Length)] :
                        backTargets.Length > 0 ?
                        backTargets[Random.Range(0, backTargets.Length)] :
                        -1;
                }
            }            

            if (targetEntity != -1)
            {
                ref var targetConfigRef = ref heroConfigRefPool.Value.Get(targetEntity);

                if (!targetConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var targetConfigEntity))
                    throw new Exception("No Hero Config");

                var heroConfigPool = libWorld.GetPool<Hero>();
                ref var targetConfig = ref heroConfigPool.Get(targetConfigEntity);
                turnInfo.TargetConfig = targetConfig;

                ref var targetRef = ref targetRefPool.Value.Add(turnEntity);
                targetRef.HeroInstancePackedEntity = world.PackEntityWithWorld(targetEntity);
                turnInfo.Target = targetRef.HeroInstancePackedEntity;
            }

            turnInfo.State = targetEntity == -1 ?
                TurnState.NoTargets : TurnState.TurnPrepared;
        }

        private void MarkForExpire(int focusEntity)
        {
            if (!usedFocusPool.Value.Has(focusEntity))
                usedFocusPool.Value.Add(focusEntity);
        }

        private EcsFilter TeamTagFilter<T, L>(EcsWorld world) where T : struct where L : struct
        {
            return world.Filter<T>()
                        .Inc<L>()
                        .Exc<DeadTag>().End();
        }
        private EcsFilter TeamTagFilter<T>(EcsWorld world) where T : struct
        {
            return world.Filter<T>()
                        .Exc<DeadTag>().End();
        }


    }
}
