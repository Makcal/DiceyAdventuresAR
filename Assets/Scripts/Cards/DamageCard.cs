using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    public class DamageCard : ActionCard
    {
        public Bonus bonus; // есть бонус

        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;
            if (battle.playerTurn) // нанести урон
                battle.enemy.GetDamage(GetSum());
            else
                battle.player.GetDamage(GetSum());
        }
    }
}
