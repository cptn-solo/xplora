using Assets.Scripts.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Data
{
    public struct ActiveEffectComp
    {
        public DamageEffect Effect;
        public int EffectDamage;
        public EcsPackedEntityWithWorld Subject;
        public int TurnAttached; // just for information, may be logged later
        public int TurnsActive; // decrement each use, then delete
        public readonly bool SkipTurn
        {
            get => Effect switch
            {
                DamageEffect.Frozing => true,
                DamageEffect.Stunned => true,
                _ => false
            };
        }
    }
}


