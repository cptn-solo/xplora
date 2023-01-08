using Assets.Scripts.UI.Data;
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.World
{
    public class Unit : MonoBehaviour
    {
        private const float speedFactor = .5f; // adjustment for hero speed to move around the world
        private UnitAnimation unitAnimation;
        private Hero hero;
        private bool isMoving;

        public Hero Hero => hero;

        [SerializeField] private Transform visual;

        private void Awake()
        {
            unitAnimation = GetComponentInChildren<UnitAnimation>();
        }

        public void SetHero(Hero hero)
        {
            this.hero = hero;
            unitAnimation.SetHero(hero);
        }

        internal void MoveTo(Vector3 targetPos)
        {
            if (!isMoving)
                StartCoroutine(MoveCoroutine(targetPos));
        }
        internal void Flip(bool flip)
        {
            var r = visual.transform.rotation;
            r.y = flip ? 180f : 0f;
            visual.transform.rotation = r;
        }

        private IEnumerator MoveCoroutine(Vector3 targetPos)
        {
            isMoving = true;
            var dir = (targetPos - transform.position).normalized;

            Flip(dir.x < 0);

            unitAnimation.Run(true);
            while (Vector3.SqrMagnitude(targetPos - transform.position) > .01f)
            {
                transform.position += Hero.Speed * speedFactor * Time.deltaTime * dir;

                yield return null;
            }
            unitAnimation.Run(false);
            isMoving = false;
        }
    }
}