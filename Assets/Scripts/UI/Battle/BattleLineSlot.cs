using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class BattleLineSlot : UIItemSlot
    {
        private Hero hero;        
        private RaidMember raidMember;

        public Hero Hero { 
            get => hero; 
            set
            {
                hero = value;
                if (transform.childCount == 0)
                    return;

                raidMember.Hero = hero;
            }
        }

        public RaidMember RaidMember => raidMember;

        public HeroPosition Position { get; internal set; }

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            raidMember = itemTransform.GetComponent<RaidMember>();
        }
    }
}