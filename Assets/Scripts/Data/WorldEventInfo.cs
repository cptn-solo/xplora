using System.Collections.Generic;
using System.Linq;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct WorldEventInfo
    {
        public Hero EventHero { get; internal set; }
        public EcsPackedEntityWithWorld HeroEntity { get; internal set; }
        public string IconName { get; internal set; } // with path
        public string EventTitle { get; internal set; }
        public string EventText { get; internal set; }

        public string[] ActionTitles { get; internal set; }

        /// <summary>
        /// Actual spawned bonuses for the player to select from in a dialog
        /// </summary>
        public BonusOptionConfig[] BonusOptions { get; private set; }

        /// <summary>
        /// Подлежащее Гекс
        /// </summary>
        private static readonly string[] TerrainTypeSubjects = new[]
        {
            "луг", "травы", "это место", "безлесье", "поле",
        };
        /// <summary>
        /// Сказуемое Гекс
        /// </summary>
        private static readonly string[] TerrainTypePredicates = new[]
        {
            "дарить", "давать", "предоставлять", "рожать", "взращивать",
        };
        /// <summary>
        /// Сказуемое Герой
        /// </summary>
        private static readonly string[] HeroPredicates = new[]
        {
            "принимать", "брать", "использовать", "благодарить", "обретать", "получать",
        };

        private static readonly Dictionary<TerrainAttribute, string[]> TerrainAttributeSubjects = new()
        {
            {
                TerrainAttribute.Bush,
                new[] { "укрытие", "прибежище", "маскировка", "сень", "тень" }
            },
            {
                TerrainAttribute.Frowers,
                new[] { "красота", "чудо", "радость", "аромат", "спасение" }
            },
            {
                TerrainAttribute.Mushrums,
                new[] { "мудрость", "знание", "правда", "истина" }
            },
            {
                TerrainAttribute.Trees,
                new[] { "древность", "крепость", "мощь", "видение", "обзор" }
            },
            {
                TerrainAttribute.River,
                new[] { "свежесть", "ясность", "чистота", "влага", "синева" }
            },
            {
                TerrainAttribute.Path,
                new[] { "нить", "стрела", "сила", "прямота" }
            },
        };

        private static readonly Dictionary<TerrainAttribute, string[]> TerrainAttributePredicates = new()
        {
            {
                TerrainAttribute.Bush,
                new[] { "укрывать", "скрывать", "прятать", "скрадывать", "беречь", "защищать" }
            },
            {
                TerrainAttribute.Frowers,
                new[] { "радовать", "помогать", "поддерживать", "вдохновлять", "исцелять" }
            },
            {
                TerrainAttribute.Mushrums,
                new[] { "усиливать", "укреплять", "повышать", "пробуждать", "давать силы" }
            },
            {
                TerrainAttribute.Trees,
                new[] { "дарует мощь", "показывает простор", "открывает глаза", "просветляет" }
            },
            {
                TerrainAttribute.River,
                new[] { "освежает", "оздоровляет", "очищает", "ускоряет", "успокаивает" }
            },
            {
                TerrainAttribute.Path,
                new[] { "ведет вперед", "сохраняет силы", "сберегает дух", "бодрит", "стелится под ногами" }
            },
        };

        private static string EventTextGenerator(int level, string heroName, TerrainAttribute attribute)
        {
            string retval = "";

            if (level > 0 && level <= 2)
            {
                var taSubj = TerrainAttributeSubjects[attribute];
                retval = $"{TerrainTypeSubjects[Random.Range(0, TerrainTypeSubjects.Length)]} " +
                    $"{TerrainTypePredicates[Random.Range(0, TerrainTypePredicates.Length)]} " +
                    $"{taSubj[Random.Range(0, taSubj.Length)]} " +
                    $"{attribute.Name().ToLower()} " +
                    $"{heroName}";
            }
            else if (level > 2 && level <= 4)
            {
                var taSubj = TerrainAttributeSubjects[attribute];
                var taPred = TerrainAttributePredicates[attribute];
                retval = $"{taSubj[Random.Range(0, taSubj.Length)]} " +
                    $"{attribute.Name().ToLower()} " +
                    $"{taPred[Random.Range(0, taPred.Length)]} " +
                    $"{heroName}";
            }
            else if (level > 4)
            {
                var taSubj = TerrainAttributeSubjects[attribute];
                retval = $"{heroName} " +
                    $"{HeroPredicates[Random.Range(0, HeroPredicates.Length)]} " +
                    $"{taSubj[Random.Range(0, taSubj.Length)]} " +
                    $"{attribute.Name().ToLower()}";
            }

            return retval;
        }

        internal static WorldEventInfo Create(TerrainEventConfig config,
            Hero hero, EcsPackedEntityWithWorld heroEntity, int level)
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

            var info = new WorldEventInfo()
            {
                EventHero = hero,
                HeroEntity = heroEntity,
                EventTitle = $"{config.Name} ({level})",
                EventText = EventTextGenerator(level, hero.Name, config.Attribute),
                IconName = hero.IconName,
                BonusOptions = resultBonusOptions,
            };

            var buffer = ListPool<string>.Get();

            for(int i = 0; i < resultBonusOptions.Length; i++)
            {
                if (i < 2)
                    buffer.Add(resultBonusOptions[i].ToString());
                else
                    buffer[1] += $", {resultBonusOptions[i].ToString()}";
            }
            info.ActionTitles = buffer.ToArray();

            ListPool<string>.Add(buffer);

            return info;

        }
    }
}