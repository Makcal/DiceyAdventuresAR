using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Random;
using DiceyAdventuresAR.GameObjects.Players;
using DiceyAdventuresAR.GameObjects;
using DiceyAdventuresAR.Enemies;
using System.Collections;
using DiceyAdventuresAR.Battle;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class LevelGraph : MonoBehaviour
    {
        public byte difficulty;

        public float MAX_LEAF_SIZE = 2f, MIN_LEAF_SIZE = 1f; // константы для размеров деления листов
        public float MAP_RADIUS; // радиус карты
        readonly List<Leaf> leaves = new List<Leaf>(); // список всех листьев

        [SerializeField] Apple applePrefab; // префабы всех объектов уровня
        [SerializeField] Chest chestPrefab;
        [SerializeField] Shop shopPrefab;
        [SerializeField] Exit exitPrefab;
        [SerializeField] GameObject fieldPrefab;
        [SerializeField] Edge edgePrefab;
        [SerializeField] List<EnemyItem> enemies1Level, enemies2Level, enemies3Level, enemies4Level, enemies5Level, bosses;

        static public LevelGraph levelGraph; // статическая переменная для более быстрого обращения к главному скрипту (LevelGraph.levelGraph = this)

        [NonSerialized] public readonly List<Field> fields = new List<Field>(); // список полей
        [NonSerialized] public Player player; // ссылка на игрока
        [NonSerialized] public Field startField; // первое поле игрока
        [NonSerialized] public BattleController battle; // ссылка на контроллер битвы

        IEnumerator Start()
        {
            levelGraph = this; // быстрая ссылка
            battle = GetComponent<BattleController>(); // ссылка на контроллер битвы

            GenerateLevel(); // генерация уровня

            player = FindObjectOfType<Player>(); // поиск игрока
            if (player == null)
            {
                Debug.LogWarning("Player wasn't found");
                yield break;
            }

            yield return null; // подождать инициализацию полей, чтобы покрасить цвет первых полей (= meshRenderer, ждать Start один кадр)
            player.Initialize(); // инициализация игрока
        }

        public void Restart() { UnityEngine.SceneManagement.SceneManager.LoadScene(1); }

        public void GenerateLevel()
        {
            GenerateMainPath();
            CompleteGeneration();
        }

        void GenerateMainPath() // основа
        {
            Leaf.MIN_SIZE = MIN_LEAF_SIZE;

            var root = new Leaf(new Vector2(-MAP_RADIUS, -MAP_RADIUS), new Vector2(MAP_RADIUS * 2, MAP_RADIUS * 2));
            leaves.Add(root); // "корень" для всех остальных листьев

            // снова и снова проходим по каждому листу, пока больше не останется листьев, которые можно разрезать
            List<Leaf> toBeChecked = new List<Leaf>(leaves); // очередь для проверки новых листев
            var doSplit = true;
            while (doSplit)
            {
                doSplit = false; // сбрасываем флаг
                var copy = new List<Leaf>(toBeChecked); // создаём копию, так как нельзя менять список во время его цикла
                toBeChecked.Clear(); // очищаем очередь
                foreach (var leaf in copy)
                {
                    if (leaf.leftChild == null && leaf.rightChild == null) // если лист ещё не разрезан...
                        if (leaf.width > MAX_LEAF_SIZE || leaf.length > MAX_LEAF_SIZE || UnityEngine.Random.value > 0.25)
                        // если этот лист слишком велик, или есть вероятность 75%...
                        {
                            if (leaf.Split()) // пытаемя разрезаеть лист
                            {
                                // если получилось, добавляем обе части в список, чтобы в дальнейшем можно было в цикле проверить и их
                                toBeChecked.Add(leaf.leftChild);
                                toBeChecked.Add(leaf.rightChild);
                                leaves.Add(leaf.leftChild); // добавляем в общий список листев
                                leaves.Add(leaf.rightChild);
                                doSplit = true; // включаем следующую итерацию, чтобы проверить полученные части
                            }
                        }
                }
            }

            // затем рекурсивно проходим по каждому листу и создаём в каждом поле
            root.CalculateFieldPositions();

            foreach (var leaf in leaves)
            {
                if (leaf.rightChild == null && leaf.leftChild == null && leaf.fieldPos != null)
                    leaf.field = CreateField((Vector2)leaf.fieldPos); // создаём поле
            }

            foreach (var leaf in leaves)
                foreach (var conn in leaf.connections)
                {
                    AddEdge(leaf.GetNearestField(conn), conn.GetNearestField(leaf)); // перебираем связи листов и создаём рёбра
                }

            // небольшая смена связей для разнообразия графа: "поворот" ребра к другому полю, чтобы получить новое конеченое поле
            // и сделать граф более ветвистым, вместо одной линии
            List<Field> operatedFields = fields.FindAll(l => l.Edges.Count == 2); // "проходные" поля с двумя связями
            Field toChange = operatedFields[Range(0, operatedFields.Count)]; // случайное
            List<Field> connected = toChange.ConnectedFields(); // два соседа

            int neighbour; // номер соседа, чтобы сменить связь
            if (connected[0].Edges.Count == 1)
                neighbour = 0; // сменить связь с конечным соседом
            else if (connected[1].Edges.Count == 1)
                neighbour = 1;
            else
                neighbour = Range(0, 2); // если ни один из соседов не конечный, то выбрать случайного
            int second = (neighbour == 1) ? 0 : 1; // номер второго соседа, на которого переводится связь
            
            AddEdge(connected[neighbour], connected[second]); // перевод связи на второго соседа

            Edge oldEdge = toChange.Edges.Find(e => e.HasField(connected[neighbour]));
            connected[neighbour].Edges.Remove(oldEdge); // удалить старую связь
            toChange.Edges.Remove(oldEdge);
            Destroy(oldEdge.gameObject);
        }

        void CompleteGeneration() // мелкие штрихи и объекты
        {
            List<Field> operatedFields; // список полей для обработки (копия нужна, чтобы не менялся основной список по ссылке)

            operatedFields = fields.FindAll(f => f.Edges.Count == 1); // "концы" графа (поля с одной связью)

            startField = operatedFields[Range(0, operatedFields.Count)]; // случайное поле для игрока
            operatedFields.Remove(startField); // больше ничего нельзя ставить на это поле

            Field exitField = DijkstrasAlgorithm(startField).OrderBy(pair => pair.Value).Last().Key; // самое удалённое поле от старта
            exitField.PlacedItem = Instantiate(exitPrefab); // выход
            operatedFields.Remove(exitField);

            if (operatedFields.Count > 0)
            {
                Field chestField = operatedFields[Range(0, operatedFields.Count)]; // случайное поле
                chestField.PlacedItem = Instantiate(chestPrefab); // по одному сундуку на каждый уровень
                operatedFields.Remove(chestField);
            }

            operatedFields = fields.FindAll(f => f.Edges.Count > 1); // поля для врагов на перекрёстках
            for (int i = 0; i < Mathf.Round(fields.Count * 0.29f); i++) // кол-во врагов = 29% от числа всех полей
            {
                Field f = operatedFields[Range(0, operatedFields.Count)];
                f.PlacedItem = Instantiate(ChooseEnemy()); // создание врага
                operatedFields.Remove(f); // больше с этим полем не работаем
            }

            operatedFields = fields.FindAll(f => f.PlacedItem == null); // свободные поля для яблок
            operatedFields.Remove(startField); // кроме, конечно, стартового поля
            for (int i = 0; i < Mathf.Round(fields.Count * 0.14f); i++) // на 14% полей можно яблочки расставить
            {
                Field f = operatedFields[Range(0, operatedFields.Count)]; // аналогично врагам
                f.PlacedItem = Instantiate(applePrefab);
                operatedFields.Remove(f);
            }
        }

        EnemyItem ChooseEnemy() // выбор врага
        {
            var enemiesTypes = new List<List<EnemyItem>> { enemies1Level, enemies2Level, enemies3Level, enemies4Level, enemies5Level };
            List<EnemyItem> enemiesList = null;
            switch (difficulty) // выбор уровня врагов
            {
                case 1:
                    enemiesList = enemiesTypes[0]; // на первом уровне только слабые враги
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    enemiesList = value < 0.5f ? enemiesTypes[difficulty - 1] : enemiesTypes[difficulty - 2]; // 50/50 шанс на врага сложности уровня или чуть слабее
                    break;
                case 6:
                    throw new NotImplementedException("Boss");
                default:
                    if (difficulty != 6) // не босс
                        Debug.LogWarning("Invalid level difficulty");
                    break;
            }

            if (enemiesList != null)
                return enemiesList[Range(0, enemiesList.Count)]; // случайный враг из списка
            else
                return null;
        }

        public Field CreateField(Vector2 pos)
        {
            return CreateField(pos.x, pos.y); // через вектор
        }

        public Field CreateField(float x, float z)
        {
            var field = Instantiate(fieldPrefab, transform).GetComponentInChildren<Field>(); // создать поля и найти его компонент
            field.Initialize(this, x, 0, z); // первичная настройка поля
            return field;
        }
 
        public Field FindField(string vertexName) // поиск по названию
        {
            foreach (var f in fields)
                if (f.name == vertexName)
                    return f;
 
            return null;
        }
 
        public Edge AddEdge(string firstName, string secondName, int weight = 0) // соединить поля по имени
        {
            var f1 = FindField(firstName);
            var f2 = FindField(secondName);

            return AddEdge(f1, f2, weight);
        }
        
        public Edge AddEdge(Field first, Field second, int weight = 1) // соединить поля
        {
            if (fields.Contains(first) && fields.Contains(second) && ! first.ConnectedFields().Contains(second))
            {
                Edge edge = Instantiate(edgePrefab);
                edge.Initialize(this, first, second, weight);
                first.AddEdge(edge);
                second.AddEdge(edge);
                return edge;
            }
            return null;
        }

        public Edge AddEdge(int firstIndex, int secondIndex, int weight = 0) // соединить поля по номеру в списке
        {
            if (Mathf.Max(firstIndex, secondIndex) < fields.Count && Mathf.Min(firstIndex, secondIndex) >= 0) // проверка границ индексов
                return AddEdge(fields[firstIndex], fields[secondIndex], weight);
            else
                return null;
        }

        public IEnumerator StartBattle(Enemy enemy) // начать битву
        {
            // создать всплывающее сообщение
            AppearingAnim msg = AppearingAnim.CreateMsg("StartBattleMsg", new Vector2(0.21f, 0.37f), new Vector2(0.78f, 0.68f), "Битва начинается");
            msg.color = Color.red;
            msg.yOffset = 50;
            msg.period = 2;
            msg.Play();

            battle.SetUpOpponents(enemy); // настройка оппонентов (увеличить и поставить)

            for (int i = 2; i < transform.childCount-1; i++)
                transform.GetChild(i).gameObject.SetActive(false); // выключить всё, кроме перых двух детей уровня (земли и игрока)

            player.transform.GetChild(0).gameObject.SetActive(false); // временно спрятать игрока и противника
            enemy.transform.GetChild(0).gameObject.SetActive(false);

            yield return new WaitForSeconds(2.0f); // подождать 2 секунды

            player.transform.GetChild(0).gameObject.SetActive(true); // они появились
            enemy.transform.GetChild(0).gameObject.SetActive(true);

            StartCoroutine(battle.StartBattle()); // битва начинается
        }

        Dictionary<Field, float> DijkstrasAlgorithm(Field start) // приписывает каждому полю длину кратчайшего пути к нему от старта
        {
            // https://ru.wikipedia.org/wiki/Алгоритм_Дейкстры
            var graph = fields.ToDictionary(f => f, f => float.PositiveInfinity); // ближайшие расстояния от старта до каждого поля (беск. - неизвестно)
            var notVisited = new List<Field>(fields); // список непосещённых полей
            graph[start] = 0; // до старта расстояние - 0

            Field next;
            while (notVisited.Count > 0)
            {
                next = notVisited.OrderBy(f => graph[f]).First(); // сортировка непосещённых по удалённости и выбор ближайшего к старту

                if (graph[next] == float.PositiveInfinity) // самое ближайшее расстояние из непосещённых - бесконечность
                    break; // значит до этого момента мы ни разу не смогли добраться до этого поля, значит, граф несвязный (состоит из частей)

                foreach (KeyValuePair<Edge, Field> pair in next.ConnectedEdgesWithFields()) // для каждого соседа
                {
                    float newDistance = pair.Key.weight + graph[next]; // новое расстояние до соседа - дистанция до поля + дорога от поля к соседу
                    if (notVisited.Contains(pair.Value) && newDistance < graph[pair.Value]) // если сосед не посещён и обнаружена дорога, короче известной
                        graph[pair.Value] = newDistance; // записать новое значение кратчайшего расстояния
                }

                notVisited.Remove(next); // поле считается посещённым
            }

            return graph;
        }
    }
}
