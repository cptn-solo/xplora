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
            var terrainTypes = new TerrainType[3] {
                    TerrainType.Grass,
                    TerrainType.LightGrass,
                    TerrainType.NoGo,
                };

            foreach(var entity in filter.Value)
            {
                ref var terrainType = ref terrainTypePool.Value.Add(entity);
                terrainType.TerrainType = terrainTypes[Random.Range(0, terrainTypes.Length)];

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