using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePrepareRoundSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<HeroInstanceRef> heroInstanceRefPool = default;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        private readonly EcsPoolInject<DraftTag> draftTagPool = default;
        private readonly EcsPoolInject<IntValueComp<SpeedTag>> speedPool = default;
        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<NameValueComp<IdleSpriteTag>> idleSpriteNamePool = default;

        private readonly EcsFilterInject<Inc<BattleRoundInfo, DraftTag>> roundInfoFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in roundInfoFilter.Value)
            {
                PrepareRound(entity);// TODO: can just copy queue from the previous round
                draftTagPool.Value.Del(entity);
            }
        }

        ///     Enqueue heroes 
        /// </summary>
        /// <param name="info">Draft round</param>
        /// <returns>Round with heroes</returns>
        private void PrepareRound(int roundEntity)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                throw new Exception("No battle");

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);
            ref var roundInfo = ref roundInfoPool.Value.Get(roundEntity);

            roundInfo.State = RoundState.PrepareRound;

            var buffer = ListPool<RoundSlotInfo>.Get();

            foreach (var heroInstance in battleService.Value.PlayerHeroes.Concat(
                battleService.Value.EnemyHeroes))
            {
                if (!heroInstance.HeroInstancePackedEntity.Unpack(
                    out _, out var heroInstanceEntity))
                    throw new Exception("No Hero instance");

                ref var heroInstanceRef = ref heroInstanceRefPool.Value.Get(heroInstanceEntity);

                RoundSlotInfo slotInfo = new ()
                {
                    HeroInstancePackedEntity = heroInstance.HeroInstancePackedEntity,
                    HeroName = namePool.Value.Get(heroInstanceEntity).Name,
                    Speed = speedPool.Value.Get(heroInstanceEntity).Value,
                    TeamId = playerTeamTagPool.Value.Has(heroInstanceEntity) ?
                        battleInfo.PlayerTeam.Id :
                        battleInfo.EnemyTeam.Id,
                    IconName = iconNamePool.Value.Get(heroInstanceEntity).Name,
                    IdleSpriteName = idleSpriteNamePool.Value.Get(heroInstanceEntity).Name,
                };
                buffer.Add(slotInfo);
            }

            if (buffer.Count == 0)
                throw new Exception("No heroes for round queue");

            var orderedHeroes = buffer.OrderByDescending(x => x.Speed);

            Dictionary<int, List<RoundSlotInfo>> speedSlots = new();
            foreach (var hero in orderedHeroes)
            {
                if (speedSlots.TryGetValue(hero.Speed, out var slots))
                    slots.Add(hero);
                else
                    speedSlots[hero.Speed] = new List<RoundSlotInfo>() { hero };
            }

            ListPool<RoundSlotInfo>.Add(buffer);

            var speedKeys = speedSlots.Keys.OrderByDescending(x => x);

            var queue = ListPool<RoundSlotInfo>.Get();

            foreach (var speed in speedKeys)
            {
                var slots = speedSlots[speed];
                while (slots.Count() > 0)
                {
                    var choosenIdx = slots.Count() == 1 ? 0 : Random.Range(0, slots.Count());
                    queue.Add(slots[choosenIdx]);
                    slots.RemoveAt(choosenIdx);
                }
            }

            roundInfo.QueuedHeroes = queue.ToArray();

            ListPool<RoundSlotInfo>.Add(queue);

            if (roundInfo.QueuedHeroes.Length == 0)
                throw new Exception("No heroes for prepared round queue");

            roundInfo.State = RoundState.RoundPrepared;

            battleService.Value.NotifyRoundEventListeners();

        }

    }
}
