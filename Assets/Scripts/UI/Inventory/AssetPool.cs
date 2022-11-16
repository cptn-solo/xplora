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

        
        private RaidMember CreateCachedRaidMember(Hero hero, List<RaidMember> cachedItems, Vector3 scale)
        {
            RaidMember heroCard = Instantiate(heroPrefab).GetComponent<RaidMember>();
            heroCard.Hero = hero;
            heroCard.gameObject.SetActive(false);
            heroCard.transform.localScale = scale;
            heroCard.transform.SetParent(transform);
            cachedItems.Add(heroCard);
            return heroCard;
        }
        private InventoryItem CreateCachedAssetCard(Asset asset, List<InventoryItem> cachedItems, Vector3 scale)
        {
            InventoryItem card = Instantiate(itemPrefab).GetComponent<InventoryItem>();
            card.Asset = asset;
            card.gameObject.SetActive(false);
            card.transform.localScale = scale;
            card.transform.SetParent(transform);
            cachedItems.Add(card);
            return card;
        }

        internal RaidMember GetRaidMember(Hero hero, Vector3 scale)
        {
            RaidMember heroCard = null;

            if (!heroPool.TryGetValue(hero.Id, out List<RaidMember> cachedItems))
            {
                cachedItems = new();
                heroCard = CreateCachedRaidMember(hero, cachedItems, scale);
                heroPool[hero.Id] = cachedItems;
            }

            if (hero.HeroType != HeroType.NA &&
                cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is RaidMember instance)
            {
                heroCard = instance;
            }
            else
            {
                heroCard = CreateCachedRaidMember(hero, cachedItems, scale);
            }

            heroCard.gameObject.SetActive(hero.HeroType != HeroType.NA);

            return heroCard;

        }

        internal InventoryItem GetAssetCard(Asset asset, Vector3 scale)
        {
            InventoryItem assetCard = null;

            asset.Code ??= "NA";

            if (!assetPool.TryGetValue(asset.Code, out List<InventoryItem> cachedItems))
            {
                cachedItems = new();
                assetCard = CreateCachedAssetCard(asset, cachedItems, scale);                
                assetPool[asset.Code] = cachedItems;
            }

            if (asset.AssetType != AssetType.NA &&
                cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is InventoryItem instance)
            {
                assetCard = instance;
            }
            else
            {
                assetCard = CreateCachedAssetCard(asset, cachedItems, scale);
            }

            assetCard.gameObject.SetActive(asset.AssetType != AssetType.NA);

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