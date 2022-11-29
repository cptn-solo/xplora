namespace Assets.Scripts.UI.Battle
{
    internal interface IHeroAnimation
    {
        void Attack();
        void Hit(bool lethal);
        void Death();
    }
}