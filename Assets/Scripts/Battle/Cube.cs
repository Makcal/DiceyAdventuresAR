using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DiceyDungeonsAR.MyLevelGraph;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace DiceyDungeonsAR.Battle
{
    public class Cube : MonoBehaviour, IDragHandler // интерфейс для перетаскивания
    {
        [NonSerialized] public ActionCard card; // карточка, которой принадлежит кубик

        byte value;
        public byte Value // значение кубика (0 - пустой слот)
        {
            get => value;
            set
            {
                if (value < 7) // поменять картинку
                {
                    GetComponent<Image>().sprite = LevelGraph.levelGraph.battle.cubesSprites[value];
                    this.value = value;
                }
            }
        }

        public void OnDrag(PointerEventData eventData) // перетаскивание кубика
        {
            if (card == null && LevelGraph.levelGraph.battle.playerTurn) // можно тащить, только если нет карточки и ход игрока
            {
                Vector2 pos = eventData.position; // где курсор/касание
                GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        IEnumerator OnTriggerEnter2D(Collider2D collision) // кубик соприкасается с другим
        {
            if (Value != 0) // если этот кубик не пустой, то не обрабатываем (обрабатываем только от лица слота)
                yield break;
            var cube = collision.gameObject.GetComponent<Cube>();
            if (cube == null) // конец, если коснулись не кубика
                yield break;

            bool accept; // принять второй кубик?
            if (card.slots.Length == 1) // если у карточки 1 слот, то проверим кубик на условие
                accept = card.condition.Check(cube.Value);
            else
            {
                // взять противоположный слот (возможно, уже со значением)
                var otherCube = card.slots[1] == this ? card.slots[0] : card.slots[1];
                accept = card.condition.Check(cube.Value, otherCube.Value); // проверка обоих слотов
            }

            if (accept) // если приняли
            {
                Value = cube.Value; // пустой кубик (этот) получает значение

                BattleController battle = LevelGraph.levelGraph.battle;

                for (int i = 0; i < battle.cubes.Count; i++)
                    if (battle.cubes[i] == cube)
                        battle.cubes[i] = null; // найти использованный кубик в списке и заменить его на null
                Destroy(cube.gameObject); // уничтожить использованный кубик

                if (transform.childCount != 0)
                    Destroy(transform.GetChild(0).gameObject); // уничтожить описание условия

                if (!battle.playerTurn)
                    yield return new WaitForSeconds(0.5f); // подождать, чтобы карточка не исчезла сразу (временно, вместо анимации)
                else
                    yield return null; // подождать уничтожения кубика

                card.TryToDoAction(); // попробовать выполнить действие
                if (battle.cubes.FindAll(c => c != null).Count == 0)
                    battle.turnEnded = true; // закончить ход, если не осталось кубиков
            }
        }

        public static Cube CreateCube(byte value) // свободный кубик
        {
            return CreateCube(LevelGraph.levelGraph.battle.canvasTr, value, null);
        }

        public static Cube CreateCube(Transform parent, ActionCard card) // слот для карточки
        {
            return CreateCube(parent, 0, card);
        }

        static Cube CreateCube(Transform parent, byte value, ActionCard card) // создать кубик
        {
            BattleController battle = LevelGraph.levelGraph.battle;

            Cube c = Instantiate(battle.cubePrefab);

            var tr = (RectTransform)c.transform; // трансформ 2d графики
            tr.SetParent(parent);
            tr.anchorMin = tr.anchorMax = Vector2.zero; // по умольчанию кубик стоит в (0; 0)
            tr.anchoredPosition = Vector2.zero;

            var width = 0.12f * battle.canvasTr.sizeDelta.y; // теоритическая ширина кубика - 12% высоты канваса
            tr.localScale *= width / tr.sizeDelta.y; // нужный масштаб для кубика

            c.Value = value;
            c.card = card; // карточка, которой принадлежит кубик
            if (value == 0 && card?.condition.type != (ConditionType.None | ConditionType.EvOd | ConditionType.Doubles))
            {
                // если число - 0 (слот карточки), и у карточки есть обычное условие, то добавить объект для текста
                var textObj = new GameObject("Condition");

                var textTr = textObj.AddComponent<RectTransform>();
                textTr.SetParent(tr);
                textTr.anchorMin = Vector2.zero;
                textTr.anchorMax = Vector2.one; // якоря в углах кубика
                textTr.offsetMin = textTr.offsetMax = Vector2.zero; // нет отступа (вся площадь кубика)
                textTr.anchoredPosition = Vector2.zero;

                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 14;
                text.alignment = TextAlignmentOptions.Center; // по центру
                text.enableKerning = false; // вредный параметр
            }
            return c;
        }
    }
}
