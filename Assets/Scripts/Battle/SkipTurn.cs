using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    public class SkipTurn : MonoBehaviour
    {
        public void Skip()
        {
            LevelGraph.levelGraph.battle.turnEnded = true;
        }
    }
}
