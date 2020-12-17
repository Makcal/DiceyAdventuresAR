using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Enemies;
using DiceyDungeonsAR.GameObjects.Players;
using UnityEngine.UI;
using DiceyDungeonsAR.UI;

namespace DiceyDungeonsAR.Battle
{
    public class BattleController : MonoBehaviour
    {
        [NonSerialized] public Enemy enemy;
        [NonSerialized] public Player player;
        [NonSerialized] public Bar enemyBar;
        bool battle = false;
        public bool playerTurn = true, turnEnded = false;

        public Sprite[] cubesSprites;
        public RectTransform cardPrefab;
        public Cube cubePrefab;
        public SkipTurn skipButton;
        public Bar barPrefab;

        public void SetUpOpponents(Enemy enemy)
        {
            this.enemy = enemy;
            enemy.transform.parent = LevelGraph.levelGraph.transform;
            enemy.transform.localPosition = new Vector3(4, 0, 0);
            enemy.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            enemy.transform.localScale *= 2;

            player = LevelGraph.levelGraph.player;
            player.transform.localPosition = new Vector3(-4, 0, 0);
            player.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            player.transform.localScale *= 2;
        }

        public List<Cube> CreateCubes(int count, bool toPlayer, RectTransform canvasTr)
        {
            var cubes = new List<Cube>();
            for (int i = 0; i < count; i++)
            {
                var c = Cube.CreateCube(canvasTr, (byte)(UnityEngine.Random.Range(0, 6) + 1));

                var tr = c.GetComponent<RectTransform>();
                tr.localScale *= 0.12f * canvasTr.sizeDelta.y / tr.sizeDelta.y;
                var anchors = toPlayer ? new Vector2(0.4f, 0.1f) : new Vector2(0.6f, 0.9f);
                var xOffset = tr.sizeDelta.x * tr.localScale.x * 1.3f * i * (toPlayer ? 1 : -1);
                var pos = new Vector2(xOffset + canvasTr.sizeDelta.x * anchors.x, canvasTr.sizeDelta.y * anchors.y);
                tr.anchoredPosition = pos;

                cubes.Add(c);
            }
            return cubes;
        }

        public IEnumerator StartBattle()
        {
            battle = true;
            var canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform;

            SkipTurn button = Instantiate(skipButton, canvasTr);

            enemyBar = Bar.CreateBar(canvasTr, new Vector2(0.758f, 0.828f), new Vector2(0.930f, 0.883f));
            enemyBar.maxValue = enemy.MaxHealth;
            enemyBar.startValue = enemy.Health;

            Text[] texts = CreateTexts(canvasTr);

            while (battle)
            {
                if (playerTurn)
                {
                    CreateCards(player.Inventory);
                    CreateCubes(Mathf.Min(player.Level + 2, 5), true, canvasTr);
                }
                else
                {
                    CreateCards(enemy.Cards);
                    CreateCubes(Mathf.Min(enemy.Level + 2, 5), false, canvasTr);
                }

                yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded);

                turnEnded = false;
                playerTurn = !playerTurn;

                foreach (var c in GameObject.FindObjectsOfType<Cube>())
                    if (c.card == null)
                        Destroy(c.gameObject);
                foreach (var im in GameObject.FindObjectsOfType<Image>())
                    if (im.GetComponent<ActionCard>() != null)
                        Destroy(im.gameObject);
            }

            Destroy(enemyBar.gameObject);
            foreach (var t in texts)
                Destroy(t.gameObject);
            Destroy(button.gameObject);
        }

        void CreateCards(CardDescription[,] cards)
        {
            for (byte j = 0; j < 2; j++)
                for (byte i = 0; i < cards.GetUpperBound(0) + 1; i++)
                    if (cards[i, j] != null)
                    {
                        var card = ActionCard.CreateCard(cards[i, j]);

                        var tr = (RectTransform)card.transform;
                        var width = tr.rect.width * tr.localScale.x;
                        var canvasWidth = ((RectTransform)tr.parent).sizeDelta.x;
                        tr.anchoredPosition = new Vector2(0.04f*canvasWidth + width*0.5f + (width + 0.04f*canvasWidth)*i, 0);
                    }
        }

        Text[] CreateTexts(Transform canvasTr)
        {
            var texts = new Text[2];
            texts[0] = CreateText(canvasTr, new Vector2(0.068f, 0.185f), new Vector2(0.177f, 0.251f), "Ты:");
            texts[1] = CreateText(canvasTr, new Vector2(0.758f, 0.883f), new Vector2(0.897f, 0.950f), enemy.Name);
            return texts;
        }

        Text CreateText(Transform canvasTr, Vector2 anchorMin, Vector2 anchorMax, string text)
        {
            var obj = new GameObject(text, typeof(Outline));
            var tr = obj.AddComponent<RectTransform>();
            tr.SetParent(canvasTr);
            tr.anchorMin = anchorMin;
            tr.anchorMax = anchorMax;
            tr.anchoredPosition = Vector2.zero;
            tr.sizeDelta = anchorMax - anchorMin;

            var textComp = obj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComp.alignment = TextAnchor.LowerLeft;
            textComp.resizeTextForBestFit = true;
            textComp.resizeTextMaxSize = 300;
            textComp.resizeTextMinSize = 2;

            return textComp;
        }

        public IEnumerator EndBattle(bool win)
        {
            battle = false;
            turnEnded = true;

            var message = AppearingAnim.CreateMsg("WinMessage", win ? "Ты победил!" : "Ты проиграл", 72);
            message.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 200);

            message.yOffset = 20;
            message.color = win ? Color.green : Color.red;
            message.period = 2;
            message.Play();

            yield return new WaitForSeconds(2);

            if (win)
            {
                for (int i = 2; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true);

                player.transform.localScale /= 2;
                player.PlacePlayer(player.currentField, true);
            }

            ResetBattle();
        }

        void ResetBattle()
        {
            enemy = null;
            battle = false;
            playerTurn = true;
            turnEnded = false;
        }
    }
}
