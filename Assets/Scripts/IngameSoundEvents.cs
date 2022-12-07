using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class IngameSoundEvents : MonoBehaviour, IIngameSoundEvents
    {
        public event EventHandler OnPlayerAttack;
        public event EventHandler OnPlayerDamaged;
        public event EventHandler OnPlayerDamagedCritical;
        public event EventHandler OnPlayerWalk;
        public event EventHandler OnPlayerJump;
        public event EventHandler OnSpiderBeamAttack;
        public event EventHandler OnZombieAttack;
        public event EventHandler OnZombieDamaged;
        public event EventHandler OnZombieDamagedCritical;
        public event EventHandler OnZombieKilled;

        public event EventHandler OnCollectedItem;
        public event EventHandler OnChestOpen;
        public event EventHandler OnMinigunShot;
        public event EventHandler OnOutOfAmmo;
        public event EventHandler OnBombExplode;
        public event EventHandler OnUziShot;

        public void CollectedItem() => OnCollectedItem?.Invoke(this, null);
        public void ChestOpen() => OnChestOpen?.Invoke(this, null);
        public void PlayerAttack() => OnPlayerAttack?.Invoke(this, null);
        public void PlayerDamaged() => OnPlayerDamaged?.Invoke(this, null);
        public void PlayerDamagedCritical() => OnPlayerDamagedCritical?.Invoke(this, null);
        public void PlayerWalk() => OnPlayerWalk?.Invoke(this, null);
        public void PlayerJump() => OnPlayerJump?.Invoke(this, null);
        public void SpiderBeamAttack() => OnSpiderBeamAttack?.Invoke(this, null);
        public void ZombieAttack() => OnZombieAttack?.Invoke(this, null);
        public void ZombieDamaged() => OnZombieDamaged?.Invoke(this, null);
        public void ZombieDamagedCritical() => OnZombieDamagedCritical?.Invoke(this, null);
        public void ZombieKilled() => OnZombieKilled?.Invoke(this, null);
        public void MinigunShot() => OnMinigunShot?.Invoke(this, null);
        public void UziShot() => OnUziShot?.Invoke(this, null);
        public void OutOfAmmo() => OnOutOfAmmo?.Invoke(this, null);
        public void BombExplode() => OnBombExplode?.Invoke(this, null);
    }
}