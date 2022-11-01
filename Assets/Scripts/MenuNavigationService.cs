﻿using Assets.Scripts.UI;
using UnityEngine;

namespace Assets.Scripts
{
    public class MenuNavigationService : MonoBehaviour
    {
        public UIManager UIManager { get; set; }

        public void NavigateToScreen(Screens screen)
        {
            Debug.Log(screen);

            if (UIManager == null)
                return;

            UIManager.ToggleScreen(screen);

            switch (screen)
            {
                case Screens.Raid:
                    break;
                case Screens.Missions:
                    break;
                case Screens.Resources:
                    break;
                case Screens.Heroes:
                    break;
                case Screens.Buildings:
                    break;
                default:
                    break;
            }
        }


    }
}