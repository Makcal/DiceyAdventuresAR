using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Enemies;
using DiceyDungeonsAR.GameObjects.Players;
using UnityEngine.UI;
using DiceyDungeonsAR.UI;
using UnityEngine.SceneManagement;

namespace DiceyDungeonsAR.Battle
{
    public class BattleController : MonoBehaviour
    {
        [NonSerialized] public Enemy enemy;
        [NonSerialized] public Player player;
        [NonSerialized] public List<Cube> cubes;
        bool battle = false;
        public bool playerTurn = true, turnEnded = false;

        public Sprite[] cubesSprites;
        public RectTransform cardPrefab;
        public Cube cubePrefab;
        public SkipTurn skipButtonPrefab;
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

        public IEnumerator StartBattle()
        {
            battle = true;
            var canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform;

            SkipTurn button = Instantiate(skipButtonPrefab, canvasTr);

            enemy.healthBar = Bar.CreateBar(canvasTr, new Vector2(0.758f, 0.828f), new Vector2(0.930f, 0.883f));
            enemy.healthBar.maxValue = enemy.MaxHealth;
            enemy.healthBar.startValue = enemy.Health;
            Text enemyText = CreateText(canvasTr, new Vector2(0.758f, 0.883f), new Vector2(0.897f, 0.950f), enemy.Name);

            while (battle)
            {
                if (playerTurn)
                {
                    button.GetComponent<Button>().interactable = true;
                    CreateCards(player.Inventory);
                    cubes = CreateCubes(Mathf.Min(player.Level + 2, 5), true, canvasTr);

                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded);
                }
                else
                {
                    button.GetComponent<Button>().interactable = false;
                    CreateCards(enemy.Cards);
                    cubes = CreateCubes(enemy.Level == 6 ? 4 : 3, false, canvasTr);

                    var cor = StartCoroutine(EnemyTurn());
                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded);
                    StopCoroutine(cor);
                }


                turnEnded = false;
                playerTurn = !playerTurn;

                foreach (var c in FindObjectsOfType<Cube>())
                    if (c.card == null)
                        Destroy(c.gameObject);
                foreach (var im in FindObjectsOfType<Image>())
                    if (im.GetComponent<ActionCard>() != null)
                        Destroy(im.gameObject);

                yield return null;
            }

            Destroy(enemy.healthBar.gameObject);
            Destroy(enemyText);
            Destroy(button.gameObject);
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

        void CreateCards(CardDescription[,] cards)
        {
            for (byte j = 0; j < 2; j++)
                for (byte i = 0; i < cards.GetUpperBound(0) + 1; i++)
                    if (cards[i, j] != null)
                    {
                        ActionCard card;
                        switch (cards[i, j].action) {
                            case CardAction.Damage:
                                card = ActionCard.CreateDamageCard(cards[i, j]);
                                break;
                            case CardAction.ChangeDice:
                                card = ActionCard.CreateChangeDiceCard(cards[i, j].uses);
                                break;
                            case CardAction.DoubleDamage:
                                card = ActionCard.CreateDoubleDamageCard(cards[i, j]);
                                break;
                            default:
                                throw new NotImplementedException($"{cards[i, j].action} action hasn't been implemented yet");
                        }

                        var tr = (RectTransform)card.transform;
                        var width = tr.rect.width * tr.localScale.x;
                        var canvasWidth = ((RectTransform)tr.parent).sizeDelta.x;
                        tr.anchoredPosition = new Vector2(0.04f*canvasWidth + width*0.5f + (width + 0.04f*canvasWidth)*i, 0);
                    }
        }

        static public Text CreateText(Transform canvasTr, Vector2 anchorMin, Vector2 anchorMax, string text)
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

            var message = AppearingAnim.CreateMsg("WinMessage", new Vector2(0.29f, 0.37f), new Vector2(0.7f, 0.68f), win ? "Ты победил!" : "Ты проиграл");

            message.yOffset = 50;
            message.color = win ? Color.green : Color.red;
            message.period = 2;
            message.Play();

            yield return new WaitForSeconds(2);

            if (win)
            {
                for (int i = 2; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true);

                player.transform.localScale /= 2;
                player.transform.position = player.currentField.transform.position + new Vector3(0, player.currentField.transform.localScale.y, 0);
                StartCoroutine(player.AddXP(enemy.Level));

                if (player.Inventory[2, 0] == null)
                    player.Inventory[2, 0] = new CardDescription()
                    {
                        action = CardAction.DoubleDamage,
                        condition = new Condition() { type = ConditionType.Doubles },
                        slotsCount = true,
                        bonus = new Bonus() { type = BonusType.Thorns },
                    };
            }
            else
            {
                yield return new WaitForSeconds(1);
                SceneManager.LoadScene(MenuHandler.mainMenuScene_);
            }

            ResetBattle();
        }

        void ResetBattle()
        {
            enemy = null;
            battle = false;
            playerTurn = true;
            turnEnded = false;
            cubes = null;
        }

        IEnumerator EnemyTurn()
        {
            var cards = new List<ActionCard>(FindObjectsOfType<ActionCard>());
            cards.Sort((c1, c2) => c1.condition.GetPriority() - c2.condition.GetPriority());

            yield return new WaitForSeconds(0.75f);

            foreach (var card in cards)
            {
                var cubes = new List<Cube>(FindObjectsOfType<Cube>());
                cubes = GetActiveCubes(cubes);
                var suitableCubes = new List<Cube>();

                if (card.condition.type == ConditionType.Doubles && card.slotsCount)
                {
                    for (byte i = 6; i > 0; i--)
                        if (cubes.FindAll(c => c.Value == i).Count >= 2)
                            suitableCubes.AddRange(cubes.FindAll(c => c.Value == i).GetRange(0, 2));
                    if (suitableCubes.Count == 0)
                        continue;
                }
                else if (card.slotsCount)
                {
                    if (cubes.Count >= 2)
                        suitableCubes.AddRange(cubes.GetRange(0, 2));
                    else
                        continue;
                }
                else
                {
                    var cube = cubes.Find(c => card.condition.Check(c.Value));
                    if (cube == null)
                        continue;
                    suitableCubes.Add(cube);
                }

                List<Vector3> startPoses = suitableCubes.ConvertAll<Vector3>(c => c.transform.position);
                for (var i = 0; i < suitableCubes.Count; i++)
                {
                    float startTime = Time.time;
                    while (suitableCubes[i])
                    {
                        if (suitableCubes[i] && card)
                            suitableCubes[i].transform.position = Vector3.Lerp(
                                startPoses[i],
                                new List<Cube>(card.slots).FindAll(c => c.Value == 0)[0].transform.position,
                                (Time.time - startTime) / 1.0f
                            );
                        yield return null;
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(0.75f);
            turnEnded = true;
        }

        List<Cube> GetActiveCubes(List<Cube> cubes)
        {
            cubes = new List<Cube>(cubes);
            cubes.RemoveAll(c => c.card != null || c == null);
            cubes.Sort((c1, c2) => c2.Value - c1.Value);
            return cubes;
        }
    }
}
