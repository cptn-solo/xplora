using Assets.Scripts.UI.Battle;

namespace Assets.Scripts.UI.Inventory
{
    public struct HeroDelegateProvider
    {
        public delegate bool AcceptableItemChecker(BattleUnit slot);
        public delegate bool AssetTransferEndDelegate(BattleUnit slot, bool accepted);
        public delegate bool AssetTransferAbortDelegate(BattleUnit slot);

        public AcceptableItemChecker Validator { get; set; }
        public AssetTransferEndDelegate TransferEnd { get; set; }
        public AssetTransferAbortDelegate TransferAbort { get; set; }

    }
}