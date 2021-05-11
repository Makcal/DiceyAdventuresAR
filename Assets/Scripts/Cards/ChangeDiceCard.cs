using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;

namespace DiceyAdventuresAR.Battle
{
    public class ChangeDiceCard : ActionCard
    {
        public override void DoAction()
        {
            var battle = LevelGraph.levelGraph.battle;

            for (byte i = 0; i < battle.cubes.Count; i++)
                if (battle.cubes[i] == null) // найти первый пробел в списке кубиков
                {
                    var cube = Cube.CreateCube((byte)(Random.Range(0, 6) + 1)); // создать один кубик
                    var tr = (RectTransform)cube.transform; // трансформ

                    var xOffset = tr.localScale.x * (tr.rect.width * 1.3f) * i;
                    var pos = new Vector2(xOffset + battle.canvasTr.sizeDelta.x * 0.4f, battle.canvasTr.sizeDelta.y * 0.1f);
                    tr.anchoredPosition = pos; // вставить кубик на нужное место по формулам из BattleController

                    battle.cubes[i] = cube; // заполнить пробел
                    break;
                }
        }
    }
}
