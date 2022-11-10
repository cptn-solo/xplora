using Assets.Scripts.UI.Data;

namespace Assets.Scripts.UI.Battle
{
    public class BattleUnit
    {
        public BattleLine Line = BattleLine.Front;
        public Hero Hero;
        public override string ToString() =>
            $"Attack: {Hero.Attack[0].IconName}, {Hero.Attack[1].IconName}, Defence: {Hero.Defence[0].IconName}, {Hero.Defence[0].IconName}, Line: {Line}";
    }


}