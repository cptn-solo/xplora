using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    public static class EcsBattleExtensions
    {
        /// <summary>
        /// All systems to schedule (enqueue) visualier systems
        /// </summary>
        /// <param name="systems"></param>
        /// <returns></returns>
        public static IEcsSystems AddSchedulers(this IEcsSystems systems)
        {
            systems
                .Add(new BattleScheduleSceneVisualsQueuedEffectsSystem()) // for queued effects only
                .Add(new BattleScheduleSceneVisualsSystem()) // prepare visualization queue for attack
                .Add(new BattleScheduleRelationEffectVisualSystem())
                .Add(new BattleScheduleRelationEffectFocusVisualSystem())
                .Add(new BattleScheduleRelationEffectFocusResetVisualSystem()) // schedules remove of used focus icons
                .Add(new BattleScheduleRelationEffectResetVisualSystem()) // set rel. effects container to the current state 
                ;
            return systems;
        }

        /// <summary>
        /// All systems to handle different visualization infos
        /// </summary>
        /// <param name="systems"></param>
        /// <returns></returns>
        public static IEcsSystems AddVisualizers(this IEcsSystems systems)
        {
            systems
                .Add(new BattleSceneRelationEffectResetVisualSystem()) // subclass for effects container panel
                .Add(new BattleSceneRelationEffectCastVisualSystem()) // subclass for effects container panel
                .Add(new BattleSceneRelationEffectFocusCastVisualSystem()) // subclass for effects container panel
                .Add(new BattleSceneVisualSystem<DamageEffectVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<ArmorPiercedVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<TakingDamageVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackDodgeVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackMoveVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackerAttackVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<AttackerMoveBackVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<HitVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<DeathVisualsInfo, Hero>())
                .Add(new BattleSceneVisualSystem<DeathVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleSceneVisualSystem<EffectsBarVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleSceneVisualSystem<HealthBarVisualsInfo, BarsAndEffectsInfo>())
                .Add(new BattleSceneRelationEffectFocusResetVisualSystem()); // removes focus icon from target before new one can be popped in
            return systems;
        }
    }
}
