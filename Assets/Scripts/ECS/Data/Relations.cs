using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Assets.Scripts.ECS.Data
{
    /// <summary>
    /// Base tag to assign to a newly created P2P relation entity
    /// </summary>
    public struct P2PRelationTag { } 

    /// <summary>
    /// Marks a value of current relation score between some parties
    /// </summary>
    public struct RelationScoreTag { } 

    /// <summary>
    /// Contains references to each score entity by hero instance entity,
    /// so when event is spawned or we just need to check a score with some other guy
    /// just pick a score entity for this guys entity
    /// </summary>
    public struct RelationPartiesRef
    {
        public Dictionary<EcsPackedEntity, EcsPackedEntity> Parties { get; set; }
    }

    public struct EffectInstanceInfo
    {
        public IEffectRule Rule { get; set; }
        public int StartRound { get; set; }
        public int EndRound { get; set; }
        public int UsageLeft { get; set; } // initially = End - Start

        public string Description => ToString();

        public override string ToString()
        {
            var retval = "";

            retval += $"{Rule.EffectType};";
            retval += $"{Rule}";
            retval += $"R: {StartRound} - {EndRound};";
            retval += $"Left: {UsageLeft};";

            return retval;
        }
    }

    public struct RelationEffectsComp
    {
        private Dictionary<RelationsEffectType, EffectInstanceInfo> currentEffects;
        private string description;

        public Dictionary<RelationsEffectType, EffectInstanceInfo> CurrentEffects
        {
            get => currentEffects;
            set
            {
                currentEffects = value;
                description = ToString();
            }
        }

        public void SetEffect(RelationsEffectType type, EffectInstanceInfo effect)
        {
            if (currentEffects == null) return;

            if (currentEffects.TryGetValue(type, out _))
            {
                currentEffects[type] = effect;
            }
            else
            {
                currentEffects.Add(type, effect);
            }
            description = ToString();
        }
        
        public string Description => description;

        public override string ToString()
        {
            var cnt = CurrentEffects != null ? CurrentEffects.Count : 0;
            var rules = cnt == 0 ? "" : GetRulesString();
            var retval = $"Cnt: {cnt}, Rules: {rules}";

            return retval;
        }

        private string GetRulesString()
        {
            var retval = $"";
            foreach (var rule in CurrentEffects)
                retval += rule.ToString() + ";";
         
            return retval;
        }
    }
}
