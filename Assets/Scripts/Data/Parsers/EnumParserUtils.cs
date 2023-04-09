using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public static class EnumParserUtils
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
                    "критическийудар" => SpecOption.CritRate,
                    "сопротивляемостькровотечению" => SpecOption.BleedingResistanceRate,
                    "сопротивляемостьяду" => SpecOption.PoisonResistanceRate,
                    "сопротивляемостьоглушению" => SpecOption.StunResistanceRate,
                    "сопротивляемостьгорению" => SpecOption.BurningResistanceRate,
                    "сопротивляемостьхолоду" => SpecOption.FrozingResistanceRate,
                    "споротивляемостьослеплению" => SpecOption.BlindnessResistanceRate,
                    _ => SpecOption.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseSpecOption [{rawValue}] Exception: {ex.Message}");
                return SpecOption.NA;
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
        
        public static RelationsEffectType ParseRelationEffectType(this object rawValueObj)
        {
            string rawValue = (string)rawValueObj;

            try
            {
                var rawValues = rawValue
                    .Replace(" ", "")
                    .ToLower();

                return rawValues switch
                {
                    "specmaxmin" => RelationsEffectType.SpecMaxMin,
                    "specabs" => RelationsEffectType.SpecAbs,
                    "specpercent" => RelationsEffectType.SpecPercent,
                    "dmgeffectabs" => RelationsEffectType.DmgEffectAbs,
                    "dmgeffectpercent" => RelationsEffectType.DmgEffectPercent,
                    "dmgeffectbonusabs" => RelationsEffectType.DmgEffectBonusAbs,
                    "dmgeffectbonuspercent" => RelationsEffectType.DmgEffectBonusPercent,
                    "algorevenge" => RelationsEffectType.AlgoRevenge,
                    "algotarget" => RelationsEffectType.AlgoTarget,
                    "algodamagetypeblock" => RelationsEffectType.AlgoDamageTypeBlock,
                    _ => RelationsEffectType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseRelationEffectType [{rawValue}] Exception: {ex.Message}");
                return RelationsEffectType.NA;
            }

        }
    }
}