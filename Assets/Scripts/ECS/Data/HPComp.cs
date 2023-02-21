using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    /// <summary>
    /// Current HP
    /// </summary>
    public struct HPComp
    {
        public int HP { get; set; }

        public HPComp UpdateHealthCurrent(int damage, int initial, out int displayVal, out int currentVal)
        {
            var result = Mathf.Max(0, HP - damage);
            var ratio = (float)(result / initial);
            displayVal = (int)(100f * ratio);
            currentVal = result;
            HP = result;

            return this;
        }

    }




}


