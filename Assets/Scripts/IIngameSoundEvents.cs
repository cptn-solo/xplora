using System;

namespace Assets.Scripts
{
    public interface IIngameSoundEvents
    {
        event EventHandler OnPlayerAttack;
        event EventHandler OnPlayerDamaged;
        event EventHandler OnPlayerDamagedCritical;
        event EventHandler OnPlayerWalk;
        event EventHandler OnPlayerJump;
        event EventHandler OnCollectedItem;

        event EventHandler OnZombieAttack;
        event EventHandler OnZombieDamaged;
        event EventHandler OnZombieDamagedCritical;
        event EventHandler OnZombieKilled;

        event EventHandler OnChestOpen;
        event EventHandler OnMinigunShot;
        event EventHandler OnUziShot;
        event EventHandler OnOutOfAmmo;
        event EventHandler OnBombExplode;
    }
}