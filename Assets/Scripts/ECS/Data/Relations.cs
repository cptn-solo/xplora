using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;

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
    /// For casting probability needs, we don't need any info about current effects, only their count
    /// </summary>
    public struct RelationEffectsCountTag { }    
    
    /// <summary>
    /// Marks a turn to process additional round queue manipulation to insert a hero after current turn
    /// </summary>
    public struct PrepareRevengeComp { 
        //attack target
        public EcsPackedEntityWithWorld Focus { get; set; }
        // who will attack
        public EcsPackedEntityWithWorld RevengeFor { get; set; }
        // effect source
        public EcsPackedEntityWithWorld RevengeBy { get; set; }
    }

    /// <summary>
    /// Marks a turn to affect targeting (aiming) system so all teammates will attack an effect's focus
    /// </summary>
    public struct PrepareTargetComp
    {
        // attack target
        public EcsPackedEntityWithWorld Focus { get; set; }
        // who will attack
        public EcsPackedEntityWithWorld TargetFor { get; set; }
        // effect source
        public EcsPackedEntityWithWorld TargetBy { get; set; }
    }

    /// <summary>
    /// Contains references to each score entity by hero instance entity,
    /// so when event is spawned or we just need to check a score with some other guy
    /// just pick a score entity for this guys entity
    /// </summary>
    public struct RelationPartiesRef_
    {
        public Dictionary<EcsPackedEntityWithWorld, EcsPackedEntityWithWorld> Parties { get; set; }
    }

    public struct RelationsMatrixComp
    {
        public Dictionary<RelationsMatrixKey, EcsPackedEntityWithWorld> Matrix { get; set; }
    }

    public struct EffectInstanceInfo
    {
        public IEffectRule Rule { get; set; }
        public int StartRound { get; set; }
        public int EndRound { get; set; }
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
            retval += $"R: {StartRound} - {EndRound};";
            retval += $"Left: {UsageLeft};";

            return retval;
        }

        public RelationEffectInfo EffectInfo { get; set; }
    }

    public struct RelEffectProbeComp
    {
        // effect source hero (spawner)
        public EcsPackedEntityWithWorld SourceOrigPacked { get; internal set; }
        public EcsPackedEntityWithWorld SourceConfigRefPacked { get; internal set; }

        // effect target hero (receiver of the effect)
        public EcsPackedEntityWithWorld TargetOrigPacked { get; internal set; }
        public EcsPackedEntityWithWorld TargetConfigRefPacked { get; internal set; }

        // target's state due to which the effect was spawned
        public RelationSubjectState SubjectState { get; internal set; }

        /// <summary>
        /// Score, current effects count (in the origin world), current effects info (in the battle wolrd
        /// </summary>
        public EcsPackedEntityWithWorld P2PEntityPacked { get; internal set; }
        
        public EcsPackedEntity TurnEntity { get; internal set; }
        
    }

    public struct RelationEffectsComp
    {
        private Dictionary<RelationEffectKey, EffectInstanceInfo> currentEffects;
        private string description;
        
        private const int MaxEffectsForHero = 99; // decision made to let them spawn as they are


        public Dictionary<RelationEffectKey, EffectInstanceInfo> CurrentEffects
        {
            get => currentEffects;
            set
            {
                currentEffects = value;
                description = ToString();
            }
        }

        public void Clear()
        {
            currentEffects.Clear();
            description = ToString();
        }

        public void SetEffect(RelationEffectKey type, EffectInstanceInfo effect)
        {
            if (currentEffects == null) return;
            
            if (currentEffects.Count > MaxEffectsForHero) return;

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
