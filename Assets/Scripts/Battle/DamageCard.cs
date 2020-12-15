using System;
using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    class DamageCard : ActionCard
    {
        public Bonus bonus;

        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;
            if (battle.playerTurn)
                battle.enemy.DealDamage(GetSum());
            else
                battle.player.DealDamage(GetSum());
        }
    }
}
