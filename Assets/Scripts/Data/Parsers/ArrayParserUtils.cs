using System;
using UnityEngine;

namespace Assets.Scripts.Data
{

    public static class ArrayParserUtils
    {
        public static SpecOption[] ParseSpecOptionsArray(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower()
                    .Split(";");

                var buffer = ListPool<SpecOption>.Get();

                foreach (var literal in rawValues)
                    buffer.Add(literal.ParseSpecOption());

                var retval = buffer.ToArray();

                ListPool<SpecOption>.Add(buffer);

                return retval;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseTerrainAttribute [{rawValue}] Exception: {ex.Message}");
                return new SpecOption[0];
            }
        }
        public static HeroTrait[] ParseHeroTraitsArray(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower()
                    .Split(";");

                var buffer = ListPool<HeroTrait>.Get();

                foreach (var literal in rawValues)
                    buffer.Add(literal.ParseHeroTrait());

                var retval = buffer.ToArray();

                ListPool<HeroTrait>.Add(buffer);

                return retval;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTraitsArray [{rawValue}] Exception: {ex.Message}");
                return new HeroTrait[0];
            }
        }
        public static int[] ParseIntArray(this object rawValueObj, bool signed = false)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .Replace("%", "")
                    .ToLower()
                    .Split(";");

                var buffer = ListPool<int>.Get();

                foreach (var literal in rawValues)
                    buffer.Add(literal.ParseIntValue(0, signed));

                var retval = buffer.ToArray();

                ListPool<int>.Add(buffer);

                return retval;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTraitsArray [{rawValue}] Exception: {ex.Message}");
                return new int[0];
            }
        }
    }
}