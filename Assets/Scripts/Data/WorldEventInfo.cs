﻿using System.Collections.Generic;
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
        public BonusOptionConfig[] BonusOptions { get; set; }

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

        public static string EventTextGenerator(int level, string heroName, TerrainAttribute attribute)
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
        
    }
}