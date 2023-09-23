using UnityEngine;

namespace Assets.Scripts.World
{
    public partial class TeamMember // Score
    {
        public void SetScore(int? score)
        {
            if (score != null)
            {
                this.scoreText.text = $"{Mathf.Abs(score.Value)}";
                this.scoreText.color = score > 0 ? Color.green : score < 0 ? Color.red : Color.white;
            }
            else
            {
                this.scoreText.text = "";
            }
        }
    }
}