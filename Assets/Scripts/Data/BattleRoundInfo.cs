using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Data
{
    public struct BattleRoundInfo
    {

        public RoundState State { get; set; }
        public int Round { get; set; }
        public RoundSlotInfo[] QueuedHeroes { get; set; }

        public override string ToString()
        {
            var full = $"Раунд #{Round}: " +
                $"{State}, " +
                $"очередь: {QueuedHeroes?.Length}";
            
            return State switch
            {
                RoundState.RoundPrepared => full,
                RoundState.RoundInProgress => full,
                _ => $"Раунд #{Round}: {State}"
            };
        }
    }
}