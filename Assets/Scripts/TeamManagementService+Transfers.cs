using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts
{
    using AssetDict = Dictionary<int, Asset>;
    using HeroDict = Dictionary<int, Hero>;

    public partial class TeamManagementService // Asset Transfers
    {
        public event UnityAction<AssetTransaction> OnAssetTransactionCompleted;
        public event UnityAction<AssetTransaction> OnAssetTransactionAborted;
        public struct AssetTransaction
        {
            public AssetDict FromInventory;
            public int FromIdx;
            public AssetDict ToInventory;
            public int ToIdx;
            public Asset Asset;
            public Hero FromHero;
            public Hero ToHero;

        }
        public struct HeroTransaction
        {
            public Hero Hero;
            public HeroDict FromLine;
            public int FromIdx;
            public HeroDict ToLine;
            public int ToIdx;

        }

        private AssetTransaction assetTransaction = default;
        private HeroTransaction heroTransaction = default;

        public void BeginAssetTransfer(AssetDict fromInventory, int fromIdx, Asset asset, Hero fromHero = default)
        {
            assetTransaction = new AssetTransaction
            {
                FromInventory = fromInventory,
                FromIdx = fromIdx,
                FromHero = fromHero,
                Asset = asset,
            };
        }
        public bool CommitAssetTransfer(AssetDict toInventory, int toIdx, Hero toHero = default)
        {
            if (assetTransaction.Equals(default))
            {
                OnAssetTransactionAborted?.Invoke(assetTransaction);
                return false;
            }

            if (toIdx < 0 && toHero.HeroType != HeroType.NA)
            {
                int tmpIdx = assetTransaction.Asset.AssetType switch
                {
                    AssetType.Attack => toHero.Attack.FirstFreeSlotIndex(x => x.AssetType == AssetType.NA),
                    AssetType.Defence => toHero.Defence.FirstFreeSlotIndex(x => x.AssetType == AssetType.NA),
                    _ => -1
                };
                if (tmpIdx >= 0)
                {
                    toInventory = assetTransaction.Asset.AssetType switch
                    {
                        AssetType.Attack => toHero.Attack,
                        AssetType.Defence => toHero.Defence,
                        _ => toHero.Inventory
                    };
                    toIdx = tmpIdx;
                }
            }
            
            assetTransaction.ToInventory = toInventory;
            assetTransaction.ToIdx = toIdx;
            assetTransaction.ToHero = toHero;

            assetTransaction.FromInventory.TakeAsset(assetTransaction.FromIdx);
            assetTransaction.ToInventory.PutAsset(assetTransaction.Asset, assetTransaction.ToIdx);
            
            OnAssetTransactionCompleted?.Invoke(assetTransaction);
            
            assetTransaction = default;
            return true;
        }

        public void BeginHeroTransfer(HeroDict from, Hero hero, int fromIdx)
        {
            heroTransaction = new HeroTransaction
            {
                Hero = hero,
                FromLine = from,
                FromIdx = fromIdx,
            };
        }

        public bool CommitHeroTransfer(HeroDict toLine, int toIndex, Hero hero)
        {
            if (heroTransaction.Equals(default))
                return false;

            heroTransaction.ToLine = toLine;
            heroTransaction.ToIdx = toIndex;

            var moved = team.MoveHero(hero, heroTransaction.FromLine, heroTransaction.FromIdx, toLine, toIndex);
            Debug.Log($"{team.BackLine} {team.FrontLine} {moved}");
            
            heroTransaction = default;
            
            return true;
        }

    }    
}