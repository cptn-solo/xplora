using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts.UI.Inventory
{
    public class AssetPool : MonoBehaviour, IObjectPool<InventoryItem>
    {
        [SerializeField] private GameObject itemPrefab;

        private readonly Dictionary<AssetType, List<InventoryItem>> assetPool = new();

        public bool TryGetValue(AssetType assetType, out List<InventoryItem> cachedItems) =>
            assetPool.TryGetValue(assetType, out cachedItems);

        public int CountInactive => throw new System.NotImplementedException();

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public InventoryItem Get()
        {
            throw new System.NotImplementedException();
        }

        public PooledObject<InventoryItem> Get(out InventoryItem v)
        {
            throw new System.NotImplementedException();
        }

        public void Release(InventoryItem element)
        {
            throw new System.NotImplementedException();
        }

        internal InventoryItem GetAssetCard(AssetType assetType)
        {
            List<InventoryItem> cachedItems;
            InventoryItem assetCard = null;
            if (!assetPool.TryGetValue(assetType, out cachedItems))
            {
                cachedItems = new();
                assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                cachedItems.Add(assetCard);
                
                assetPool[assetType] = cachedItems;
            }

            if (cachedItems.Where(x => !x.enabled).FirstOrDefault() is InventoryItem instance)
            {
                assetCard = instance;
            }
            else {
                assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                cachedItems.Add(assetCard);
            }
            
            assetCard.transform.SetParent(transform);
            assetCard.gameObject.SetActive(true);

            return assetCard;
            
        }
    }
}