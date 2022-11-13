using Assets.Scripts.UI.Battle;

namespace Assets.Scripts.UI.Inventory
{
    public struct HeroDelegateProvider
    {
        public delegate bool AcceptableItemChecker(RaidMember slot);
        public delegate bool AssetTransferEndDelegate(RaidMember slot, bool accepted);
        public delegate bool AssetTransferAbortDelegate(RaidMember slot);

        public AcceptableItemChecker Validator { get; set; }
        public AssetTransferEndDelegate TransferEnd { get; set; }
        public AssetTransferAbortDelegate TransferAbort { get; set; }

    }
}