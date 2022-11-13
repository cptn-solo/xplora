using Assets.Scripts.UI.Battle;
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
        [SerializeField] private GameObject heroPrefab;

        private readonly Dictionary<string, List<InventoryItem>> assetPool = new();
        private readonly Dictionary<int, List<RaidMember>> heroPool = new();

        public bool TryGetValue(Asset asset, out List<InventoryItem> cachedItems) =>
            assetPool.TryGetValue(asset.Code, out cachedItems);

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
        
        internal RaidMember GetHeroCard(Hero hero)
        {
            RaidMember heroCard = null;

            if (!heroPool.TryGetValue(hero.Id, out List<RaidMember> cachedItems))
            {
                cachedItems = new();
                
                heroCard = Instantiate(heroPrefab).GetComponent<RaidMember>();
                heroCard.Hero = hero;
                cachedItems.Add(heroCard);

                heroPool[hero.Id] = cachedItems;
            }

            if (cachedItems.Where(x => !x.enabled).FirstOrDefault() is RaidMember instance)
            {
                heroCard = instance;
            }
            else
            {
                heroCard = Instantiate(heroPrefab).GetComponent<RaidMember>();
                heroCard.Hero = hero;
                cachedItems.Add(heroCard);
            }

            heroCard.transform.SetParent(transform);
            heroCard.gameObject.SetActive(true);

            return heroCard;

        }

        internal InventoryItem GetAssetCard(Asset asset)
        {
            InventoryItem assetCard = null;

            asset.Code ??= "NA";

            if (!assetPool.TryGetValue(asset.Code, out List<InventoryItem> cachedItems))
            {
                cachedItems = new();
                assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                assetCard.Asset = asset;
                cachedItems.Add(assetCard);
                
                assetPool[asset.Code] = cachedItems;
            }

            if (cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is InventoryItem instance)
            {
                assetCard = instance;
            }
            else
            {
                assetCard = Instantiate(itemPrefab).GetComponent<InventoryItem>();
                assetCard.Asset = asset;
                cachedItems.Add(assetCard);
            }

            assetCard.transform.SetParent(transform);
            assetCard.gameObject.SetActive(true);

            return assetCard;
            
        }
    }
}