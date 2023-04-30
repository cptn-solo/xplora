using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

namespace Assets.Scripts.ECS.Systems
{
    public class WorldProcessPowerSourceVisitorSystem : WorldProcessVisitorSystem<PowerSourceComp>
    {
        protected override bool ValidateVisitor(EcsWorld world, int visitorEntity)
        {
            var pool = world.GetPool<StaminaComp>();
            return pool.Has(visitorEntity);
        }
    }
}