using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    public class DamageCard : ActionCard
    {
        public Bonus bonus;

        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;
            if (battle.playerTurn)
                battle.enemy.GetDamage(GetSum());
            else
                battle.player.GetDamage(GetSum());
        }
    }
}
