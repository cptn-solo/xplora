using System;

namespace Assets.Scripts.World
{
    public class PoiAnimation : WorldObjectAnimation
    {
        private const string AnimStageActive = "Active";
        private const string AnimStageInactive = "Inactive";

        private string currentSgate = AnimStageActive;

        protected override string AnimStageIdle => currentSgate;

        internal void SetActive(bool toggle)
        {
            currentSgate = toggle ? AnimStageActive : AnimStageInactive;

            Initialize();
        }
    }
}