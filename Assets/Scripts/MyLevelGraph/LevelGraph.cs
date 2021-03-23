using System;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.GameObjects.Players;
using DiceyDungeonsAR.GameObjects;
using DiceyDungeonsAR.Enemies;
using System.Collections;
using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class LevelGraph : MonoBehaviour
    {
        public float MAX_LEAF_SIZE = 2f, MIN_LEAF_SIZE = 1f; // константы для размеров деления листов
        public float MAP_RADIUS; // радиус карты
        readonly List<Leaf> leaves = new List<Leaf>(); // список всех листьев

        [SerializeField] Apple applePrefab; // префабы всех объектов уровня
        [SerializeField] Chest chestPrefab;
        [SerializeField] Shop shopPrefab;
        [SerializeField] Exit exitPrefab;
        [SerializeField] GameObject fieldPrefab;
        [SerializeField] Edge edgePrefab;
        [SerializeField] List<EnemyItem> enemies1Level, enemies2Level, enemies3Level, bosses;

        static public LevelGraph levelGraph; // статическая переменная для более быстрого обращения к главному скрипту (LevelGraph.levelGraph = this)

        [NonSerialized] public readonly List<Field> fields = new List<Field>(); // список полей
        [NonSerialized] public Player player; // ссылка на игрока
        [NonSerialized] public BattleController battle; // ссылка на контроллер битвы

        IEnumerator Start()
        {
            levelGraph = this;
            battle = GetComponent<BattleController>();

            GenerateLevel();

            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogWarning("Player wasn't found");
                yield break;
            }

            yield return null; // подождать инициализацию полей, чтобы покрасить цвет первых полей (= meshRenderer, ждать Start один кадр)
            player.Initialize();
        }

        public void GenerateLevel()
        {
            Leaf.MIN_SIZE = MIN_LEAF_SIZE;
            var root = new Leaf(new Vector2(-MAP_RADIUS, -MAP_RADIUS), new Vector2(MAP_RADIUS * 2, MAP_RADIUS * 2)); // "корень" для всех остальных листьев
            leaves.Add(root);

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

            // затем рекурсивно проходим по каждому листу и создаём в каждом комнату.
            root.CalculateFieldPositions();
            
            foreach (var leaf in leaves)
            {
                if (leaf.rightChild == null && leaf.leftChild == null && leaf.fieldPos != null)
                     leaf.field = AddField((Vector2)leaf.fieldPos); // создаём поле
            }

            foreach (var leaf in leaves)
                foreach (var conn in leaf.connections)
                {
                    AddEdge(leaf.GetNearestField(conn), conn.GetNearestField(leaf)); // перебираем связи листов и создаём рёбра
                }
        }

        public Field AddField(Vector2 pos)
        {
            return AddField(pos.x, pos.y);
        }

        public Field AddField(float x, float z)
        {
            var field = Instantiate(fieldPrefab, transform).GetComponentInChildren<Field>();
            field.Initialize(this, x, 0, z);
            return field;
        }
 
        public Field FindField(string vertexName)
        {
            foreach (var f in fields)
            {
                if (f.name.Equals(vertexName))
                {
                    return f;
                }
            }
 
            return null;
        }
 
        public void AddEdge(string firstName, string secondName, int weight = 0)
        {
            var f1 = FindField(firstName);
            var f2 = FindField(secondName);
            if (f2 != null && f1 != null)
            {
                AddEdge(f1, f2, weight);
            }
        }
        
        public void AddEdge(Field first, Field second, int weight = 0)
        {
            if (fields.Contains(first) && fields.Contains(second) && ! first.ConnectedFields().Contains(second))
            {
                Edge edge = Instantiate(edgePrefab);
                edge.Initialize(this, first, second, weight);
                first.AddEdge(edge);
                second.AddEdge(edge);
            }
        }

        public void AddEdge(int firstIndex, int secondIndex, int weight = 0)
        {
            if (Mathf.Max(firstIndex, secondIndex) < fields.Count && Mathf.Min(firstIndex, secondIndex) >= 0)
                AddEdge(fields[firstIndex], fields[secondIndex], weight);
        }

        public IEnumerator StartBattle(Enemy enemy)
        {
            AppearingAnim msg = AppearingAnim.CreateMsg("StartBattleMsg", new Vector2(0.21f, 0.37f), new Vector2(0.78f, 0.68f), "Битва начинается");
            msg.color = Color.red;
            msg.yOffset = 50;
            msg.period = 2;
            msg.Play();

            battle.SetUpOpponents(enemy);

            for (int i = 2; i < transform.childCount-1; i++)
                transform.GetChild(i).gameObject.SetActive(false);

            player.transform.GetChild(0).gameObject.SetActive(false);
            enemy.transform.GetChild(0).gameObject.SetActive(false);

            yield return new WaitForSeconds(2.0f);

            player.transform.GetChild(0).gameObject.SetActive(true);
            enemy.transform.GetChild(0).gameObject.SetActive(true);

            StartCoroutine(battle.StartBattle());
        }
    }
}
