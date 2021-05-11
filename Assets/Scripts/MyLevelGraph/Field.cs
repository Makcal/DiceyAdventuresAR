using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyAdventuresAR.GameObjects;
using DiceyAdventuresAR.AR;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class Field : MonoBehaviour
    {
        [NonSerialized] new public string name; // имя поля
        public List<Edge> Edges { get; } = new List<Edge>(); // список соединений

        [SerializeField] Material defaultMaterial, attainableMaterial, unattainableMaterial; // покраска
        float unattainableTime = 0; // время красного цвета

        LevelGraph level; // ссылка на уровень
        MeshRenderer meshRenderer;

        Item placedItem = null;
        public Item PlacedItem // предмет, лежащий на поле
        {
            get => placedItem;
            set
            {
                if (placedItem != null)
                    Destroy(placedItem.gameObject);
                placedItem = value;

                if (value == null)
                {
                    return;
                }

                value.field = this; // стартовые параметры
                value.transform.parent = transform.parent;
                value.transform.localScale = Vector3.one;
                value.transform.localPosition = new Vector3(0, 2 * transform.localScale.y, 0);
                value.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            }
        }

        private bool attainable = false; // можно сейчас перейти на это поле за один ход?
        public bool Attainable
        {
            get => attainable;
            set
            {
                attainable = value;
                meshRenderer.material = value ? attainableMaterial : defaultMaterial; // перекрасить поле
            }
        }

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void Initialize(LevelGraph level, float x, float y, float z)
        {
            this.level = level; // установить ссылку
            name = (level.fields.Count + 1).ToString(); // имя поля - номер поля по порядку
            level.fields.Add(this); // занести в список полей

            transform.parent.localPosition = new Vector3(x, y, z) * 1f;
        }

        public override string ToString()
        {
            return "Field " + name; // строка с названием поля
        }

        public void AddEdge(Edge newEdge)
        {
            Edges.Add(newEdge); // занести в список
        }

        public List<Field> ConnectedFields() // соседние поля
        {
            var fields = new List<Field>();
            foreach (var e in Edges)
            {
                fields.Add(e.startField == this ? e.connectedField : e.startField); // если первое поле равно этому, то взять второе,
                // иначе первое (противоположное текущему полю)
            }
            return fields;
        }

        public Dictionary<Edge, Field> ConnectedEdgesWithFields() // соответствие ребро - его сосед
        {
            // если первое поле ребра равно this, то взять второе,
            // иначе первое (противоположное текущему полю)
            return Edges.ToDictionary(e => e, e => e.startField == this ? e.connectedField : e.startField); ;
        }

        public void MarkAttainable()
        {
            var attainableFields = ConnectedFields(); // соседние поля
            foreach (var f in level.player.currentField.ConnectedFields()) // прошлые поля в стандартный
                f.Attainable = false;
            foreach (var f in attainableFields) // соседние поля в зелёный
                f.Attainable = true;
            foreach (var e in level.player.currentField.Edges) // прошлые рёбра в стандартный
                e.Attainable = false;
            foreach (var e in Edges) // соседние рёбра в зелёный
                e.Attainable = true;
        }

        public Item PlaceItem(Item item)
        {
            PlacedItem = item; // положить предмет
            return item;
        }

        public void MarkAsUnattainable(bool value)
        {
            StopCoroutine(nameof(ToUnattainable)); // остановить текущий процесс покраски
            StartCoroutine(ToUnattainable(value)); // запустить новый
        }

        IEnumerator ToUnattainable(bool value) // покраска в красный постепенно
        {
            var newMaterial = new Material(defaultMaterial);
            for (; value ? unattainableTime < 0.5f : unattainableTime > 0; unattainableTime += (value ? 2 : -1) * Time.deltaTime)
            {
                newMaterial.Lerp(defaultMaterial, unattainableMaterial, unattainableTime / 0.5f);
                meshRenderer.material = newMaterial;
                yield return null;
            }
        }
    }
}
