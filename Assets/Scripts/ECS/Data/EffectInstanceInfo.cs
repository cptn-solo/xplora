using Assets.Scripts.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    public struct EffectInstanceInfo
    {
        public IEffectRule Rule { get; set; }
        public int StartRound { get; set; }
        public int UsageLeft { get; set; } // initially = End - Start

        public readonly string Description => ToString();

        /// <summary>
        /// Entity of a hero from the other relation side
        /// packed in the ecs world of relations origin (Raid)
        /// </summary>
        public EcsPackedEntityWithWorld EffectSource { get; set; }

        /// <summary>
        /// Score and effects count in the world of the relation origin
        /// </summary>
        public EcsPackedEntityWithWorld EffectP2PEntity { get; set; }

        public override readonly string ToString()
        {
            var retval = "";

            retval += $"{Rule.EffectType};";
            retval += $"{Rule}";
            retval += $"Left: {UsageLeft};";

            return retval;
        }

        public RelationEffectInfo EffectInfo { get; set; }
    }
}
