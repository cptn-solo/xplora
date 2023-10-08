using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS
{
    public static class EcsSceneVisualsExtensions
    {
        /// <summary>
        /// Creates an entity for the visual effects and appends this entity to the queued visuals of the turn
        /// </summary>
        /// <param name="world"></param>
        /// <param name="turnEntity"></param>
        /// <returns>Ref variable of the component added to set options of the visuals to</returns>
        public static ref T ScheduleSceneVisuals<T>(
            this EcsWorld world, 
            int turnEntity)
            where T : struct, ISceneVisualsInfo
        {
            var uiEvent = world.NewEntity();
            
            ref var eventInfo = ref world.GetPool<T>().Add(uiEvent);         
            ref var turnVisuals = ref world.GetPool<SceneVisualsQueueComp>().Get(turnEntity);
            turnVisuals.QueuedVisuals.Add(world.PackEntity(uiEvent));
            return ref eventInfo;
        }

        public static void ToggleEntityButtonByType<B>(
            this EcsWorld world, 
            bool toggle) 
            where B : struct
        {
            var filter = world.Filter<EntityButtonRef<B>>().End();
            var pool = world.GetPool<EntityButtonRef<B>>();
            foreach (var entity in filter)
            {
                ref var buttonRef = ref pool.Get(entity);
                buttonRef.EntityButton.Toggle(toggle);
            }
        }

    }
}
