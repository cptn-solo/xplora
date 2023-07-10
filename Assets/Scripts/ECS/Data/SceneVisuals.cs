using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System.Collections.Generic;

public struct DamageEffectVisualsInfo
{
    public EcsPackedEntityWithWorld SubjectEntity;
    public int EffectsDamage;
    public DamageEffect[] Effects;
    public bool Lethal;
}

public struct SceneVisualsQueueComp
{
    public List<EcsPackedEntity> QueuedVisuals;
}