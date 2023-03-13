namespace Assets.Scripts.UI.Data
{
    public enum Actions
    {
        SelectHero = 1,
        SelectQueueMember = 2,
        CompleteTurn = 100,
        BeginBattle = 101,
        PrepareQueue = 102,
        BeginRound = 150,
        RetreatBattle = 200,
        AutoBattle = 300,
        StepBattle = 350, // when autoplaying can stop and let the user make a turn
        ReloadMetadata = 400,
        SaveTeamForBattle = 500,
        ToggleLogPanel = 600,
    }

}