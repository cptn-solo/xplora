namespace Assets.Scripts.UI.Battle
{
    internal interface IHeroAnimation
    {
        void Attack(bool range = false);
        void Hit(bool lethal);
        void Death();
    }
}