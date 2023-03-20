using System;

namespace Assets.Scripts.Data
{
    public class HeroPosition : Tuple<int, BattleLine, int>
    {
        public HeroPosition(int teamId, BattleLine battleLine, int slotIndex) :
            base(teamId, battleLine, slotIndex)
        {
        }

        public int Team => Item1;
        public BattleLine Line => Item2;
        public int Slot => Item3;

        public override bool Equals(object obj)
        {
            return obj is HeroPosition target &&
                target.Team.Equals(Team) &&
                target.Line.Equals(Line) &&
                target.Slot.Equals(Slot);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Team, Line, Slot);
        }

        public override string ToString()
        {
            return $"{Team}-{Line}-{Slot}";
        }
    }
}