using Assets.Scripts.UI.Data;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueueSlot : MonoBehaviour
    {
        [SerializeField] private int queueIndex;

        private QueueMember queueMember;

        public QueueMember QueueMember => queueMember;

        public int QueueIndex => queueIndex;
        
        public void InitQueueMember(Hero hero, DamageEffect[] effects)
        {
            queueMember = GetComponentInChildren<QueueMember>();
            queueMember.Hero = hero;
            queueMember.SetEffects(effects);
        }
    }
}