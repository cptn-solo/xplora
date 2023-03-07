using System;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public partial class RaidService // Event Dialog
    {
        private IEventDialog<WorldEventInfo> dialog;
        public IEventDialog<WorldEventInfo> Dialog => dialog;

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

        internal void TryCastEcsTerrainEvent(TerrainEventConfig eventConfig,
            Hero eventHero, EcsPackedEntityWithWorld eventHeroEntity, int maxLevel)
        {
            if (!(5 + (maxLevel * 5)).RatedRandomBool())
            {
                Debug.Log($"Missed Event: {eventConfig}");
                return;
            }

            currentEventInfo = WorldEventInfo.Create(eventConfig,
                eventHero, eventHeroEntity, maxLevel);

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
                            BoostEcsTeamMemberSpecOption(
                                currentEventInfo.Value.HeroEntity,
                                selectedOption.SpecOption,
                                selectedOption.Factor);
                        }
                        break;
                    case 1:// next battle boost
                        {
                            //1. add bonus field to hero struct
                            //2. update bonus field in hero in the library
                            //3. implement bonus field handling in battle manager (including reset after the battle)
                            BoostEcsNextBattleSpecOption(
                                currentEventInfo.Value.HeroEntity,
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

