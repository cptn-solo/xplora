
using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using Leopotam.EcsLite;

namespace Assets.Scripts.Battle
{
    public interface IHeroPosition //Slot
    {
        public Tuple<int, BattleLine, int> Position { get; }
        public IHeroInstanceEntity Unit { get; }
        public IBarsAndEffects UnitStateView { get; }
    }

    public interface IHeroInstanceEntity //RaidMember
    {
        public EcsPackedEntityWithWorld? HeroInstanceEntity { get; set; }
    }

    public interface IBarsAndEffects //Overlay
    {
        public void SetBarsEndEffectsInfo(
            List<BarInfo> barsInfoBattle,
            Dictionary<DamageEffect, int> effects);
    }

}