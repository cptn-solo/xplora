using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Data
{
    public static class DataUtils
    {
        public static bool RatedRandomBool(this int rate)
        {
            var ret = false;
            if (rate == 100)
            {
                ret = true;
            }
            else if (rate >= 1 &&
                rate < 100)
            {
                ret = 0 == Random.Range(0, (int)Mathf.Ceil(100 / rate));
            }
            return ret;
        }


    }


}