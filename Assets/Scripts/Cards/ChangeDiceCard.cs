using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;

namespace DiceyDungeonsAR.Battle
{
    public class ChangeDiceCard : ActionCard
    {
        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;

            for (byte i = 0; i < battle.cubes.Count; i++)
                if (battle.cubes[i] == null)
                {
                    var canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform;
                    var c = Cube.CreateCube(canvasTr, (byte)(Random.Range(0, 6) + 1));

                    var tr = c.GetComponent<RectTransform>();
                    tr.localScale *= 0.12f * canvasTr.sizeDelta.y / tr.sizeDelta.y;
                    var anchors = new Vector2(0.4f, 0.1f);
                    var xOffset = tr.sizeDelta.x * tr.localScale.x * 1.3f * i;
                    var pos = new Vector2(xOffset + canvasTr.sizeDelta.x * anchors.x, canvasTr.sizeDelta.y * anchors.y);
                    tr.anchoredPosition = pos;

                    battle.cubes[i] = c;
                    break;
                }
        }
    }
}
