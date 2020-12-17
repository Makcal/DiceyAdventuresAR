﻿using System;
using UnityEngine;
using UnityEngine.UI;
using DiceyDungeonsAR.MyLevelGraph;
using TMPro;

namespace DiceyDungeonsAR.Battle
{
    //public delegate bool Condition(int value);

    public abstract class ActionCard : MonoBehaviour
    {
        public Cube[] slots;
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
            byte sum = 0;
            foreach (var c in slots)
                sum += c.Value;
            return sum;
        }

        public bool TryAction()
        {
            foreach (var c in slots)
                if (c.Value == 0)
                    return false;

            DoAction();
            if (GameObject.FindObjectsOfType(typeof(ActionCard)).Length == 1)
                LevelGraph.levelGraph.battle.turnEnded = true;
            Destroy(gameObject);
            return true;
        }

        static T CreateCard<T>(bool size, bool slotsCount, Condition condition, Color color, string text, string bonus) where T : ActionCard
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

            if (slotsCount)
            {
                card.slots = new Cube[2];
                for (int i = 0; i < 2; i++)
                {
                    Cube cube = Cube.CreateCube(card.transform, card: card);

                    var cubeTr = cube.GetComponent<RectTransform>();
                    cubeTr.anchorMin = cubeTr.anchorMax = new Vector2(0.25f + 0.5f * i, 0.5f);
                    cubeTr.anchoredPosition = Vector2.zero;

                    card.slots[i] = cube;

                    if (condition.type != ConditionType.None && condition.type != ConditionType.EvOd)
                        cube.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = condition.GetDesc();
                }
            }
            else
            {
                card.slots = new Cube[1];

                Cube cube = Cube.CreateCube(card.transform, card: card);

                var cubeTr = cube.GetComponent<RectTransform>();
                cubeTr.anchorMin = cubeTr.anchorMax = new Vector2(0.5f, size ? 0.5f : 0.61f);
                cubeTr.anchoredPosition = Vector2.zero;

                card.slots[0] = cube;

                if (condition.type != ConditionType.None && condition.type != ConditionType.EvOd)
                    cube.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = condition.GetDesc();
            }

            var width = 0.4f * ((RectTransform)canvasTr).sizeDelta.y / (size ? 1 : 1.54f);
            tr.localScale *= width / tr.sizeDelta.y;

            var tmpTr = (RectTransform)tr.GetChild(0);
            tmpTr.sizeDelta = new Vector2(tr.sizeDelta.x, 0.2f * tr.sizeDelta.y);
            if (size)
                tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.3f) ;

            tmpTr.GetComponent<TextMeshProUGUI>().text = text + "\n" + bonus;

            return card;
        }
        static public ActionCard CreateCard(CardDescription description)
        {
            switch (description.action)
            {
                case CardAction.Damage:
                    var card = CreateCard<DamageCard>(description.size, description.slotsCount, description.condition, description.bonus.GetColor(), "Нанести <sprite index=0> урона", "");
                    card.bonus = description.bonus;
                    return card;
                default:
                    return null;
            }
        }
    }
}