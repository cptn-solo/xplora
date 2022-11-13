using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLineSlot : UIItemSlot
    {
        private Hero hero;
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

        private RaidMember raidMember;
        public RaidMember RaidMember => raidMember;

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            raidMember = itemTransform.GetComponent<RaidMember>();
        }
    }
}