using DiceyAdventuresAR.GameObjects.Players;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.Enemies;

namespace DiceyAdventuresAR.GameObjects
{
    public class EnemyItem : Item
    {
        public override void UseByPlayer(Player player)
        {
            StartCoroutine(LevelGraph.levelGraph.StartBattle(GetComponent<Enemy>()));
        }
    }
}
