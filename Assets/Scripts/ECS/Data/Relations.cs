using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;
using System.Collections.Generic;
using UnityEngine;

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
}
