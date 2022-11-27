using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLineSlot : UIItemSlot, ITeamId
    {
        private Hero hero;        
        private int teamId;
        private BattleLine line;

        public int TeamId => teamId;
        public BattleLine Line;

        public void SetTeamId(int id) =>
            teamId = id;
        public void SetLine(BattleLine line) =>
            this.line = line;

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

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            raidMember = itemTransform.GetComponent<RaidMember>();
        }
    }
}