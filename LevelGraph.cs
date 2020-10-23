using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.GameObjects.Players;
using DiceyDungeonsAR.GameObjects;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class LevelGraph : MonoBehaviour
    {
        public List<Field> fields = new List<Field>();
        public GameObject fieldPrefab;
        public Player player;
        private GameObject ground;

        public void Start()
        {
            ground = transform.GetChild(0).gameObject;

            GenerateLevel();

            var playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogWarning("Player wasn't found");
                return;
            }

            player = playerObject.GetComponent<Player>();
            if (player == null)
            {
                Debug.LogWarning("Player component wasn't found");
                return;
            }

            player.Initialize();
        }

        public void GenerateLevel()
        {
            //.PlaceItem(Resources.Load("Prefabs/GameObjects/Chest") as GameObject)
            AddField(0, 1);
            AddField(1, 1);
            AddField(1, 2);
            AddField(1, 3);
            AddField(1, 4);
            AddField(2, 4).PlaceItem(Resources.Load("Prefabs/GameObjects/Apple") as GameObject);
            AddField(2, 3);
            AddField(3, 3);
            AddField(4, 3).PlaceItem(Resources.Load("Prefabs/GameObjects/Exit") as GameObject);

            AddField(2, 1).PlaceItem(Resources.Load("Prefabs/GameObjects/Chest") as GameObject);
            AddField(0, 2).PlaceItem(Resources.Load("Prefabs/GameObjects/Apple") as GameObject);
            AddField(0, 4).PlaceItem(Resources.Load("Prefabs/GameObjects/Chest") as GameObject);
            AddField(2, 2).PlaceItem(Resources.Load("Prefabs/GameObjects/Shop") as GameObject);

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
            var field = Instantiate(fieldPrefab, ground.transform).GetComponentInChildren<Field>();
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
                var edge = Instantiate(Resources.Load("Prefabs/GameObjects/Exit") as GameObject).GetComponent<LevelEdge>();
                edge.Initialized(first, second);
                first.AddEdge(edge);
                second.AddEdge(edge);
            }
        }

        public void AddEdge(int firstIndex, int secondIndex, int weight = 0)
        {
            if (Mathf.Max(firstIndex, secondIndex) < fields.Count && Mathf.Min(firstIndex, secondIndex) >= 0)
                AddEdge(fields[firstIndex], fields[secondIndex], weight);
        }
    }
}
