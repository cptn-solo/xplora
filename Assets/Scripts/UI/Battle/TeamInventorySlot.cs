using Assets.Scripts.UI.Inventory;

namespace Assets.Scripts.UI.Battle
{
    public class TeamInventorySlot : AssetInventorySlot, ITeamId
    {
        private int teamId;

        public int TeamId => teamId;

        public void SetTeamId(int id) =>
            teamId = id;

    }
}