using UnityEngine;
using DiceyDungeonsAR.GameObjects.Players;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Enemies;

namespace DiceyDungeonsAR.GameObjects
{
    public class EnemyItem : Item
    {
        public override void UseByPlayer(Player player)
        {
            StartCoroutine(LevelGraph.levelGraph.StartBattle(GetComponent<Enemy>()));
        }
    }
}
