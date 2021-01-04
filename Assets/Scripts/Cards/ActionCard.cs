using System;
using UnityEngine;
using UnityEngine.UI;
using DiceyDungeonsAR.MyLevelGraph;
using TMPro;
using System.Collections.Generic;

namespace DiceyDungeonsAR.Battle
{
    public abstract class ActionCard : MonoBehaviour
    {
        public Cube[] slots;
        public bool slotsCount = false;
        public byte Uses
        {
            get
            {
                var tmpTr = (RectTransform)transform.GetChild(2);
                var text = tmpTr.GetComponent<TextMeshProUGUI>().text;
                return text != "" ? Byte.Parse(text.Replace(" осталось", "")) : (byte)1;
            }
            set
            {
                var tmpTr = (RectTransform)transform.GetChild(2);
                tmpTr.GetComponent<TextMeshProUGUI>().text = $"{value} осталось";
            }
        }
        public Color Color
        {
            get => GetComponent<Image>().color;
            set
            {
                GetComponent<Image>().color = value;
            }
        }
        bool size;
        public Condition condition;

        public abstract void DoAction();

        public byte GetSum()
        {
            return (byte)(slots[0].Value + (slotsCount ? slots[1].Value : 0));
        }

        public bool TryAction()
        {
            foreach (var c in slots)
                if (c.Value == 0)
                    return false;

            Uses--;
            DoAction();
            foreach (var c in slots)
                c.Value = 0;

            var battle = LevelGraph.levelGraph.battle;
            if (Uses == 0)
            {
                if (FindObjectsOfType(typeof(ActionCard)).Length == 1)
                    battle.turnEnded = true;

                Destroy(gameObject);
            }

            return true;
        }

        static T CreateCard<T>(bool size, bool slotsCount, Condition condition, byte uses, Color color, string text, string bonus) where T : ActionCard
        {
            RectTransform tr = Instantiate(LevelGraph.levelGraph.battle.cardPrefab);
            var canvasTr = GameObject.FindGameObjectWithTag("Canvas").transform;
            tr.SetParent(canvasTr);
            tr.sizeDelta = new Vector2(200, size ? 200 : 130);

            color /= 255;
            color.a = 1;
            tr.GetComponent<Image>().color = color;

            T card = tr.gameObject.AddComponent<T>();
            card.size = size;
            card.condition = condition;
            card.Uses = uses;
            card.slotsCount = slotsCount;

            card.slots = slotsCount ? new Cube[2] : new Cube[1];
            for (int i = 0; i < card.slots.Length; i++)
            {
                Cube cube = Cube.CreateCube(card.transform, card: card);

                var cubeTr = cube.GetComponent<RectTransform>();
                cubeTr.anchorMin = cubeTr.anchorMax = slotsCount ? new Vector2(0.25f + 0.5f * i, 0.5f) : new Vector2(0.5f, size ? 0.5f : 0.61f);
                cubeTr.anchoredPosition = Vector2.zero;

                card.slots[i] = cube;

                if(condition.type != (ConditionType.None | ConditionType.EvOd | ConditionType.Doubles))
                    cube.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = condition.GetDesc();
            }

            if (condition.type == ConditionType.Doubles && slotsCount)
            {
                var textObj = new GameObject("Condition");

                var textTr = textObj.AddComponent<RectTransform>();
                textTr.SetParent(tr);
                textTr.anchorMin = textTr.anchorMax = Vector2.one / 2;
                textTr.anchoredPosition = Vector2.zero;

                var doublesText = textObj.AddComponent<TextMeshProUGUI>();
                doublesText.text = "=";
                doublesText.fontSize = 50;
                doublesText.alignment = TextAlignmentOptions.Center;
                doublesText.enableKerning = false;
            }

            var width = 0.4f * ((RectTransform)canvasTr).sizeDelta.y / (size ? 1 : 1.54f);
            tr.localScale *= width / tr.sizeDelta.y;

            var tmpTr = (RectTransform)tr.GetChild(0);
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.31f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = text;

            tmpTr = (RectTransform)tr.GetChild(1);
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.23f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = bonus;

            tmpTr = (RectTransform)tr.GetChild(2);
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.15f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = uses > 1 ? $"{uses} осталось" : "";

            return card;
        }
        static public DamageCard CreateDamageCard(CardDescription description)
        {
            var card = CreateCard<DamageCard>(description.size, description.slotsCount, description.condition, description.uses, description.bonus.GetColor(), "Нанести <sprite index=0> урона", "");
            card.bonus = description.bonus;
            return card;
        }

        static public ChangeDiceCard CreateChangeDiceCard(byte uses = 3)
        {
            var diceCard = CreateCard<ChangeDiceCard>(true, false, Condition.TrueCond, uses, new Color(123, 123, 123), "Перебросить кубик", "");
            return diceCard;
        }
    }
}
