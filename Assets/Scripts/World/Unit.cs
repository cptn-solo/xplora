using Assets.Scripts.Data;
using Assets.Scripts.World.HexMap;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.World
{
    public class Unit : MonoBehaviour
    {
        private UnitAnimation unitAnimation;
        private Hero hero;
        private HexCoordinates coordinates;
        private HexCoordinates targetCoord;
        private bool isMoving;

        public Hero Hero => hero;
        public HexCoordinates CurrentCoord => coordinates;

        /// <summary>
        /// Assume we can move only 1 cell per turn
        /// </summary>
        public int WorldRange => 1;

        public event UnityAction<HexCoordinates, Unit> OnArrivedToCoordinates;
        public event UnityAction<HexCoordinates, HexCoordinates, Unit> OnDepartedFromCoordinates;
        public event UnityAction<HexCoordinates, Unit> OnMoveTargetSetToCoordinates;

        [SerializeField] private Transform visual;
        [SerializeField] private float speedFactor = 5f; // adjustment for hero speed to move around the world

        private const float approachThreshold = 1f;
        
        private Coroutine activeMove = null;

        private void Awake()
        {
            unitAnimation = GetComponentInChildren<UnitAnimation>();
        }

        public void SetInitialCoordinates(HexCoordinates initialCoordinates)
        {
            coordinates = initialCoordinates;
        }

        public void SetHero(Hero hero)
        {
            this.hero = hero;
            unitAnimation.SetHero(hero);
        }

        internal void SetMoveTargetCoordinates(HexCoordinates targetCoord)
        {
            this.targetCoord = targetCoord;
            OnMoveTargetSetToCoordinates?.Invoke(targetCoord, this);
        }

        internal void MoveToTargetCoordinates()
        {            
            if (isMoving || activeMove != null)
            {
                isMoving = false;
                StopCoroutine(activeMove);
            }
            activeMove = StartCoroutine(MoveCoroutine());
            OnDepartedFromCoordinates?.Invoke(coordinates, targetCoord, this);
        }

        internal void Flip(bool flip)
        {
            var r = visual.transform.rotation;
            r.y = flip ? 180f : 0f;
            visual.transform.rotation = r;
        }

        private IEnumerator MoveCoroutine()
        {
            var targetPos = targetCoord.ToPosition();
            
            isMoving = true;
            
            var dir = (targetPos - transform.position).normalized;

            Flip(dir.x < 0);

            unitAnimation.Run(true);
            while (isMoving)
            {
                var maxDistance = Hero.Speed * speedFactor * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, maxDistance);
                var dif = Vector3.SqrMagnitude(targetPos - transform.position);
                if (dif <= approachThreshold)
                {
                    coordinates = targetCoord;
                    OnArrivedToCoordinates?.Invoke(coordinates, this);
                    break;
                }

                yield return null;
            }
            unitAnimation.Run(false);
            isMoving = false;
        }
    }
}