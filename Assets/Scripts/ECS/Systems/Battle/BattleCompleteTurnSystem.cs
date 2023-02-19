using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleCompleteTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<MakeTurnTag> makeTurnTagPool;
        private readonly EcsFilterInject<Inc<BattleTurnInfo, MakeTurnTag>> makeTurnFilter;

        public void Run(IEcsSystems systems)
        {
            foreach(var entity in makeTurnFilter.Value)
            {
                CompleteTurn(entity);
                makeTurnTagPool.Value.Del(entity);
            }
        }
        internal void CompleteTurn(int turnEntity)
        {
            //turnCoroutineRunning = true;

            var turnInfo = CurrentTurn;

            //capture value just in case
            var skipTurn = turnInfo.State == TurnState.TurnSkipped;

            ApplyQueuedEffects(turnInfo, out var attacker, out var effectsInfo);

            //yield return null;

            if (effectsInfo != null)
            {
                UpdateBattleHero(attacker); // Sync health
                OnTurnEvent?.Invoke((BattleTurnInfo)effectsInfo, CurrentRound, CurrentBattle);
            }

            if (attacker.HealthCurrent <= 0 || skipTurn)
            {
                turnInfo = turnInfo.UpdateAttacker(attacker);
                SetTurnInfo(turnInfo, TurnState.TurnCompleted);

                //yield return null;
            }
            else
            {
                ProcessAttack(turnInfo, attacker, out var target, out var resultInfo);

                UpdateBattleHero(target); // Sync health

                SetTurnInfo(resultInfo, TurnState.TurnInProgress);
                OnTurnEvent?.Invoke(CurrentTurn, CurrentRound, CurrentBattle);

                //yield return null;

                SetTurnState(TurnState.TurnCompleted);
            }

            //yield return null;

            //turnCoroutineRunning = false;
        }

    }
}
