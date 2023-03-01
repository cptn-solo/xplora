using Leopotam.EcsLite;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Inventory
{
    public class BattleUnitOverlayPool: BaseCardPool<Overlay, BarsAndEffectsInfo>
    {
        public Overlay CreateOverlay(
            EcsPackedEntityWithWorld heroInstance)
        {
            return base.CreateCard(heroInstance);
        }

    }
}