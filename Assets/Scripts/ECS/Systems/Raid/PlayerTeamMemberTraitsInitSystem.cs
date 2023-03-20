using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class PlayerTeamMemberTraitsInitSystem : IEcsInitSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool = default;

        private readonly EcsFilterInject<Inc<HeroConfigRefComp, PlayerTeamTag>> filter = default;

        public void Init(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(entity);

                if (!heroConfigRef.Packed.Unpack(out var libWorld, out var libEntity))
                    throw new Exception("No Hero Config");

                ref var heroConfig = ref libWorld.GetPool<Hero>().Get(libEntity);

                AddTraits(entity, heroConfig);
            }
        }
        private void AddTraits(int entity, Hero heroConfig)
        {
            foreach (var trait in heroConfig.Traits)
            {
                if (trait.Value.Level <= 0)
                    continue;

                switch (trait.Value.Trait)
                {
                    case HeroTrait.Hidden:
                        AddTraitComponent<TraitHiddenTag>(entity, trait);
                        break;
                    case HeroTrait.Purist:
                        AddTraitComponent<TraitPuristTag>(entity, trait);
                        break;
                    case HeroTrait.Shrumer:
                        AddTraitComponent<TraitShrumerTag>(entity, trait);
                        break;
                    case HeroTrait.Scout:
                        AddTraitComponent<TraitScoutTag>(entity, trait);
                        break;
                    case HeroTrait.Tidy:
                        AddTraitComponent<TraitTidyTag>(entity, trait);
                        break;
                    case HeroTrait.Soft:
                        AddTraitComponent<TraitSoftTag>(entity, trait);
                        break;
                    default:
                        break;
                }
            }
        }

        private void AddTraitComponent<T>(int playerTeamMemberEntity, KeyValuePair<HeroTrait, HeroTraitInfo> trait)
            where T : struct
        {
            var pool = ecsWorld.Value.GetPool<IntValueComp<T>>();
            ref var val = ref pool.Add(playerTeamMemberEntity);
            val.Value = trait.Value.Level;
        }

    }
}