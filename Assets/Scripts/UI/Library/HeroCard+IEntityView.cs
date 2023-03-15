using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Library
{
    public partial class HeroCard : IEntityView<Hero>
    {
        public override void UpdateData() =>
            Hero = DataLoader(PackedEntity.Value);
    }
}