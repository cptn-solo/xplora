using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen : MenuScreen
    {
        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;
        
        [SerializeField] private GameObject heroPrefab;


        private UIItemSlot[] playerFrontSlots = new UIItemSlot[4];
        private UIItemSlot[] playerBackSlots = new UIItemSlot[4];


        protected override void OnBeforeAwake() =>
            InitInputActions();
        protected override void OnBeforeEnable() =>
            EnableInputActions();

        protected override void OnBeforeDisable() =>
            DisableInputActions();

        protected override void OnBeforeUpdate() =>
            ProcessInputActions();

        protected override void OnBeforeStart()
        {

            InitPlayerHeroSlots();

            AddBattleUnit("shuriken", "shield", BattleLine.Front);
            AddBattleUnit("power", "shield",BattleLine.Back);
            AddBattleUnit("bomb", "hp", BattleLine.Front);

        }

        private void AddBattleUnit(string priAttIcon, string priDefIcon, BattleLine line)
        {
            var bu1 = new BattleUnit
            {
                Line = line,

                PriAttack = new Attack
                {
                    IconName = priAttIcon
                },

                PriDefence = new Defence
                {
                    IconName = priDefIcon
                }
            };

            AddPlayerUnit(bu1);
        }

        private void InitPlayerHeroSlots()
        {
            foreach (var slot in playerPartyFront.GetComponentsInChildren<UIItemSlot>())
                playerFrontSlots[slot.SlotIndex] = slot;

            foreach (var slot in playerPartyBack.GetComponentsInChildren<UIItemSlot>())
                playerBackSlots[slot.SlotIndex] = slot;

        }

        private UIItemSlot FirstAvailableSlot(BattleLine line)
        {
            var slots = line switch
            {
                BattleLine.Back => playerBackSlots,
                BattleLine.Front => playerFrontSlots,
                _ => null
            };
            
            if (slots == null)
                return null;

            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot.ItemTransform == null)
                    return slot;
            }

            return null;
        }
    }


}