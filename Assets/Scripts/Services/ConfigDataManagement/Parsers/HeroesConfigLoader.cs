using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public class HeroesConfigLoader : BaseConfigLoader
    {

        protected override string RangeString => "'Герои'!A1:Q32";
        protected override string ConfigName => "Heroes";
        private const int heroesNumber = 16;

        private const string heroesIconsPath = "Heroes/Icons";
        private const string heroesIdleSpritesPath = "Heroes/IdleSprites";

        private readonly HeroesLibrary library;

        public HeroesConfigLoader(HeroesLibrary library, DataDelegate onDataAvailable) : 
            base(onDataAvailable) {
            this.library = library;
        }

        protected override void ProcessList(IList<IList<object>> list)
        {
            if (list == null || list.Count < 3)
                return;

            object val(int row, int cell) => list[row][cell];

            for (int col = 0; col < heroesNumber; col++)
            {

                var cell = col + 1; // 1st column is used for headers
                var id = col;
                var typeName = (string)val(0, cell);
                var heroName = (string)val(1, cell);

                var iconName = $"{heroesIconsPath}/{typeName}";
                var idleSpriteName = $"{heroesIdleSpritesPath}/{typeName}";

                if (library.HeroById(id) is Hero hero && hero.HeroType != HeroType.NA)
                {
                    UpdateHero(hero, val, cell);
                }
                else if (library.GiveHero(Hero.EmptyHero(id, heroName, iconName, idleSpriteName)))
                {
                    UpdateHero(library.HeroById(id), val, cell);
                }
            }            
        }
        
        private bool UpdateHero(Hero oldHero, ValueGetter val, int cell)
        {
            var hero = oldHero;
            var rowIndex = 0;
            hero.Domain = val(++rowIndex, cell).ParseHeroDomain();
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

            hero.HealthCurrent = hero.Health;

            library.UpdateHero(hero);

            return true;
        }
    }
}