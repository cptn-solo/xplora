using System.Globalization;
using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public static class ComplexParserUtils
    {
        public static IntRange ParseIntRangeValue(this object rawValueObj, bool signed = false)
        {
            var arr = rawValueObj.ParseIntArray(signed);
            
            if (arr == null)
                throw new Exception($"ParseIntRangeValue [{rawValueObj}]");

            if (arr.Length == 0)
                throw new Exception($"ParseIntRangeValue no range in [{rawValueObj}]");

            if (arr.Length == 1)
                return new IntRange(arr[0], arr[0]);

            return new IntRange(arr[0], arr[1]);
        }

        public static void ParseIntRangeValue(this object rawValueObj, out int minVal, out int maxVal)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .Split('-');
                minVal = int.Parse(rawValues[0], NumberStyles.None);
                maxVal = int.Parse(rawValues[1], NumberStyles.None);
            }
            catch (Exception ex)
            {
                minVal = 0;
                maxVal = 0;
                Debug.LogError($"ParseAbsoluteRangeValue [{rawValue}] Exception: {ex.Message}");
            }
        }

        public static RelationBonusInfo ParseRelationBonus(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .Replace("%", "")
                    .ToLower()
                    .Split(";");
                
                if (rawValues.Length == 0)
                    return RelationBonusInfo.Empty;

                var ret = RelationBonusInfoFactory.CreateWithStringParams(rawValues);
                
                return ret;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTraitsArray [{rawValue}] Exception: {ex.Message}");
                return RelationBonusInfo.Empty;
            }
        }

        public static IEffectRule ParseRelationEffectRule(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .Replace("%", "")
                    .ToLower()
                    .Split(";");
                
                if (rawValues.Length == 0)
                    return null;

                var ret = RelationEffectRuleFactory.CreateWithStringParams(rawValues);
                                
                return ret;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTraitsArray [{rawValue}] Exception: {ex.Message}");
                return null;
            }
        }
    }
}