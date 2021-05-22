using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.Enemies;
using DiceyAdventuresAR.GameObjects.Players;
using UnityEngine.UI;
using DiceyAdventuresAR.UI;
using UnityEngine.SceneManagement;

namespace DiceyAdventuresAR.Battle
{
    public class BattleController : MonoBehaviour
    {
        [NonSerialized] public RectTransform canvasTr; // быстрая ссылка на трансформ канваса

        [NonSerialized] public Enemy enemy; // враг
        [NonSerialized] public Player player; // игрок

        [NonSerialized] public List<Cube> cubes; // кубики на текущем ходу
        [NonSerialized] public List<ActionCard> cards; // карточки

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
        public IEnumerator SetUpOpponents(Enemy enemy) // подготовка
        {
            this.enemy = enemy; // ссылка на врага
            enemy.transform.localPosition = new Vector3(1.6f, 2, 0);
            enemy.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up); // смотреть на игрока
            enemy.transform.localScale *= 2;

            player = LevelGraph.levelGraph.player; // ссылка на игрока
            player.transform.localPosition = new Vector3(-1.6f, 2, 0);
            player.transform.localRotation = Quaternion.LookRotation(Vector3.right, Vector3.up); // смотреть на врага
            player.transform.localScale *= 2; // сделать большими

            var setters = new List<ObjectSetter> { player.GetComponent<ObjectSetter>(), enemy.GetComponent<ObjectSetter>() };
            setters.ForEach(s => StartCoroutine(s.Set())); // запустить установщики и ждать окончания
            yield return new WaitWhile(() => setters.Select(s => s.ended).Contains(false));
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

                    cards = CreateCards(player.inventory); // создаём карточки из инвентаря игрока
                    cubes = CreateCubes(Mathf.Min(player.Level + 2, 5), true); // кубики для игрока по формуле, но не больше 5

                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded); // ждать окончания хода
                }
                else // ход врага
                {
                    button.GetComponent<Button>().interactable = false; // нельзя нажать на кнопку (не твой ход)

                    cards = CreateCards(enemy.inventory); // создаём карточки из инвентаря врага
                    cubes = CreateCubes(Mathf.Min(enemy.cubesCount, 3), false); // кубики для врага

                    var cor = StartCoroutine(EnemyTurn()); // начать ход врага
                    yield return new WaitUntil(() => LevelGraph.levelGraph.battle.turnEnded); // ждать окончания хода
                    StopCoroutine(cor); // принудительно закончить ход врага
                }

                turnEnded = false; // сбрасываем переменную
                playerTurn = !playerTurn; // смена хода

                foreach (var c in cards)
                    if (c)
                        Destroy(c.gameObject); // уничтожить все карточки
                foreach (var c in cubes)
                    if (c)
                        Destroy(c.gameObject); // уничтожить все кубики

                yield return null;
            }

            Destroy(button.gameObject); // уничтожить кнопку
        }

        List<Cube> CreateCubes(int count, bool toPlayer) // создать n кубиков
        {
            var cubes = new List<Cube>();
            for (int i = 0; i < count; i++)
            {
                var cube = Cube.CreateCube((byte)(UnityEngine.Random.Range(0, 6) + 1)); // создать кубик со случайным числом
                var tr = cube.GetComponent<RectTransform>(); // трансформ 2d графики

                var anchors = toPlayer ? new Vector2(0.4f, 0.1f) : new Vector2(0.6f, 0.9f); // якорь (точка отсчёта координат)
                var xOffset = tr.localScale.x * (tr.rect.width * 1.3f) * i * (toPlayer ? 1 : -1); // сдвиг кубика от якоря
                // (tr.rect.width * 1.3f) - ширина кубика с небольшим пробелом, i раз отступ, localScale - множитель размера
                // у врага порядок справа налево (умножить на -1)
                var pos = new Vector2(xOffset + canvasTr.sizeDelta.x * anchors.x, canvasTr.sizeDelta.y * anchors.y);
                tr.anchoredPosition = pos;
                // устанавливаем позицию кубика относительно якорей на канвасе плюс отступ

                cubes.Add(cube);
            }
            return cubes;
        }

        List<ActionCard> CreateCards(CardDescription[,] descriptions) // создать карточки из массива описаний
        {
            var cards = new List<ActionCard>();
            for (byte j = 0; j < 2; j++)
                // 0 - основной ряд (большие и маленькие)
                // 1 - ряд маленьких карточек (если на ряде 0 стоит тоже маленькая)
                for (byte i = 0; i < descriptions.GetUpperBound(0) + 1; i++) // проход по ряду
                    if (descriptions[i, j] != null) // есть описание
                    {
                        ActionCard card;
                        switch (descriptions[i, j].action) // зависит от действия карточки
                        {
                            case CardAction.Damage:
                                card = ActionCard.CreateDamageCard(descriptions[i, j]); // урон
                                break;
                            case CardAction.ChangeDice:
                                card = ActionCard.CreateChangeDiceCard(descriptions[i, j].uses); // перебросить
                                break;
                            case CardAction.DoubleDamage:
                                card = ActionCard.CreateDoubleDamageCard(descriptions[i, j]); // двойной урон
                                break;
                            default: // не реализовано
                                throw new NotImplementedException($"{descriptions[i, j].action} action hasn't been implemented yet");
                        }
                        cards.Add(card);

                        var tr = (RectTransform)card.transform; // трансформ карточки
                        var width = tr.rect.width * tr.localScale.x; // ширина карточки с учётом масштаба
                        var canvasWidth = ((RectTransform)tr.parent).sizeDelta.x; // ширина экрана
                        // сдвиг от якоря (карточки в ряд)
                        // width + 0.04f*canvasWidth - ширина карточки плюс расстояние между карточками, i раз отступ (для i-ого места)
                        // 0.04f*canvasWidth + width*0.5f - общий сдвиг от края экрана
                        tr.anchoredPosition = new Vector2(0.04f*canvasWidth + width*0.5f + (width + 0.04f*canvasWidth)*i, 0);
                    }
            return cards;
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
                player.transform.position = player.currentField.transform.GetChild(0).position + new Vector3(0, player.currentField.transform.GetChild(0).localScale.y, 0);
                StartCoroutine(player.AddXP(enemy.level)); // дать опыт
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

        IEnumerator EnemyTurn() // ход врага (корутина)
        {
            // сортировка по приоритету. Если c1 - c2 > 0, то считается, что c2 > c1, чтобы сортировалось по убыванию (наоборот)
            cards.Sort((c1, c2) => c1.condition.GetPriority() - c2.condition.GetPriority());

            yield return new WaitForSeconds(0.75f); // пауза на "раздумья"

            foreach (var card in cards)
            {
                List<Cube> suitableCubes;
                do
                {
                    cubes = GetActiveCubes(cubes); // отфильтровать и отсортировать список кубиков
                    suitableCubes = new List<Cube>(); // удовлетворяющие кубики

                    if (card.condition.type == ConditionType.Doubles && card.slotsCount) // карточка с условием двойнушек
                    {
                        for (byte i = 6; i > 0; i--) // от 6 до 1
                        {
                            List<Cube> found = cubes.FindAll(c => c.Value == i); // все кубики с одинаковым числом
                            if (found.Count >= 2) // есть хотя бы два таких
                            {
                                suitableCubes.AddRange(found.GetRange(0, 2)); // добавляем в список подходящих
                                break; // готово
                            }
                        }
                        if (suitableCubes.Count == 0)
                            break; // пропустить эту карточку, если не найдено подходящих кубиков
                    }
                    else if (card.slotsCount) // карточка с двумя слотами без условия
                    {
                        if (cubes.Count >= 2)
                            suitableCubes.AddRange(cubes.GetRange(0, 2)); // просто два самых больших (первых в списке)
                        else
                            break; // недостаточно кубиков
                    }
                    else
                    {
                        var cube = cubes.Find(c => card.condition.Check(c.Value)); // найти первый кубик, что удовлетворяет условию
                        if (cube == null)
                            break; // нет таких кубиков
                        suitableCubes.Add(cube);
                    }

                    for (var i = 0; i < suitableCubes.Count; i++)
                    {
                        Vector3 startPose = suitableCubes[i].transform.position; // стартовая позиция
                        float startTime = Time.time; // стартовое время

                        while (suitableCubes[i]) // пока куб (есть, существует, не "съела" карточка)
                        {
                            if (suitableCubes[i] && card) // если куб и карточка всё ещё существуют
                                suitableCubes[i].transform.position = Vector3.Lerp(
                                    startPose,
                                    // позиция первого пустого кубика в карточке
                                    new List<Cube>(card.slots).Find(c => c.Value == 0).transform.position,
                                    (Time.time - startTime) / 1.0f // отношение пройденного времени к полному теоретическому (1 секунда)
                                ); // приближаем кубик к слоту в карточке для активации
                            yield return null;
                        }
                    }

                    yield return new WaitForSeconds(0.5f); // пауза после каждой карточки
                } while (card.Uses > 0);
            }

            yield return new WaitForSeconds(0.75f); // пауза после хода
            turnEnded = true;
        }

        List<Cube> GetActiveCubes(List<Cube> cubes)
        {
            cubes = new List<Cube>(cubes); // клонируем список, чтобы не менять данный список
            cubes.RemoveAll(c => c == null || c.card != null); // убрать из списка, если нет кубика вовсе или привязан к карточке
            cubes.Sort((c1, c2) => c2.Value - c1.Value); // сортировка по числу на кубике
            return cubes;
        }
    }
}
