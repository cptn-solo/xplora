using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct RelationsEventInfo
    {
        public EcsPackedEntity SourceEntity { get; set; }
        public EcsPackedEntity? TargetEntity { get; set; }

        public string EventTitle { get; internal set; }
        public string SrcIconName { get; internal set; }
        public string TgtIconName { get; internal set; }
        public string EventText { get; internal set; }

        public RelationScoreInfo ScoreInfo { get; internal set; }
        public RelationEventItemInfo[] EventItems { get; internal set; }

        public string[] ActionTitles { get; internal set; }
        public int SrcRSD { get; internal set; }
        public int SrcRSDAbs => Mathf.Abs(SrcRSD);

        public HeroKindGroup SrcKindGroup { get; internal set; }
        public int TgtRSD { get; internal set; }
        public int TgtRSDAbs => Mathf.Abs(TgtRSD);

        public HeroKindGroup TgtKindGroup { get; internal set; }
        public EcsPackedEntity ScoreEntity { get; internal set; }
        public int ScoreDiff { get; internal set; }
        public int Score { get; internal set; }
        public RelationTargetInfo Rule { get; internal set; }
    }
}