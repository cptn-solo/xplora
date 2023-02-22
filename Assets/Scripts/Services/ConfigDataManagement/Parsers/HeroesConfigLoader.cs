using Assets.Scripts.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services
{
    public delegate ref Hero HeroConfigProcessor(int id);

    public class HeroesConfigLoader : BaseConfigLoader
    {
        protected override string RangeString => "'Герои'!A1:Q40";
        protected override string ConfigName => "Heroes";
        private const int heroesNumber = 16;

        private const string heroesIconsPath = "Heroes/Icons";
        private const string heroesIdleSpritesPath = "Heroes/IdleSprites";

        private readonly HeroConfigProcessor processor;

        public HeroesConfigLoader(HeroConfigProcessor processor, DataDelegate onDataAvailable) : 
            base(onDataAvailable) {
            this.processor = processor;
        }

        public HeroesConfigLoader() :
            base()
        {
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            if (list == null || list.Count < 3 || processor == null)
                return;

            object val(int row, int cell)
            {
                if (list.Count > row &&
                    list[row] is var rowValues &&
                    rowValues.Count > cell)
                    return rowValues[cell];
                return "";
            }

            for (int idx = 0; idx < heroesNumber; idx++)
            {

                var cell = idx + 1; // 1st column is used for headers
                var typeName = (string)val(0, cell);
                var heroName = (string)val(1, cell);

                var iconName = $"{heroesIconsPath}/{typeName}";
                var idleSpriteName = $"{heroesIdleSpritesPath}/{typeName}";

                ref var hero = ref processor(idx);

                hero.Id = idx;
                hero.Name = heroName;
                hero.IconName = iconName;
                hero.IdleSpriteName = idleSpriteName;

                UpdateHero(ref hero, val, cell);
            }            
        }
        
        private void UpdateHero(ref Hero hero, ValueGetter val, int cell)
        {
            var rowIndex = 0;
            hero.Domain = val(++rowIndex, cell).ParseHeroDomain();

            if (hero.Domain != HeroDomain.Hero)
                hero.HeroType = HeroType.Beast;

            hero.Name = (string)val(++rowIndex, cell);

            rowIndex = 4;

            val(++rowIndex, cell).ParseAbsoluteRangeValue(out int minVal, out int maxVal);
            hero.DamageMin = minVal;
            hero.DamageMax = maxVal;

            hero.DefenceRate = val(++rowIndex, cell).ParseRateValue();
            hero.AccuracyRate = val(++rowIndex, cell).ParseRateValue();
            hero.DodgeRate = val(++rowIndex, cell).ParseRateValue();
            hero.Health = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.Speed = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.CriticalHitRate = val(++rowIndex, cell).ParseRateValue();
            hero.AttackType = val(++rowIndex, cell).ParseAttackType();
            hero.DamageType = val(++rowIndex, cell).ParseDamageType();
            hero.ResistBleedRate = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.ResistPoisonRate = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.ResistStunRate = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.ResistBurnRate = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.ResistFrostRate = val(++rowIndex, cell).ParseAbsoluteValue();
            hero.ResistFlushRate = val(++rowIndex, cell).ParseAbsoluteValue();

            rowIndex = 21;

            hero.SndAttack = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndDodged = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndHit = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndStunned = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndBleeding = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndPierced = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndBurning = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndFreezed = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndCritHit = val(++rowIndex, cell).ParseSoundFileValue();
            hero.SndDied = val(++rowIndex, cell).ParseSoundFileValue();

            rowIndex = 33;

            hero.Traits[HeroTrait.Hidden] = HeroTrait.Hidden.Level(val(++rowIndex, cell).ParseAbsoluteValue());
            hero.Traits[HeroTrait.Purist] = HeroTrait.Purist.Level(val(++rowIndex, cell).ParseAbsoluteValue());
            hero.Traits[HeroTrait.Shrumer] = HeroTrait.Shrumer.Level(val(++rowIndex, cell).ParseAbsoluteValue());
            hero.Traits[HeroTrait.Scout] = HeroTrait.Scout.Level(val(++rowIndex, cell).ParseAbsoluteValue());
            hero.Traits[HeroTrait.Tidy] = HeroTrait.Tidy.Level(val(++rowIndex, cell).ParseAbsoluteValue());
            hero.Traits[HeroTrait.Soft] = HeroTrait.Soft.Level(val(++rowIndex, cell).ParseAbsoluteValue());
        }
    }
}