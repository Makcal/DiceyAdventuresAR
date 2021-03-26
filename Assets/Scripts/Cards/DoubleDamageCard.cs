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
                battle.enemy.GetDamage(GetSum() * 2);
            else
                battle.player.GetDamage(GetSum() * 2);
        }
    }
}
