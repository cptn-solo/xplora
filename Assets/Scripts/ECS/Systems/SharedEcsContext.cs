namespace Assets.Scripts.ECS.Systems
{
    public class SharedEcsContext
    {
        public bool Paused { get; private set; }
        public void Pause(bool flag)
        {
            Paused = flag;
        }
    }
}
