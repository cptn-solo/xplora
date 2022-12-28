using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public static class FieldParserUtils
    {

        public static DamageType ParseDamageType(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;
            
            try
            {
                var rawValues = rawValue.Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "силовой" => DamageType.Force,
                    "режущий" => DamageType.Cut,
                    "колющий" => DamageType.Pierce,
                    "огненный" => DamageType.Burn,
                    "замораживающий" => DamageType.Frost,
                    _ => DamageType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseDamageType [{rawValue}] Exception: {ex.Message}");
                return DamageType.NA;
            }
        }
        public static AttackType ParseAttackType(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "дистанционная" => AttackType.Ranged,
                    "ближняя" => AttackType.Melee,
                    "магическая" => AttackType.Magic,
                    _ => AttackType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseAttackType [{rawValue}] Exception: {ex.Message}");
                return AttackType.NA;
            }
        }

        public static DamageEffect ParseDamageEffect(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "оглушение" => DamageEffect.Stunned,
                    "кровотечение" => DamageEffect.Bleeding,
                    "пробитиеброни" => DamageEffect.Pierced,
                    "горение" => DamageEffect.Burning,
                    "заморозка" => DamageEffect.Frozing,
                    _ => DamageEffect.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseDamageEffect [{rawValue}] Exception: {ex.Message}");
                return DamageEffect.NA;
            }
        }
        
        public static void ParseAbsoluteRangeValue(this object rawValueObj, out int minVal, out int maxVal)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace(" ", "").Split('-');
                minVal = int.Parse(rawValues[0], System.Globalization.NumberStyles.None);
                maxVal = int.Parse(rawValues[1], System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                minVal = 0;
                maxVal = 0;
                Debug.LogError($"ParseAbsoluteRangeValue [{rawValue}] Exception: {ex.Message}");
            }
        }

        public static int ParseRateValue(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace("%", "").Replace(" ", "");
                return int.Parse(rawValues, System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseRateValue [{rawValue}] Exception: {ex.Message}");
                return 0;
            }
        }
        public static string ParseSoundFileValue(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.ToLower().Replace(".mp3", "").Replace(" ", "");
                return rawValues.Trim();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseSoundFileValue [{rawValue}] Exception: {ex.Message}");
                return "nosound";
            }
        }
        public static int ParseAbsoluteValue(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace("%", "").Replace(" ", "");
                return int.Parse(rawValues, System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseAbsoluteValue [{rawValue}] Exception: {ex.Message}");
                return 0;
            }
        }

        public static bool ParseBoolValue(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue.Replace("%", "").Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "да" => true,
                    "yes" => true,
                    "true" => true,
                    "on" => true,
                    "истина" => true,
                    "1" => true,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseBoolValue [{rawValue}] Exception: {ex.Message}");
                return false;
            }
        }
    }
}