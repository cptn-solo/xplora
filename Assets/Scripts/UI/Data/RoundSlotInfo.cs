using System.Collections.Generic;

namespace Assets.Scripts.UI.Data
{
    public struct RoundSlotInfo
    {
        public int HeroId { get; private set; }
        public string HeroName { get; private set; }
        public int TeamId { get; private set; }
        public List<DamageEffect> Effects { get; private set; }
        public bool Skipped { get; private set; }

        public static RoundSlotInfo Create(Hero hero)
        {
            RoundSlotInfo info = default;
            info.HeroId = hero.Id;
            info.TeamId = hero.TeamId;
            info.HeroName = hero.Name;
            info.Effects = new();
            return info;
        }

        public RoundSlotInfo AddEffect(DamageEffectInfo effect)
        {
            Effects.Add(effect.Effect);
            Skipped = effect.TurnSkipped;
            return this;
        }
    }
}