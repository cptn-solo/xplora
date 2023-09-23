using Leopotam.EcsLite;

namespace Assets.Scripts.Data
{

    public struct RelationsEventToastInfo : IDismissTimer
    {
        public EcsPackedEntityWithWorld SourceEntity { get; set; }
        public EcsPackedEntityWithWorld? TargetEntity { get; set; }

        public string SrcIconName { get; internal set; }
        public string TgtIconName { get; internal set; }

        public RelationScoreInfo ScoreInfo { get; internal set; }
        public float DismissTimer { get; set; }
    }
}