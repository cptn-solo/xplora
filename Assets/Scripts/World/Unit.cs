using Assets.Scripts.UI.Data;
using Assets.Scripts.World.HexMap;
using System.Collections;
using UnityEngine;

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

        [SerializeField] private Transform visual;
        [SerializeField] private float speedFactor = 5f; // adjustment for hero speed to move around the world
        
        private Coroutine activeMove = null;

        private void Awake()
        {
            unitAnimation = GetComponentInChildren<UnitAnimation>();
        }

        public void SetHero(Hero hero)
        {
            this.hero = hero;
            unitAnimation.SetHero(hero);
        }

        internal void MoveTo(HexCoordinates targetCoord)
        {
            this.targetCoord = targetCoord;
            if (isMoving || activeMove != null)
            {
                isMoving = false;
                StopCoroutine(activeMove);
            }
            activeMove = StartCoroutine(MoveCoroutine());
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
            
            Debug.Log($"MoveCoroutine {targetPos}");

            isMoving = true;
            
            var dir = (targetPos - transform.position).normalized;

            Flip(dir.x < 0);

            unitAnimation.Run(true);
            while (isMoving)
            {
                transform.position += Hero.Speed * speedFactor * Time.deltaTime * dir;

                if (Vector3.SqrMagnitude(targetPos - transform.position) <= .01f)
                {
                    coordinates = targetCoord;
                    break;
                }

                yield return null;
            }
            unitAnimation.Run(false);
            isMoving = false;
        }
    }
}