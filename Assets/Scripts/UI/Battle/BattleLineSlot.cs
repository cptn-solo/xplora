﻿using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class BattleLineSlot : UIItemSlot
    {
        private Hero hero;        
        private RaidMember raidMember;

        public Hero Hero => hero;

        public void SetHero(Hero? hero, bool isPlayerTeam = false){ 
            this.hero = hero == null ? default : hero.Value;
            if (transform.childCount == 0)
                return;

            raidMember.SetHero(hero, isPlayerTeam);
        }
        public void SetBarsAndEffects(List<BarInfo> bars, Dictionary<DamageEffect, int> effects) =>
            raidMember.SetBarsAndEffects(bars, effects);

        public RaidMember RaidMember => raidMember;

        public HeroPosition Position { get; internal set; }

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            raidMember = itemTransform.GetComponent<RaidMember>();
        }
    }
}