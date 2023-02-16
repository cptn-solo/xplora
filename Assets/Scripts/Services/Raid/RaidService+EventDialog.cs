using Assets.Scripts.Data;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Event Dialog
    {
        private IEventDialog<WorldEventInfo> dialog;

        internal void OnEventAction<T>(int idx) where T : struct
        {
            if (typeof(T) == typeof(WorldEventInfo))
            {
                dialog.Dismiss();
                //TODO: apply event result for button idx
            }
        }

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

            if (dialog != null && eventHero.HeroType != HeroType.NA)
            {
                var info = new WorldEventInfo()
                {
                    EventTitle = eventConfig.Name,
                    EventText = "Event description",
                    IconName = eventHero.IconName,
                    ActionTitles = new string[2]
                    {
                        "1st action",
                        "2nd action"
                    }
                };
                dialog.SetEventInfo(info);
            }

        }
    }

}

