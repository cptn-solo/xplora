using System.Collections;
using System.Collections.Generic;
using Leopotam.EcsLite;
using UnityEngine;

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
    }
}
