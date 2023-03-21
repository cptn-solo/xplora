using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;

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

        public static bool IncrementIntValue<T>(this EcsWorld world, int factor, int entity)
            where T : struct =>
            world.IncrementValue<IntValueComp<T>, int>(factor, entity);

        public static bool IncrementValue<T, V>(this EcsWorld world, int factor, int entity)
            where T : struct, IValue<V>
        {
            var pool = world.GetPool<T>();

            if (!pool.Has(entity))
                pool.Add(entity);

            ref var comp = ref pool.Get(entity);
            comp.Add(factor);

            return true;
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
