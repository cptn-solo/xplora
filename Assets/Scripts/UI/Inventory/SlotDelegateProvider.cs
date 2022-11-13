using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public struct SlotDelegateProvider
    {
        public delegate bool AcceptableItemChecker(UIItemSlot slot);
        public delegate void AssetTransferStartDelegate(UIItemSlot slot, Transform item);
        public delegate bool AssetTransferEndDelegate(UIItemSlot slot, bool accepted);
        public delegate bool AssetTransferAbortDelegate(UIItemSlot slot);
        public delegate Transform PoolDelegate(UIItemSlot slot);

        public AcceptableItemChecker Validator { get; set; }
        public AssetTransferStartDelegate TransferStart { get; set; }
        public AssetTransferEndDelegate TransferEnd { get; set; }
        public AssetTransferAbortDelegate TransferAbort { get; set; }
        public PoolDelegate Pool { get; set; }
    }
}