using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiceyDungeonsAR.GameObjects;
using DiceyDungeonsAR.AR;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class Field : MonoBehaviour, ISelectableObject
    {
        [NonSerialized] new public string name;
        public readonly List<Edge> edges = new List<Edge>();
        LevelGraph level;

        private bool attainable = false;
        private Item placedItem = null;
        [SerializeField] Material defaultMaterial, attainableMaterial, unattainableMaterial;
        MeshRenderer meshRenderer;
        float unattainableTime = 0;

        public bool IsSelected { get; set; } = false;

        public Item PlacedItem
        {
            get => placedItem;
            set
            {
                if (placedItem != null)
                    Destroy(placedItem.gameObject);
                placedItem = value;
                value.field = this;
                value.transform.parent = transform.parent;
                value.transform.localScale = Vector3.one;
                value.transform.localPosition = new Vector3(0, 2 * transform.localScale.y, 0);
                value.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            }
        }

        public bool Attainable
        {
            get => attainable;
            set
            {
                attainable = value;
                meshRenderer.material = value ? attainableMaterial : defaultMaterial;
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

        void OnMouseDown()
        {
            OnSelectEnter(); // типа выделения для тестирования на компьютере
        }

        public void OnSelectEnter()
        {
            level.player.TryToPlacePlayer(this); // при выделении посавить игрока
        }

        public void OnSelectExit()
        {
            MarkAsUnattainable(false); // когда выделение (AR камера) выходит, убрать красный
        }

        public override string ToString()
        {
            return "Field " + name; // строка с названием поля
        }

        public void AddEdge(Edge newEdge)
        {
            edges.Add(newEdge); // занести в список
        }

        public List<Field> ConnectedFields()
        {
            var fields = new List<Field>();
            foreach (var e in edges)
            {
                fields.Add(e.startField == this ? e.connectedField : e.startField); // если первое поле равно этому, то взять второе,
                // иначе первое (противоположное текущему полю)
            }
            return fields;
        }

        public void MarkAttainable()
        {
            var attainableFields = ConnectedFields(); // соседние поля
            foreach (var f in level.player.currentField.ConnectedFields()) // прошлые поля в стандартный
                f.Attainable = false;
            foreach (var f in attainableFields) // соседние поля в зелёный
                f.Attainable = true;
            foreach (var e in level.player.currentField.edges) // прошлые рёбра в стандартный
                e.Attainable = false;
            foreach (var e in edges) // соседние рёбра в зелёный
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
