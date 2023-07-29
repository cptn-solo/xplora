using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;
using UnityEngine;

public interface ISceneVisualsInfo
{
    /// <summary>
    /// Entity to look views for and to apply visuals to
    /// </summary>
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct VisualsTransformTag { }

/// <summary>
/// Capture the effect to visualize cast at the beginning of the visualization stage
/// </summary>
public struct RelationEffectsPendingComp { 
    public EcsPackedEntityWithWorld EffectSource { get; set; }
    public EcsPackedEntityWithWorld EffectTarget { get; set; }
    public RelationEffectInfo EffectInfo { get; set; }  
}

public struct RelationEffectsFocusPendingComp
{
    public EcsPackedEntity TurnEntity; // turn to match effect to
    public EcsPackedEntityWithWorld EffectSource { get; set; }
    public EcsPackedEntityWithWorld EffectTarget { get; set; }
    public EcsPackedEntityWithWorld FocusEntity { get; set; }

    internal RelationsEffectType EffectType; // target or revenge
}

public struct RelationEffectsFocusResetInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    //entity to keep track of the focus icon (there are several focus possible for one targeted hero, so they are separate)
    internal EcsPackedEntityWithWorld FocusEntity;
}

public struct RelationEffectsFocusCastInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    // animation starting point
    public Transform SourceTransform;
    //actually Subject's transform of visuals anchor
    public Transform TargetTransform; 

    internal BundleIconInfo FocusInfo;
}

/// <summary>
/// Resets effects panel before turn will recast new (current) ones
/// </summary>
public struct RelEffectResetVisualsInfo : ISceneVisualsInfo
{
    // to whom the effect will be casted (effect source hero image move)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct RelEffectCastVisualsInfo : ISceneVisualsInfo
{
    // to whom the effect will be casted (effect source hero image move)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    // animation starting point
    public Transform SourceTransform;
    public RelationEffectInfo EffectInfo { get; set; }
}

public struct DamageEffectVisualsInfo : ISceneVisualsInfo
{
    // turn active hero (attacker)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public int EffectsDamage;
    public DamageEffect[] Effects;
}

public struct EffectsBarVisualsInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public Dictionary<DamageEffect, int> ActiveEffects;
    public DamageEffect[] InstantEffects;
}

public struct HealthBarVisualsInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public BarInfo[] BarsInfoBattle { get; internal set; }

    public int Damage; // for future use, to show diff maybe
    public int Health;
    public int HealthCurrent;
}

public struct ArmorPiercedVisualsInfo : ISceneVisualsInfo
{
    // affected hero (target)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public int Damage;
}

public struct TakingDamageVisualsInfo : ISceneVisualsInfo
{
    // turn target hero
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public int Damage;
    public bool Critical;
    public bool Lethal;
}

public struct HitVisualsInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct DeathVisualsInfo : ISceneVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct AttackDodgeVisualsInfo : ISceneVisualsInfo
{
    // turn target hero to animate dodge effects on
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct AttackMoveVisualsInfo : ISceneVisualsInfo
{
    // turn active hero (attacker)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public EcsPackedEntityWithWorld TargetEntity { get; set; }
    public Transform TargetTransform;

    // will take later from context for subject and target entities:
    /*
     if (info.AttackerConfig.Ranged) // ranged don't run, just shoot
            break;

        EnqueueTurnAnimation(() => {
            // move both cards
            var move = 1.0f;
            attackerRM.HeroAnimation.Run(move, attackerPos);                        
        }, 1.3f);     
     */
}

public struct AttackerAttackVisualsInfo : ISceneVisualsInfo
{
    // attacker (will do ranged/melee/mage action)
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
    public EcsPackedEntityWithWorld TargetEntity { get; set; }
    public Transform TargetTransform;
    public bool Ranged;

    //info.AttackerConfig.Ranged
    //targetRM.transform.position
    //info.AttackerConfig.SndAttack
}

public struct AttackerMoveBackVisualsInfo : ISceneVisualsInfo
{ 
    public EcsPackedEntityWithWorld SubjectEntity { get; set; }
}

public struct SceneVisualsQueueComp
{
    public List<EcsPackedEntity> QueuedVisuals;
}