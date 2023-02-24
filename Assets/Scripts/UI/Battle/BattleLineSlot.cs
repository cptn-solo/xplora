using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Inventory;
using Assets.Scripts.UI.Library;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class BattleLineSlot : UIItemSlot, IHeroPosition, ITransform
    {
        private RaidMember raidMember;

        public RaidMember RaidMember => raidMember;

        #region IHeroPosition

        public HeroPosition Position { get; internal set; }

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);

            if (itemTransform == null)
                return;

            raidMember = itemTransform.GetComponent<RaidMember>();
        }

        public void Reset()
        {
            raidMember = null;
        }

        #endregion

        #region ITransform

        public Transform Transform => transform;

        #endregion

    }
}