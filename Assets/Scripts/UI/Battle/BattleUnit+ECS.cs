using Leopotam.EcsLite;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleUnit : IEntityView<Hero>// ECS
    {
        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public DataLoadDelegate<Hero> DataLoader { get; set; }

        public Transform Transform => transform;

        public void UpdateData()
        {
            var hero = DataLoader(PackedEntity.Value);
            SetHero(hero);
        }
        public void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        
    }

}