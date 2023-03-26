using Assets.Scripts.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public class HeroRelationsScorePanel: MonoBehaviour
    {
        [SerializeField] private BarWithTitle positiveBar;
        [SerializeField] private BarWithTitle negativeBar;

        internal void SetInfo(RelationScoreInfo scoreInfo)
        {
            positiveBar.gameObject.SetActive(false);
            negativeBar.gameObject.SetActive(false);

            if (scoreInfo.ScoreSign >= 0)
            {
                positiveBar.SetInfo(scoreInfo.ScoreInfo);
                positiveBar.gameObject.SetActive(true);
            }
            else
            {
                negativeBar.SetInfo(scoreInfo.ScoreInfo);
                negativeBar.gameObject.SetActive(true);
            }
        }
    }
}