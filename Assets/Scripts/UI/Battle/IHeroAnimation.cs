using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    internal interface IHeroAnimation
    {
        void Attack(bool range = false, Vector3 position = default);
        void Hit();
        void Death();
    }
}