namespace Assets.Scripts.World
{
    public class PoiAnimation : WorldObjectAnimation
    {
        private const string AnimStageActive = "Active";
        private const string AnimStageInactive = "Inactive";

        protected override string AnimStageIdle => AnimStageActive;
    }
}