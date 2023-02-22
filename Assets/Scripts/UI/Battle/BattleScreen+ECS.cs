using Assets.Scripts.Battle;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen //ECS
    {
        private void BindEcsSlots()
        {
            var buffer = ListPool<IHeroPosition>.Get();

            buffer.AddRange(playerFrontSlots);
            buffer.AddRange(enemyFrontSlots);
            buffer.AddRange(playerBackSlots);
            buffer.AddRange(enemyBackSlots);

            //foreach (var slot in frontSlots)
            //{
            //    slot.SetHero(battleManager.HeroAtPosition(slot.Position), teamId == playerTeamId);
            //}

            //foreach (var slot in backSlots)
            //{
            //    slot.SetHero(battleManager.HeroAtPosition(slot.Position), teamId == playerTeamId);
            //}


            battleManager.BindEcsBattleScreenHeroSlots(buffer);

            ListPool<IHeroPosition>.Add(buffer);
        }
    }
}