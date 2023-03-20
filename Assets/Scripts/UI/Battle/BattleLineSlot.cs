using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLineSlot : UIItemSlot, IHeroPosition, ITransform
    {
        private BattleUnit battleUnit;

        public BattleUnit BattleUnit => battleUnit;

        #region IHeroPosition

        public HeroPosition Position { get; internal set; }

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);

            if (itemTransform == null)
                return;

            battleUnit = itemTransform.GetComponent<BattleUnit>();
        }

        public void Reset()
        {
            battleUnit = null;
        }

        #endregion

        #region ITransform

        public Transform Transform => transform;

        public void OnGameObjectDestroy()
        {
            // Nothing to do here
        }


        #endregion

    }
}