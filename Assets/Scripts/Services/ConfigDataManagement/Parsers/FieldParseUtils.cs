using Assets.Scripts.Data;
using Assets.Scripts.UI.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public static class FieldParserUtils
    {
        public static HeroDomain ParseHeroDomain(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "h" => HeroDomain.Hero,
                    "e" => HeroDomain.Enemy,
                    _ => HeroDomain.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroDomain [{rawValue}] Exception: {ex.Message}");
                return HeroDomain.NA;
            }

        }
        public static DamageType ParseDamageType(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;
            
            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
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
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
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
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
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

        public static TerrainPOI ParseTerrainPOI(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "источникстамины" => TerrainPOI.PowerSource,
                    "колодецздоровья" => TerrainPOI.HPSource,
                    "смотроваявышка" => TerrainPOI.WatchTower,
                    _ => TerrainPOI.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseTerrainPOI [{rawValue}] Exception: {ex.Message}");
                return TerrainPOI.NA;
            }
        }

        public static TerrainType ParseTerrainType(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "луг" => TerrainType.Grass,
                    "песок" => TerrainType.Sand,
                    _ => TerrainType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseTerrainType [{rawValue}] Exception: {ex.Message}");
                return TerrainType.NA;
            }
        }

        public static SpecOption ParseSpecOption(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "урон" => SpecOption.DamageRange,
                    "защита" => SpecOption.DefenceRate,
                    "точность" => SpecOption.AccuracyRate,
                    "уклонение" => SpecOption.DodgeRate,
                    "здоровье" => SpecOption.Health,
                    "скорость" => SpecOption.Speed,
                    "нетратитвыносливость" => SpecOption.UnlimitedStaminaTag,
                    "критическийудар" => SpecOption.NA,
                    "сопротивляемостькровотечению" => SpecOption.NA,
                    "сопротивляемостьяду" => SpecOption.NA,
                    "сопротивляемостьоглушению" => SpecOption.NA,
                    "сопротивляемостьгорению" => SpecOption.NA,
                    "сопротивляемостьхолоду" => SpecOption.NA,
                    "споротивляемостьослеплению" => SpecOption.NA,
                    _ => SpecOption.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseSpecOption [{rawValue}] Exception: {ex.Message}");
                return SpecOption.NA;
            }
        }

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

        public static TerrainAttribute ParseTerrainAttribute(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "кустарник" => TerrainAttribute.Bush,
                    "цветы" => TerrainAttribute.Frowers,
                    "грибы" => TerrainAttribute.Mushrums,
                    "деревья" => TerrainAttribute.Trees,
                    "река" => TerrainAttribute.River,
                    "тропа" => TerrainAttribute.Path,
                    _ => TerrainAttribute.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseTerrainAttribute [{rawValue}] Exception: {ex.Message}");
                return TerrainAttribute.NA;
            }

        }

        public static HeroTrait ParseHeroTrait(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();
                return rawValues switch
                {
                    "скрытный" => HeroTrait.Hidden,
                    "эстет" => HeroTrait.Purist,
                    "грибник" => HeroTrait.Shrumer,
                    "разведчик" => HeroTrait.Scout,
                    "чистюля" => HeroTrait.Tidy,
                    "изнеженный" => HeroTrait.Soft,
                    _ => HeroTrait.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTrait [{rawValue}] Exception: {ex.Message}");
                return HeroTrait.NA;
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

        public static HeroKind HeroKindByName(this string kindString) =>
            kindString.ToLower() switch
            {
                "asc" => HeroKind.Asc,
                "spi" => HeroKind.Spi,
                "int" => HeroKind.Int,
                "cha" => HeroKind.Cha,
                "tem" => HeroKind.Tem,
                "con" => HeroKind.Con,
                "str" => HeroKind.Str,
                "dex" => HeroKind.Dex,
                _ => HeroKind.NA
            };

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

                var buffer = ListPool<HeroKind>.Get();
                var ret = new RelationBonusInfo
                {
                    Bonus = rawValues[0].ParseIntValue(0, true)
                };

                for (var i = 1; i < rawValues.Length; i++)
                    buffer.Add(rawValues[i].HeroKindByName());                    

                var retval = buffer.ToArray();

                ListPool<HeroKind>.Add(buffer);

                ret.TargetKinds = retval;
                
                return ret;
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseHeroTraitsArray [{rawValue}] Exception: {ex.Message}");
                return RelationBonusInfo.Empty;
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