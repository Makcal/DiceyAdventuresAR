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
        RectTransform canvasTr; // быстрая ссылка на трансформ канваса

        [NonSerialized] public Enemy enemy; // враг
        [NonSerialized] public Player player; // игрок

        [NonSerialized] public List<Cube> cubes; // кубики

        bool battle = false; // идёт ли битва
        [NonSerialized] public bool playerTurn = true; // ход игрока?
        [NonSerialized] public bool turnEnded = false; // текущий ход окончен?

        // для Юнити
        public Sprite[] cubesSprites; // спрайты кубиков
        public RectTransform cardPrefab; // префаб карточки
        public Cube cubePrefab; // префаб кубика
        public SkipTurn skipButtonPrefab; // префаб кнопки пропуска хода
        public Bar barPrefab; // префаб полоски для скрипта Bar.cs

        // методы
        public void SetUpOpponents(Enemy enemy) // подготовка
        {
            this.enemy = enemy; // ссылка на врага
            enemy.transform.parent = LevelGraph.levelGraph.transform;
            enemy.transform.localPosition = new Vector3(1.6f, 0, 0);
            enemy.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up); // смотреть на игрока
            enemy.transform.localScale *= 2;

            player = LevelGraph.levelGraph.player; // ссылка на игрока
            player.transform.localPosition = new Vector3(-1.6f, 0, 0);
            player.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up); // смотреть на врага
            player.transform.localScale *= 2; // сделать большими
        }

        public IEnumerator StartBattle()
        {
            battle = true; // битва началась
            canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform; // трансформ канваса для UI

            SkipTurn button = Instantiate(skipButtonPrefab, canvasTr); // создать кнопку пропуска

            enemy.Initialize(); // инициализация врага

            while (battle) // пока идёт битва
            {
                if (playerTurn) // ход игрока
                {
                    button.GetComponent<Button>().interactable = true; // можно нажать на кнопку

                    CreateCards(player.inventory); // создаём карточки из инвентаря игрока
                    cubes = CreateCubes(Mathf.Min(player.Level + 2, 5), true); // кубики для игрока по формуле, но не больше 5

                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded); // ждать окончания хода
                }
                else // ход врага
                {
                    button.GetComponent<Button>().interactable = false; // нельзя нажать на кнопку (не твой ход)

                    CreateCards(enemy.inventory); // создаём карточки из инвентаря врага
                    cubes = CreateCubes(enemy.CubesCount, false); // кубики для врага

                    var cor = StartCoroutine(EnemyTurn()); // начать ход врага
                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded); // ждать окончания хода
                    StopCoroutine(cor); // принудительно закончить ход врага
                }

                turnEnded = false; // сбрасываем переменную
                playerTurn = !playerTurn; // смена хода

                foreach (var с in FindObjectsOfType<ActionCard>())
                    Destroy(с.gameObject); // уничтожить все карточки
                foreach (var c in FindObjectsOfType<Cube>())
                    Destroy(c.gameObject); // уничтожить все кубики

                yield return null;
            }

            Destroy(button.gameObject); // уничтожить кнопку
        }

        public List<Cube> CreateCubes(int count, bool toPlayer) // создать n кубиков
        {
            var cubes = new List<Cube>();
            for (int i = 0; i < count; i++)
            {
                var c = Cube.CreateCube(canvasTr, (byte)(UnityEngine.Random.Range(0, 6) + 1)); // создать кубик со случайным числом

                var tr = c.GetComponent<RectTransform>(); // трансформ 2d графики
                // во сколько раз 0.12 от высоты экрана (кубик по формуле) больше стандартной высоты куба
                tr.localScale *= (0.12f * canvasTr.sizeDelta.y) / tr.sizeDelta.y;

                var anchors = toPlayer ? new Vector2(0.4f, 0.1f) : new Vector2(0.6f, 0.9f); // якорь (точка отсчёта координат)
                var xOffset = tr.localScale.x * (tr.rect.width * 1.3f) * i * (toPlayer ? 1 : -1); // сдвиг кубика от якоря
                // (tr.rect.width * 1.3f) - ширина кубика с небольшим пробелом, i раз отступ, localScale - множитель размера
                // у врага порядок справа налево
                var pos = new Vector2(xOffset + canvasTr.sizeDelta.x * anchors.x, canvasTr.sizeDelta.y * anchors.y);
                tr.anchoredPosition = pos;
                // устанавливаем позицию кубика относительно якорей на канвасе плюс отступ

                cubes.Add(c);
            }
            return cubes;
        }

        void CreateCards(CardDescription[,] cards) // создать карточки из массива описаний
        {
            for (byte j = 0; j < 2; j++) // 0 - основной инвентарь, 1 - ряд маленьких карточек (если на 0 стоит тоже маленькая)
                for (byte i = 0; i < cards.GetUpperBound(0) + 1; i++) // проход по ряду
                    if (cards[i, j] != null) // есть описание
                    {
                        ActionCard card;
                        switch (cards[i, j].action) // зависит от действия карточки
                        {
                            case CardAction.Damage:
                                card = ActionCard.CreateDamageCard(cards[i, j]); // урон
                                break;
                            case CardAction.ChangeDice:
                                card = ActionCard.CreateChangeDiceCard(cards[i, j].uses); // перебросить
                                break;
                            case CardAction.DoubleDamage:
                                card = ActionCard.CreateDoubleDamageCard(cards[i, j]); // двойной урон
                                break;
                            default: // не реализовано
                                throw new NotImplementedException($"{cards[i, j].action} action hasn't been implemented yet");
                        }

                        var tr = (RectTransform)card.transform; // трансформ карточки
                        var width = tr.rect.width * tr.localScale.x; // ширина карточки с учётом масштаба
                        var canvasWidth = ((RectTransform)tr.parent).sizeDelta.x; // ширина экрана
                        // сдвиг от якоря (карточки в ряд)
                        // width + 0.04f*canvasWidth - ширина карточки плюс расстояние между карточками, i раз отступ (для i-ого места)
                        // 0.04f*canvasWidth + width*0.5f - общий сдвиг от края экрана
                        tr.anchoredPosition = new Vector2(0.04f*canvasWidth + width*0.5f + (width + 0.04f*canvasWidth)*i, 0);
                    }
        }

        public IEnumerator EndBattle(bool win) // конец битвы
        {
            battle = false;
            turnEnded = true;

            // всплывающее сообщение
            var message = AppearingAnim.CreateMsg("WinMessage", new Vector2(0.29f, 0.37f), new Vector2(0.7f, 0.68f), win ? "Ты победил!" : "Ты проиграл");

            message.yOffset = 50;
            message.color = win ? Color.green : Color.red;
            message.period = 2;
            message.Play();

            yield return new WaitForSeconds(2); // пауза

            if (win)
            {
                for (int i = 2; i < transform.childCount; i++)
                    transform.GetChild(i).gameObject.SetActive(true); // включаем поля и рёбра уровня

                player.transform.localScale /= 2; // возврат игрока на место
                player.transform.position = player.currentField.transform.position + new Vector3(0, player.currentField.transform.localScale.y, 0);
                StartCoroutine(player.AddXP(enemy.Level)); // дать опыт
            }
            else
            {
                yield return new WaitForSeconds(1);
                SceneManager.LoadScene(MenuHandler.mainMenuScene_); // меню
            }

            ResetBattle(); // сброс настроек контроллера
        }

        void ResetBattle() // сброс переменных
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
