using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public struct SlotDelegateProvider
    {
        public delegate bool AcceptableItemChecker(UIItemSlot slot);
        public delegate void AssetTransferStartDelegate(UIItemSlot slot, Transform item);
        public delegate bool AssetTransferEndDelegate(UIItemSlot slot);
        public delegate void AssetTransferCleanupDelegate(UIItemSlot slot);
        public delegate bool AssetTransferAbortDelegate(UIItemSlot slot);
        public delegate bool TransferEnabledChecker(UIItemSlot slot);
        public delegate Transform PoolDelegate(UIItemSlot slot);

        public AcceptableItemChecker Validator { get; set; }
        public TransferEnabledChecker TransferEnabled { get; set; }
        public AssetTransferStartDelegate TransferStart { get; set; }
        public AssetTransferEndDelegate TransferEnd { get; set; }
        public AssetTransferCleanupDelegate TransferCleanup { get; set; }
        public AssetTransferAbortDelegate TransferAbort { get; set; }
        public PoolDelegate Pool { get; set; }
    }
}