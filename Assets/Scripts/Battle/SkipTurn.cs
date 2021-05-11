using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;

namespace DiceyAdventuresAR.Battle
{
    public class SkipTurn : MonoBehaviour
    {
        public void Skip()
        {
            LevelGraph.levelGraph.battle.turnEnded = true;
        }
    }
}
