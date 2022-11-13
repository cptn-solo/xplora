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
       
        private RaidMember CreateCachedHeroCard(Hero hero, List<RaidMember> cachedItems)
        {
            RaidMember heroCard = Instantiate(heroPrefab).GetComponent<RaidMember>();
            heroCard.Hero = hero;
            heroCard.gameObject.SetActive(false);
            heroCard.transform.SetParent(transform);
            cachedItems.Add(heroCard);
            return heroCard;
        }
        private InventoryItem CreateCachedAssetCard(Asset asset, List<InventoryItem> cachedItems)
        {
            InventoryItem card = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            card.Asset = asset;
            card.gameObject.SetActive(false);
            card.transform.SetParent(transform);
            cachedItems.Add(card);
            return card;
        }

        internal RaidMember GetHeroCard(Hero hero)
        {
            RaidMember heroCard = null;

            if (!heroPool.TryGetValue(hero.Id, out List<RaidMember> cachedItems))
            {
                cachedItems = new();
                heroCard = CreateCachedHeroCard(hero, cachedItems);
                heroPool[hero.Id] = cachedItems;
            }

            if (cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is RaidMember instance)
            {
                heroCard = instance;
            }
            else
            {
                heroCard = CreateCachedHeroCard(hero, cachedItems);
            }

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
                assetCard = CreateCachedAssetCard(asset, cachedItems);                
                assetPool[asset.Code] = cachedItems;
            }

            if (cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is InventoryItem instance)
            {
                assetCard = instance;
            }
            else
            {
                assetCard = CreateCachedAssetCard(asset, cachedItems);
            }

            assetCard.gameObject.SetActive(true);

            return assetCard;            
        }

        #region IObjectPool (may be one day)
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
        #endregion
    }
}