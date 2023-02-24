using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    /// <summary>
    /// Current HP
    /// </summary>
    public struct HPComp : IIntValue
    {
        public int Value { get; set; }

        public HPComp UpdateHealthCurrent(int damage, int initial, out int displayVal, out int currentVal)
        {
            var result = Mathf.Max(0, Value - damage);
            var ratio = (float)(result / initial);
            displayVal = (int)(100f * ratio);
            currentVal = result;
            Value = result;

            return this;
        }

    }




}


