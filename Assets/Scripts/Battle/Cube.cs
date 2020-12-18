using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DiceyDungeonsAR.MyLevelGraph;
using UnityEngine.UI;
using TMPro;

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
            if (card == null)
            {
                Vector2 pos = eventData.position;
                GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (Value != 0)
                return;
            var cube = collision.gameObject.GetComponent<Cube>();
            if (cube == null)
                return;

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

                int cubes = 0;
                foreach (var c in GameObject.FindObjectsOfType<Cube>())
                    if (c.card == null)
                        cubes++;
                if (cubes == 1)
                    LevelGraph.levelGraph.battle.turnEnded = true;

                for (int i = 0; i < LevelGraph.levelGraph.battle.cubes.Count; i++)
                    if (LevelGraph.levelGraph.battle.cubes[i] == cube)
                        LevelGraph.levelGraph.battle.cubes[i] = null;

                Destroy(cube.gameObject);
                if (transform.childCount != 0)
                    Destroy(transform.GetChild(0).gameObject);
                card.TryAction();
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
            if (value == 0 && card.condition.type != ConditionType.None && card.condition.type != ConditionType.EvOd)
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
            if (value == 0 && card.condition.type != ConditionType.None && card.condition.type != ConditionType.EvOd)
            {
                var tr = (RectTransform)transform;
                ((RectTransform)tr.GetChild(0)).sizeDelta = new Vector2(tr.sizeDelta.x, tr.sizeDelta.y * 2 / 3);
            }
        }
    }
}
