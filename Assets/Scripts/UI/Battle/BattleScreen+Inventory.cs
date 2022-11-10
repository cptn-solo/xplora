using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Inventory
    {
        private GameObject ItemForAsset(Asset asset)
        {
            var item = Instantiate(itemPrefab);

            var inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.Asset = asset;
            return item;
        }
    }
}