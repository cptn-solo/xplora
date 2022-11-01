using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MenuScreen : MonoBehaviour
    {
        [SerializeField] private Screens screen;
        public Screens Screen => screen;
    }

}