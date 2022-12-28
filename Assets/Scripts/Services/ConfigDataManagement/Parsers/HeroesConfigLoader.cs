using Assets.Scripts.UI.Data;
using System.Collections.Generic;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public class HeroesConfigLoader : BaseConfigLoader
    {

        private const int heroesNumber = 8;
        private const string heroesIconsPath = "Heroes/Icons";
        private const string heroesIdleSpritesPath = "Heroes/IdleSprites";

        private readonly HeroesLibrary library;
        protected override string RangeString => "'Герои'!A1:I31";
        protected override string ConfigName => "Heroes";

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

            hero.Name = (string)val(1, cell);

            val(4, cell).ParseAbsoluteRangeValue(out int minVal, out int maxVal);
            hero.DamageMin = minVal;
            hero.DamageMax = maxVal;

            hero.DefenceRate = val(5, cell).ParseRateValue();
            hero.AccuracyRate = val(6, cell).ParseRateValue();
            hero.DodgeRate = val(7, cell).ParseRateValue();
            hero.Health = val(8, cell).ParseAbsoluteValue();
            hero.Speed = val(9, cell).ParseAbsoluteValue();
            hero.CriticalHitRate = val(10, cell).ParseRateValue();
            hero.AttackType = val(11, cell).ParseAttackType();
            hero.DamageType = val(12, cell).ParseDamageType();
            hero.ResistBleedRate = val(13, cell).ParseAbsoluteValue();
            hero.ResistPoisonRate = val(14, cell).ParseAbsoluteValue();
            hero.ResistStunRate = val(15, cell).ParseAbsoluteValue();
            hero.ResistBurnRate = val(16, cell).ParseAbsoluteValue();
            hero.ResistFrostRate = val(17, cell).ParseAbsoluteValue();
            hero.ResistFlushRate = val(18, cell).ParseAbsoluteValue();

            hero.SndAttack = val(21, cell).ParseSoundFileValue();
            hero.SndDodged = val(22, cell).ParseSoundFileValue();
            hero.SndHit = val(23, cell).ParseSoundFileValue();
            hero.SndStunned = val(24, cell).ParseSoundFileValue();
            hero.SndBleeding = val(25, cell).ParseSoundFileValue();
            hero.SndPierced = val(26, cell).ParseSoundFileValue();
            hero.SndBurning = val(27, cell).ParseSoundFileValue();
            hero.SndFreezed = val(28, cell).ParseSoundFileValue();
            hero.SndCritHit = val(29, cell).ParseSoundFileValue();
            hero.SndDied = val(30, cell).ParseSoundFileValue();

            library.UpdateHero(hero);

            return true;
        }
    }
}