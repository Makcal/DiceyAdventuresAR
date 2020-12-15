using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Enemies;
using DiceyDungeonsAR.GameObjects.Players;
using UnityEngine.UI;

namespace DiceyDungeonsAR.Battle
{
    public class BattleController : MonoBehaviour
    {
        [NonSerialized] public Enemy enemy;
        public Player player;
        bool battle = false;
        public bool playerTurn = true, turnEnded = false;

        public Sprite[] cubesSprites;
        public RectTransform cardPrefab;
        public Cube cubePrefab;

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
                var anchors = toPlayer ? new Vector2(0.4f, 0.1f) : new Vector2(0.6f, 0.9f);
                var xOffset = ((RectTransform)cubePrefab.transform).rect.width * 1.3f * i * (toPlayer ? 1 : -1);
                var pos = new Vector2(xOffset + canvasTr.rect.width * anchors.x, canvasTr.rect.height * anchors.y);

                var c = Cube.CreateCube(canvasTr, Vector2.zero, pos, (byte)(UnityEngine.Random.Range(0, 6) + 1));
                cubes.Add(c);
            }
            return cubes;
        }

        public IEnumerator StartBattle()
        {
            battle = true;
            var canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform;
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
        }

        void CreateCards(CardDescription[,] cards)
        {
            for (byte j = 0; j < 2; j++)
                for (byte i = 0; i < cards.GetUpperBound(0) + 1; i++)
                    if (cards[i, j] != null)
                    {
                        var card = ActionCard.CreateCard(cards[i, j]);

                        var tr = (RectTransform)card.transform;
                        var width = tr.rect.width;
                        var canvasWidth = ((RectTransform)tr.parent).rect.width;
                        tr.anchoredPosition = new Vector2(0.04f*canvasWidth + width*0.5f + (width + 0.04f*canvasWidth)*i, 0);
                    }
        }

        public void EndBattle(bool win)
        {
            battle = false;
            turnEnded = true;
            var message = AppearingAnim.CreateMsg("WinMessage", win ? "Ты победил!" : "Ты проиграл", 72);

            message.GetComponent<RectTransform>().sizeDelta = new Vector2(650, 200);

            message.yOffset = 20;
            message.color = win ? Color.green : Color.red;
            message.Play();

            if (win)
            {
                for (int i = 2; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true);

                player.transform.localScale /= 2;
                player.PlacePlayer(player.currentField, true);
            }
        }
    }
}
