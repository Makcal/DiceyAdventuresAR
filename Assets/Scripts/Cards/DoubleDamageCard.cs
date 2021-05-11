using DiceyAdventuresAR.MyLevelGraph;

namespace DiceyAdventuresAR.Battle
{
    public class DoubleDamageCard : ActionCard
    {
        public Bonus bonus; // есть бонус

        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;
            if (battle.playerTurn) // нанести двойной урон
                battle.enemy.GetDamage(GetSum() * 2);
            else
                battle.player.GetDamage(GetSum() * 2);
        }
    }
}
