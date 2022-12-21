using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    internal delegate void DoStage();
    internal class VisualDelegate : Tuple<DoStage, WaitForSeconds>
    {
        private static readonly Dictionary<float, WaitForSeconds> waiters = new();
        private static WaitForSeconds GetWaiter(float sec)
        {
            if (waiters.TryGetValue(sec, out var waiter))
                return waiter;

            var ret = new WaitForSeconds(sec);
            waiters[sec] = ret;

            return ret;
        }
        public DoStage Callback => Item1;
        public WaitForSeconds Waiter => Item2;
        public VisualDelegate(DoStage d, float? sec) : 
            base(d, sec == null ? null : GetWaiter((float)sec)) { }
    }
}