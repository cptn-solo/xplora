using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleUnit : IEntityView<Hero>// ECS
    {
        public override void UpdateData()
        {
            var hero = DataLoader(PackedEntity.Value);
            SetHero(hero);
        }        
    }
}