using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Raid Member Delegate
    {
        private void InitRaidMemberDelegates()
        {
            heroDelegate.Validator = (RaidMember s) => {
                return assetTransfer.TransferAsset.AssetType != AssetType.NA;
            };
            heroDelegate.TransferEnd = (RaidMember s, bool accepted) =>
            {
                var success = assetTransfer.Commit(s.Hero.Inventory, -1, s.Hero);
                s.Hero = s.Hero;

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            heroDelegate.TransferAbort = (RaidMember s) =>
            {
                var success = false;
                success = assetTransfer.Abort();

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }
    }
}

