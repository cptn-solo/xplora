using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class TerrainTypesInitSystem : IEcsInitSystem
    {
        private readonly EcsPoolInject<NonPassableTag> noGoTagPool;
        private readonly EcsPoolInject<TerrainTypeComp> terrainTypePool;

        private readonly EcsFilterInject<Inc<FieldCellComp>> filter;

        public void Init(IEcsSystems systems)
        {            
            foreach(var entity in filter.Value)
            {
                ref var terrainType = ref terrainTypePool.Value.Add(entity);
                var idx = Random.Range(1, 101);
                terrainType.TerrainType = idx.RandomRangedTerrainType();

                switch (terrainType.TerrainType)
                {
                    case TerrainType.NoGo:
                        noGoTagPool.Value.Add(entity);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}