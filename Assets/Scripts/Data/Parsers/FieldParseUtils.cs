using System;
using System.Globalization;
using UnityEngine;

namespace Assets.Scripts.Data
{

    public static class FieldParserUtils
    {
        public static float ParseFloatRateValue(this object rawValueObj, 
            float defaultValue = 0f,
            bool signed = false)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace("%", "")
                    .Replace(" ", "")
                    .Replace(",", ".");
                
                if (!signed)
                    rawValues = rawValues
                    .Replace("-", "");

                if (rawValues.Length == 0)
                    return defaultValue;

                return float.Parse(rawValues,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseFloatRateValue [{rawValue}] Exception: {ex.Message}");
                return 0f;
            }
        }
        
        public static int ParseIntValue(this object rawValueObj,
            int defaultValue = 0,
            bool signed = false)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace("%", "")
                    .Replace(" ", "");

                if (!signed)
                    rawValues = rawValues
                    .Replace("-", "");

                if (rawValues.Length == 0)
                    return defaultValue;

                return int.Parse(rawValues,
                    signed ?
                        NumberStyles.AllowLeadingSign :
                        NumberStyles.None);
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

        public static bool ParseBoolValue(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace("%", "")
                    .Replace(" ", "")
                    .ToLower();
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