using Assets.Scripts.UI.Data;

namespace Assets.Scripts.UI.Inventory
{
    public class AssetInventorySlot : UIItemSlot
    {
        private Asset asset;
        public Asset Asset
        {
            get => asset;
            set
            {
                asset = value;
                if (transform.childCount == 0)
                    return;

                var cargo = transform.GetChild(0);
                var inventoryItem = cargo.GetComponent<InventoryItem>();
                inventoryItem.Asset = asset;
            }
        }

    }
}