namespace Assets.Scripts.Data
{
    public struct BonusOptionConfig
    {
        /// <summary>
        /// NA if TraitOption used
        /// </summary>
        public SpecOption SpecOption { get; private set; }
        /// <summary>
        /// NA if SpecOption used
        /// </summary>
        public HeroTrait TraitOption { get; private set; }
        public int SpawnRate { get; private set; }
        public int Factor { get; private set; } //% for rates, value for abs

        public override string ToString()
        {
            var factorString = SpecOption switch
            {
                SpecOption.UnlimitedStaminaTag => "unlim",
                _ => $"{Factor}"
            };
            return $"{ (SpecOption != SpecOption.NA ? SpecOption.Name() : TraitOption.Name())} +{factorString}";
        }

        public static BonusOptionConfig Create(
            SpecOption specOption, HeroTrait traitOption, int spawnRate, int factor)
        {
            BonusOptionConfig retval = new()
            {
                SpecOption = specOption,
                TraitOption = traitOption,
                SpawnRate = spawnRate,
                Factor = factor
            };
            return retval;
        }
    }

    public struct EventBonusConfig
    {
        /// <summary>
        /// Expected:
        /// For button1 bonus we'll have 2 options with some probability rates.
        /// For button2 bonuses we'll have 1 option for explicit bonus with 100% spawn rate
        /// and 2 options for implicit bonus with some probability rates
        /// </summary>
        public BonusOptionConfig[] BonusConfigs { get; private set; }
        public static EventBonusConfig Create(BonusOptionConfig[] configs)
        {
            EventBonusConfig retval = new() { BonusConfigs = configs };
            return retval;
        }
    }

    public struct TerrainEventConfig
    {
        public TerrainAttribute Attribute { get; private set; }
        public HeroTrait Trait { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Expected:
        /// Button1 bonus + (button2 explicit bonus + button2 implicit bonus) = 3 bonuses total
        /// </summary>
        public EventBonusConfig[] BonusOptions { get; private set; }

        public static TerrainEventConfig Create(
            TerrainAttribute attribute,
            HeroTrait trait,
            string name,
            EventBonusConfig[] eventBonusConfigs)
        {
            return new()
            {
                Attribute = attribute,
                Trait = trait,
                Name = name,
                BonusOptions = eventBonusConfigs
            };
        }

    }
}