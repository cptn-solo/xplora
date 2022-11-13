using Assets.Scripts.UI.Data;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Raid Member Delegate
    {
        private void InitRaidMemberDelegates()
        {
            heroDelegate.Validator = (RaidMember s) => {
                return teamManager.TransferAsset.AssetType != AssetType.NA;
            };
            heroDelegate.TransferEnd = (RaidMember s, bool accepted) =>
            {
                var success = teamManager.CommitAssetTransfer(s.Hero.Inventory, -1, s.Hero);
                s.Hero = s.Hero;

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            heroDelegate.TransferAbort = (RaidMember s) =>
            {
                var success = false;
                success = teamManager.AbortAssetTransfer();

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }
    }
}

