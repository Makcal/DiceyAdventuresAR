using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    public class DoubleDamageCard : ActionCard
    {
        public Bonus bonus;

        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;
            if (battle.playerTurn)
                battle.enemy.DealDamage(GetSum() * 2);
            else
                battle.player.DealDamage(GetSum() * 2);
        }
    }
}
