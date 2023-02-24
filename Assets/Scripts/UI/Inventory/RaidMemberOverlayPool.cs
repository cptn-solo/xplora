using Leopotam.EcsLite;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Inventory
{
    public class RaidMemberOverlayPool: BaseCardPool<Overlay, BarsAndEffectsInfo>
    {
        public Overlay CreateOverlay(
            EcsPackedEntityWithWorld heroInstance)
        {
            return base.CreateCard(heroInstance);
        }

    }
}