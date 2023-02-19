using Leopotam.EcsLite;

namespace Assets.Scripts.Data
{
    public struct RoundSlotInfo
    {
        public EcsPackedEntityWithWorld HeroInstancePackedEntity { get; set; }

        public string HeroName { get; set; }
        public int TeamId { get; set; }
        public int Speed { get; set; }

    }
}