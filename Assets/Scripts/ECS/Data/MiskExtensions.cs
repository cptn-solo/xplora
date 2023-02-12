using Assets.Scripts.UI.Data;

namespace Assets.Scripts.ECS.Data
{
    public static class MiskExtensions
    {
        public static Hero HeroBestBySpeed(this Hero[] heroes)
        {
            var speed = 0;
            var idx = -1;
            for (int i = 0; i < heroes.Length; i++)
                if (heroes[i].Speed is int s && s > speed)
                {
                    idx = i;
                    speed = s;
                }

            return heroes[idx];
        }
    }
}


