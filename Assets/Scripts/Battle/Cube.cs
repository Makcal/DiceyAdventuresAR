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
    public class Cube : MonoBehaviour, IDragHandler
    {
        byte value;
        public byte Value 
        {
            get => value;
            set
            {
                if (value < 7)
                {
                    GetComponent<Image>().sprite = LevelGraph.levelGraph.battle.cubesSprites[value];
                    this.value = value;
                }
            }
        }
        [NonSerialized] public ActionCard card;

        public void OnDrag(PointerEventData eventData)
        {
            if (card == null && LevelGraph.levelGraph.battle.playerTurn)
            {
                Vector2 pos = eventData.position;
                GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        IEnumerator OnTriggerEnter2D(Collider2D collision)
        {
            if (Value != 0)
                yield break;
            var cube = collision.gameObject.GetComponent<Cube>();
            if (cube == null)
                yield break;

            bool accept;
            if (card.slots.Length == 1)
                accept = card.condition.Check(cube.Value);
            else
            {
                var otherCube = card.slots[1] == this ? card.slots[0] : card.slots[1];
                accept = card.condition.Check(cube.Value, otherCube.Value);
            }

            if (accept)
            {
                Value = cube.Value;

                BattleController battle = LevelGraph.levelGraph.battle;
                for (int i = 0; i < battle.cubes.Count; i++)
                    if (battle.cubes[i] == cube)
                        battle.cubes[i] = null;

                Destroy(cube.gameObject);
                if (transform.childCount != 0)
                    Destroy(transform.GetChild(0).gameObject); // delete condition description

                if (!battle.playerTurn)
                    yield return new WaitForSeconds(0.5f); // wait instead of anim [temp]
                else
                    yield return null; // wait for destroying cube

                card.TryAction();
                if (new List<Cube>(FindObjectsOfType<Cube>()).FindAll(c => c.card == null).Count == 0)
                    battle.turnEnded = true;
            }
        }

        static public Cube CreateCube(Transform parent, byte value = 0, ActionCard card = null)
        {
            Cube c = Instantiate(LevelGraph.levelGraph.battle.cubePrefab);

            var tr = (RectTransform)c.transform;
            tr.SetParent(parent);
            tr.anchorMin = tr.anchorMax = Vector2.zero;
            tr.anchoredPosition = Vector2.zero;

            c.Value = value;
            c.card = card;
            if (value == 0 && card.condition.type != (ConditionType.None | ConditionType.EvOd | ConditionType.Doubles))
            {
                var textObj = new GameObject("Condition");

                var textTr = textObj.AddComponent<RectTransform>();
                textTr.SetParent(tr);
                textTr.anchorMin = textTr.anchorMax = Vector2.one / 2;
                textTr.anchoredPosition = Vector2.zero;
                //textTr.sizeDelta = new Vector2(tr.sizeDelta.x, tr.sizeDelta.y * 2 / 3);

                var text = textObj.AddComponent<TextMeshProUGUI>();
                text.fontSize = 14;
                text.alignment = TextAlignmentOptions.Center;
                text.enableKerning = false;
            }
            return c;
        }

        void Start()
        {
            if (value == 0 && card.condition.type != (ConditionType.None | ConditionType.EvOd | ConditionType.Doubles))
            {
                var tr = (RectTransform)transform;
                ((RectTransform)tr.GetChild(0)).sizeDelta = new Vector2(tr.sizeDelta.x, tr.sizeDelta.y * 2 / 3);
            }
        }
    }
}
