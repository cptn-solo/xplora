using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamMemberRelationsInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;
        
        private readonly EcsPoolInject<KindGroupSpiritTag> spiritGroupPool = default;
        private readonly EcsPoolInject<KindGroupBodyTag> bodyGroupPool = default;
        private readonly EcsPoolInject<KindGroupNeutralTag> neutralGroupPool = default;
        
        private readonly EcsPoolInject<IntValueComp<HeroKindRSDTag>> rsdPool = default;
        

        private readonly EcsFilterInject<Inc<HeroConfigRefComp, PlayerTeamTag>> filter = default;

        private readonly EcsCustomInject<HeroLibraryService> heroLibraryService = default;

        public void Init(IEcsSystems systems)
        {
            ref var relationsConfig = ref heroLibraryService.Value.HeroRelationsConfig;
            
            foreach (var entity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                InitHeroKindGroup(entity, heroConfig, relationsConfig);
                DeriveMainKindGroup(entity, relationsConfig);
            }
        }        

        private void DeriveMainKindGroup(int entity, HeroRelationsConfig relationsConfig)
        {
            var rsd = ecsWorld.Value.ReadIntValue<HeroKindRSDTag>(entity);

            if (relationsConfig.SpiritGroupScoreRange.Contains(rsd))
            {
                spiritGroupPool.Value.Add(entity);
            }
            else if (relationsConfig.BodyGroupScoreRange.Contains(rsd))
            {
                bodyGroupPool.Value.Add(entity);
            }
            else if (relationsConfig.NeutralGroupScoreRange.Contains(rsd))
            {
                neutralGroupPool.Value.Add(entity);
            }
            else throw new Exception($"Can't get Hero Kind Group for RSD {rsd}, no suitable range");

        }

        private void InitHeroKindGroup(int entity, Hero heroConfig, HeroRelationsConfig relationsConfig)
        {
            rsdPool.Value.Add(entity);

            foreach (var kind in relationsConfig.SpiritKindGroup)
            {
                var kindInfo = heroConfig.Kinds[kind];
                switch (kind)
                {
                    case HeroKind.Asc:
                        AddHeroKindComponent<HeroKindAscTag>(entity, kindInfo.Level);                        
                        break;
                    case HeroKind.Spi:
                        AddHeroKindComponent<HeroKindSpiTag>(entity, kindInfo.Level);
                        break;
                    case HeroKind.Int:
                        AddHeroKindComponent<HeroKindIntTag>(entity, kindInfo.Level);
                        break;
                    case HeroKind.Cha:
                        AddHeroKindComponent<HeroKindChaTag>(entity, kindInfo.Level);
                        break;                    
                    default:
                        break;
                }                
            }      

            foreach (var kind in relationsConfig.BodyKindGroup)
            {
                var kindInfo = heroConfig.Kinds[kind];
                switch (kind)
                {                    
                    case HeroKind.Tem:
                        AddHeroKindComponent<HeroKindTemTag>(entity, -kindInfo.Level);
                        break;
                    case HeroKind.Con:
                        AddHeroKindComponent<HeroKindConTag>(entity, -kindInfo.Level);
                        break;
                    case HeroKind.Str:
                        AddHeroKindComponent<HeroKindStrTag>(entity, -kindInfo.Level);
                        break;
                    case HeroKind.Dex:
                        AddHeroKindComponent<HeroKindDexTag>(entity, -kindInfo.Level);
                        break;
                    default:
                        break;
                }                
            }   
        }

        private void AddHeroKindComponent<T>(int playerTeamMemberEntity, int level)
            where T : struct
        {
            var pool = ecsWorld.Value.GetPool<IntValueComp<T>>();
            ref var val = ref pool.Add(playerTeamMemberEntity);
            val.Boundary = new IntRange(0, int.MaxValue);
            val.Value = Mathf.Abs(level);

            ecsWorld.Value.IncrementIntValue<HeroKindRSDTag>(level, playerTeamMemberEntity);
        }

    }
}