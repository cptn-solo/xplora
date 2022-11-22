using Assets.Scripts.UI.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueueSlot : MonoBehaviour
    {
        [SerializeField] private int teamId;
        [SerializeField] private int queueIndex;

        private QueueMember queueMember;

        public QueueMember QueueMember => queueMember;

        public int TeamId => teamId;
        public int QueueIndex => queueIndex;
        
        public void InitQueueMember()
        {
            queueMember = GetComponentInChildren<QueueMember>();
            queueMember.Hero = Hero.Default;
        }
    }
}