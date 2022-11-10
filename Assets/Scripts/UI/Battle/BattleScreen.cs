using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen : MenuScreen
    {
        [SerializeField] private RectTransform playerPartyFront;
        [SerializeField] private RectTransform playerPartyBack;

        [SerializeField] private RectTransform enemyPartyFront;
        [SerializeField] private RectTransform enemyPartyBack;
        
        [SerializeField] private RectTransform teamInventory;
        [SerializeField] private RectTransform heroInventory;

        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject itemPrefab;


        private readonly UIItemSlot[] playerFrontSlots = new UIItemSlot[4];
        private readonly UIItemSlot[] playerBackSlots = new UIItemSlot[4];
        
        private readonly UIItemSlot[] teamInventorySlots = new UIItemSlot[15];
        private readonly UIItemSlot[] heroInventorySlots = new UIItemSlot[10];
        private readonly UIItemSlot[] heroAttackSlots = new UIItemSlot[2];
        private readonly UIItemSlot[] heroDefenceSlots = new UIItemSlot[2];

        private Team team = Team.EmptyTeam(0, "A Team");
        private readonly List<Hero> heroes = new();

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
            InitInventorySlots();

            var hp = Asset.EmptyAsset(AssetType.Defence, "HP", "hp");
            var shield = Asset.EmptyAsset(AssetType.Defence, "Shield", "shield");
            var bomb = Asset.EmptyAsset(AssetType.Attack, "Bomb", "bomb");
            var shuriken = Asset.EmptyAsset(AssetType.Attack, "Shuriken", "shuriken");
            var power = Asset.EmptyAsset(AssetType.Attack, "Power", "power");

            hp.GiveAsset(team, 5);
            shield.GiveAsset(team, 5);
            bomb.GiveAsset(team, 5);
            shuriken.GiveAsset(team, 5);
            power.GiveAsset(team, 5);

            foreach (var asset in team.Inventory)
                teamInventorySlots[asset.Key].Put(ItemForAsset(asset.Value).transform);

            for (int i = 0; i < 4; i++)
                heroes.Add(Hero.EmptyHero(i, $"Hero {i}"));

            hp.GiveAsset(heroes[0], 2);
            hp.GiveAsset(heroes[2], 2);
            shield.GiveAsset(heroes[2], 2);
            shield.GiveAsset(heroes[3], 3);
            bomb.GiveAsset(heroes[3], 2);
            bomb.GiveAsset(heroes[0], 2);
            shuriken.GiveAsset(heroes[0], 1);
            shuriken.GiveAsset(heroes[1], 3);
            power.GiveAsset(heroes[1], 2);
            power.GiveAsset(heroes[2], 1);


            AddBattleUnit(heroes[0], BattleLine.Front);           
            AddBattleUnit(heroes[1], BattleLine.Front);
            AddBattleUnit(heroes[2], BattleLine.Front);
            AddBattleUnit(heroes[3], BattleLine.Back);
        }

        private void AddBattleUnit(Hero hero, BattleLine line)
        {
            var bu1 = new BattleUnit
            {
                Line = line,
                Hero = hero,
            };

            AddPlayerUnit(bu1);
        }        

        private void InitPlayerHeroSlots()
        {
            foreach (var slot in playerPartyFront.GetComponentsInChildren<BattleLineSlot>())
                playerFrontSlots[slot.SlotIndex] = slot;

            foreach (var slot in playerPartyBack.GetComponentsInChildren<BattleLineSlot>())
                playerBackSlots[slot.SlotIndex] = slot;

        }
        private void InitInventorySlots()
        {
            foreach (var slot in teamInventory.GetComponentsInChildren<TeamInventorySlot>())
                teamInventorySlots[slot.SlotIndex] = slot;

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroInventorySlot>())
                heroInventorySlots[slot.SlotIndex] = slot;

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroAttackSlot>())
                heroAttackSlots[slot.SlotIndex] = slot;

            foreach (var slot in heroInventory.GetComponentsInChildren<HeroDefenceSlot>())
                heroDefenceSlots[slot.SlotIndex] = slot;
        }

        private UIItemSlot FirstAvailableHeroSlot(BattleLine line)
        {
            var slots = line switch
            {
                BattleLine.Back => playerBackSlots,
                BattleLine.Front => playerFrontSlots,
                _ => null
            };

            return FirstFreeSlot(slots);
        }
        
        private static UIItemSlot FirstFreeSlot(UIItemSlot[] slots)
        {
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