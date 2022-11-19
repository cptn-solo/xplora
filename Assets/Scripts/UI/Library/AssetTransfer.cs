using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using Asset = Assets.Scripts.UI.Data.Asset;

namespace Assets.Scripts
{
    using AssetDict = Dictionary<int, Asset>;
    public class AssetTransfer
    {
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
        

        private AssetTransaction transaction = default;

        public Asset TransferAsset => transaction.Asset;

        public void Begin(AssetDict fromInventory, int fromIdx, Hero fromHero = default)
        {
            transaction = new AssetTransaction
            {
                FromInventory = fromInventory,
                FromIdx = fromIdx,
                FromHero = fromHero,
                Asset = fromInventory[fromIdx],
            };
        }
        public bool Commit(AssetDict toInventory, int toIdx, Hero toHero = default)
        {
            if (transaction.Asset.AssetType == AssetType.NA)
                return false;

            if (toIdx < 0 && toHero.HeroType != HeroType.NA)
            {
                int tmpIdx = transaction.Asset.AssetType switch
                {
                    AssetType.Attack => toHero.Attack.FirstFreeSlotIndex(x => x.AssetType == AssetType.NA),
                    AssetType.Defence => toHero.Defence.FirstFreeSlotIndex(x => x.AssetType == AssetType.NA),
                    _ => -1
                };
                if (tmpIdx >= 0)
                {
                    toInventory = transaction.Asset.AssetType switch
                    {
                        AssetType.Attack => toHero.Attack,
                        AssetType.Defence => toHero.Defence,
                        _ => toHero.Inventory
                    };
                    toIdx = tmpIdx;
                }
            }
            
            transaction.ToInventory = toInventory;
            transaction.ToIdx = toIdx;
            transaction.ToHero = toHero;

            transaction.FromInventory.TakeAsset(transaction.FromIdx);
            transaction.ToInventory.PutAsset(transaction.Asset, transaction.ToIdx);
            
            transaction = default;
            return true;
        }


        public bool Abort()
        {
            transaction = default;
            
            return true;
        }


    }    
}