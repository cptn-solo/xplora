using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleInventory : MonoBehaviour
    {

        [SerializeField] private RectTransform inventoryPanel;

        internal void Toggle(bool toggle)
        {
            inventoryPanel.gameObject.SetActive(toggle);
        }
    }
}