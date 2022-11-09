namespace Assets.Scripts.UI.Battle
{
    public enum BattleLine
    {
        NA,
        Front,  // avantgard
        Back,   // ariergard
    }
    public class BattleUnit
    {
        public Attack PriAttack;
        public Attack SecAttack;
        public Defence PriDefence;
        public Defence SecDefence;

        public BattleLine Line = BattleLine.Front;
        public override string ToString() =>
            $"Attack: {PriAttack?.IconName}, {SecAttack?.IconName}, Defence: {PriDefence?.IconName}, {SecDefence?.IconName}, Line: {Line}";
    }


}