using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.ECS.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;

namespace Assets.Scripts.ECS
{
    public static class EcsExtensions
    {
        public static int[] AllEntities(this EcsFilter filter)
        {
            var cnt = filter.GetEntitiesCount();
            var en = new int[cnt];
            var enIdx = -1;
            foreach (var e in filter)
                en[++enIdx] = e;

            return en;
        }
        public static IEcsSystems CleanupHere<T>(this IEcsSystems systems, string worldName = null) where T : struct
        {
#if DEBUG && !LEOECSLITE_NO_SANITIZE_CHECKS
            if (systems.GetWorld(worldName) == null) { throw new System.Exception($"Requested world \"{(string.IsNullOrEmpty(worldName) ? "[default]" : worldName)}\" not found."); }
#endif
            return systems.Add(new CleanupSystem<T>(systems.GetWorld(worldName)));
        }


        public static int SetIntValue<T>(this EcsWorld world, int factor, int entity)
            where T : struct =>
            world.SetValue<IntValueComp<T>, int>(factor, entity);

        public static V SetValue<T, V>(this EcsWorld world, V factor, int entity)
            where T : struct, IValue<V>
        {
            var pool = world.GetPool<T>();

            if (!pool.Has(entity))
                pool.Add(entity);

            ref var comp = ref pool.Get(entity);
            comp.Value = factor;

            return comp.Value;
        }


        public static int IncrementIntValue<T>(this EcsWorld world, int factor, int entity)
            where T : struct =>
            world.IncrementValue<IntValueComp<T>, int>(factor, entity);

        public static V IncrementValue<T, V>(this EcsWorld world, int factor, int entity)
            where T : struct, IValue<V>
        {
            var pool = world.GetPool<T>();

            if (!pool.Has(entity))
                pool.Add(entity);

            ref var comp = ref pool.Get(entity);
            comp.Add(factor);

            return comp.Value;
        }

        public static IntRange ReadIntRangeValue<T>(this EcsWorld world, int entity)
            where T : struct =>
            world.ReadValue<IntRangeValueComp<T>, IntRange>(entity);

        public static int ReadIntValue<T>(this EcsWorld world, int entity)
            where T : struct =>
            world.ReadValue<IntValueComp<T>, int>(entity);

        public static V ReadValue<T, V>(this EcsWorld world, int entity)
            where T : struct, IValue<V>
        {
            var pool = world.GetPool<T>();

            if (!pool.Has(entity))
                return default(V);

            ref var comp = ref pool.Get(entity);

            return comp.Value;
        }

    }
}
