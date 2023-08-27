using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.ECS.Systems
{
    public class VisitTerrainAttributeSystem : BaseEcsSystem
    {
        //private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<VisitedComp<TerrainAttributeComp>> pool = default;
        private readonly EcsPoolInject<
            ActiveTraitHeroComp<TerrainAttributeComp>> traitHeroPool = default;
        private readonly EcsPoolInject<WorldEventInfo> eventInfoPool = default;

        private readonly EcsFilterInject<
            Inc<VisitedComp<TerrainAttributeComp>,
                ActiveTraitHeroComp<TerrainAttributeComp>>,
            Exc<WorldEventInfo>> visitFilter = default;

        private readonly EcsCustomInject<WorldService> worldService = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in visitFilter.Value)
            {
                ref var attributes = ref pool.Value.Get(entity);
                ref var traitHero = ref traitHeroPool.Value.Get(entity);

                //NB: for now it is 1:1 mapping, can be extended later
                var eventConfig = worldService.Value
                    .TerrainEventsLibrary.TerrainEvents[attributes.Info.TerrainAttribute];

                CreateEvent(entity, eventConfig,
                    traitHero.Hero, traitHero.PackedHeroInstanceEntity, traitHero.MaxLevel);
                
            }
        }

        private void CreateEvent(int entity, 
            TerrainEventConfig config, 
            Hero hero, EcsPackedEntityWithWorld heroEntity,
            int level)
        {
            var buttonBonuses = ListPool<BonusOptionConfig>.Get();

            foreach(var bonusOption in config.BonusOptions)
            {
                BonusOptionConfig? bonus = null;

                foreach (var bonusConfig in bonusOption.BonusConfigs)
                    if (bonusConfig.SpawnRate.RatedRandomBool())
                    {
                        bonus = bonusConfig;
                        break;
                    }

                if (bonus == null)
                {
                    if (bonusOption.BonusConfigs.Sum(x => x.SpawnRate) < 100)
                    {
                        continue;
                    }
                    else
                    {
                        // fallback bonus if spawnrate fails
                        bonus = bonusOption.BonusConfigs[
                            Random.Range(0, bonusOption.BonusConfigs.Length)];

                        buttonBonuses.Add(bonus.Value);
                    }
                }
                else
                {
                    buttonBonuses.Add(bonus.Value);
                }
            }

            var resultBonusOptions = buttonBonuses.ToArray();

            ListPool<BonusOptionConfig>.Add(buttonBonuses);

            ref var info = ref eventInfoPool.Value.Add(entity);
            info.EventHero = hero;
            info.HeroEntity = heroEntity;
            info.EventTitle = $"{config.Name} ({level})";
            info.EventText = WorldEventInfo.EventTextGenerator(level, hero.Name, config.Attribute);
            info.IconName = hero.IconName;
            info.BonusOptions = resultBonusOptions;

            var buffer = ListPool<string>.Get();

            for(int i = 0; i < resultBonusOptions.Length; i++)
            {
                if (i < 2)
                    buffer.Add(resultBonusOptions[i].ToString());
                else
                    buffer[1] += $", \n{resultBonusOptions[i]}";
            }
            info.ActionTitles = buffer.ToArray();

            ListPool<string>.Add(buffer);
        }

    }
}