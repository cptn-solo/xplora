using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using UnityEngine;

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