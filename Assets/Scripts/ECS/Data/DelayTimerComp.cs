using UnityEngine;

namespace Assets.Scripts.ECS.Data
{
    public struct DelayTimerComp<T>
    {
        public float DelayUntill;

        public bool Ready =>
            Time.time >= DelayUntill;

        public void SetDelayFromNow(float delaySec)
        {
            DelayUntill = Time.time + delaySec;
        }
    }
}


