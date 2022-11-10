using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Battle
{
    public class RaidMember : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Image priAttackImage;
        [SerializeField] private Image secAttackImage;
        [SerializeField] private Image priDefenceImage;
        [SerializeField] private Image secDefenceImage;

        private BattleUnit unit;
        public BattleUnit Unit
        {
            get => unit;
            set
            {
                unit = value;
                ResolveIcons();
            }
        }

        private void ResolveIcons()
        {
            ResolveIcon(priAttackImage, unit.Hero.Attack[0]);
            ResolveIcon(secAttackImage, unit.Hero.Attack[1]);
            ResolveIcon(priDefenceImage, unit.Hero.Defence[0]);
            ResolveIcon(secDefenceImage, unit.Hero.Defence[1]);
        }

        private void ResolveIcon(Image image, Asset asset)
        {
            image.sprite = null;
            image.enabled = false;
            if (!asset.Equals(default) && asset.IconName != null)
            {
                image.sprite = SpriteForResourceName(asset.IconName);
                image.enabled = true;
            }
        }

        private Sprite SpriteForResourceName(string iconName)
        {
            var icon = Resources.Load<Sprite>(iconName);
            return icon;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var cargo = eventData.pointerDrag.transform;

            if (cargo.GetComponent<InventoryItem>() is InventoryItem inventoryItem)
            {
                Debug.Log($"InventoryItem: {inventoryItem}");
            }
        }
    }
}