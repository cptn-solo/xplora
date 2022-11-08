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
            ResolveIcon(priAttackImage, unit.PriAttack);
            ResolveIcon(secAttackImage, unit.SecAttack);
            ResolveIcon(priDefenceImage, unit.PriDefence);
            ResolveIcon(secDefenceImage, unit.SecDefence);
        }

        private void ResolveIcon(Image image, UnitItem item)
        {
            image.sprite = null;
            image.enabled = false;
            if (item != null && item.IconName != null)
            {
                image.sprite = SpriteForResourceName(item.IconName);
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