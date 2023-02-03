namespace Assets.Scripts.Data
{
    public enum RaidState
    {
        NA = -1,
        AwaitingUnits = 10,
        //UnitsBeingSpawned = 100,
        UnitsSpawned = 200,
        //AwaitingUnitsDestruction = 300,
        //UnitsBeingDestroyed = 400,
        //PrepareBattle = 600,
        InBattle = 700,
        Aftermath = 800,
    }
}