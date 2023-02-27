using Assets.Scripts.Data;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Event Dialog
    {
        private IEventDialog<WorldEventInfo> dialog;
        private WorldEventInfo? currentEventInfo;

        internal void RegisterEventDialog<T>(IEventDialog<T> dialog) where T : struct
        {
            if (typeof(T) == typeof(WorldEventInfo))
                this.dialog = (IEventDialog<WorldEventInfo>)dialog;
        }

        internal void UnregisterEventDialog<T>(IEventDialog<T> dialog) where T : struct
        {
            if (typeof(T) == typeof(WorldEventInfo))
                this.dialog = null;
        }

        private void ProcessTerrainAttribute(TerrainAttribute attribute)
        {
            var playerHeroes = GetActiveTeamMembers();

            if (playerHeroes.Length == 0)
                return;

            //NB: for now it is 1:1 mapping, can be extended later
            var eventConfig = worldService.TerrainEventsLibrary.TerrainEvents[attribute];
            var maxedHeroes = ListPool<Hero>.Get();
            var maxLevel = 0;

            Hero eventHero = default;

            foreach (var hero in playerHeroes)
            {
                if (hero.Traits.TryGetValue(eventConfig.Trait, out var traitInfo) &&
                    traitInfo.Level > maxLevel)
                {
                    maxLevel = traitInfo.Level;
                    maxedHeroes.Add(hero);
                }
            }
            if (maxedHeroes.Count > 0)
                eventHero = maxedHeroes[Random.Range(0, maxedHeroes.Count)];

            ListPool<Hero>.Add(maxedHeroes);

            if (dialog == null || eventHero.HeroType == HeroType.NA)
                return;

            ShowEvent(eventConfig, eventHero, maxLevel);
        }

        private void ShowEvent(TerrainEventConfig eventConfig, Hero eventHero, int maxLevel)
        {
            currentEventInfo = WorldEventInfo.Create(eventConfig, eventHero, maxLevel);

            dialog.SetEventInfo(currentEventInfo.Value);

        }

        internal void OnEventAction<T>(int idx) where T : struct
        {
            if (typeof(T) == typeof(WorldEventInfo))
            {

                dialog.Dismiss();
                // apply event result for button idx

                var selectedOption = currentEventInfo.Value.BonusOptions[idx];

                switch (idx)
                {
                    case 0:// permanent boost
                        {
                            //1. update hero in the library
                            libManagementService.BoostSpecOption(
                                currentEventInfo.Value.EventHero,
                                selectedOption.SpecOption,
                                selectedOption.Factor);

                            //2. update hero in the raid entity
                            // redundant if Raid references lib hero config
                        }
                        break;
                    case 1:// next battle boost
                        {
                            //1. add bonus field to hero struct
                            //2. update bonus field in hero in the library
                            //3. implement bonus field handling in battle manager (including reset after the battle)
                            BoostEcsNextBattleSpecOption(
                                currentEventInfo.Value.EventHero,
                                selectedOption.SpecOption,
                                selectedOption.Factor);

                            if (currentEventInfo.Value.BonusOptions.Length > idx + 1)
                            {
                                var additionalOption = currentEventInfo.Value.BonusOptions[idx + 1];
                                //1. increment hero trait in the library
                                libManagementService.BoostTraitOption(
                                    currentEventInfo.Value.EventHero,
                                    additionalOption.TraitOption,
                                    additionalOption.Factor);

                                //2. increment hero trait in the raid entity
                                // redundant if Raid references lib hero config

                            }
                        }
                        break;
                    default:
                        break;
                }

                currentEventInfo = null;
            }
        }
    }

}

