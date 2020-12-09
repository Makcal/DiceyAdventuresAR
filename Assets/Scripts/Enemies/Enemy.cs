using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Enemies
{
    abstract public class Enemy : MonoBehaviour
    {
        public void SetUpEnemy()
        {
            transform.parent = LevelGraph.levelGraph.transform;
            transform.localPosition = new Vector3(4, 0, 0);
            transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            transform.localScale *= 2;
        }
    }
}
