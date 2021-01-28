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
        public Apple applePrefab;
        public Chest chestPrefab;
        public Shop shopPrefab;
        public Exit exitPrefab;
        public GameObject fieldPrefab;
        public LevelEdge edgePrefab;
        public List<EnemyItem> enemies1Level, enemies2Level, enemies3Level, bosses;

        static public LevelGraph levelGraph;

        [NonSerialized] public List<Field> fields = new List<Field>();
        [NonSerialized] public Player player;
        [NonSerialized] public BattleController battle;

        public void Start()
        {
            levelGraph = this;
            battle = GetComponent<BattleController>();

            GenerateLevel();

            var player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogWarning("Player wasn't found");
                return;
            }

            player.Initialize();
        }

        public void GenerateLevel()
        {
            //.PlaceItem(Instantiate(chestPrefab))
            AddField(-2, -1);
            AddField(-1, -1).PlaceItem(Instantiate(enemies1Level[0]));
            AddField(-1, 0);
            AddField(-1, 1);
            AddField(-1, 2).PlaceItem(Instantiate(enemies2Level[0]));
            AddField(0, 2).PlaceItem(Instantiate(applePrefab));
            AddField(0, 1).PlaceItem(Instantiate(enemies3Level[0]));
            AddField(1, 1).PlaceItem(Instantiate(bosses[0]));
            AddField(2, 1).PlaceItem(Instantiate(exitPrefab));

            AddField(0, -1).PlaceItem(Instantiate(chestPrefab));
            AddField(-2, 0).PlaceItem(Instantiate(applePrefab));
            AddField(-2, 2).PlaceItem(Instantiate(chestPrefab));
            AddField(0, 0).PlaceItem(Instantiate(shopPrefab));

            for (int i = 0; i < fields.Count - 5; i++)
            {
                AddEdge(i, i + 1);
            }
            
            AddEdge(1, 9);
            AddEdge(2, 10);
            AddEdge(4, 11);
            AddEdge(6, 12);
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
            if (fields.Contains(first) && fields.Contains(second))
            {
                LevelEdge edge = Instantiate(edgePrefab);
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
