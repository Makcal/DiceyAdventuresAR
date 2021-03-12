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
        public readonly List<Edge> Edges = new List<Edge>();
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
            this.level = level;
            name = (level.fields.Count + 1).ToString();
            level.fields.Add(this);

            transform.parent.localPosition = new Vector3(x, y, z) * 1f;
        }

        void OnMouseDown()
        {
            OnSelectEnter();
        }

        public void OnSelectEnter()
        {
            level.player.PlacePlayer(this);
        }

        public void OnSelectExit()
        {
            MarkAsUnattainable(false);
        }

        public override string ToString()
        {
            return "Field " + name;
        }

        public void AddEdge(Edge newEdge)
        {
            Edges.Add(newEdge);
        }

        public List<Field> ConnectedFields()
        {
            var fields = new List<Field>();
            foreach (var e in Edges)
            {
                fields.Add(e.startField == this ? e.connectedField : e.startField); // если первое поле равно этому, то взять второе,
                // иначе первое (противоположное этому полю)
            }
            return fields;
        }

        public void MarkAttainable()
        {
            var attainableFields = ConnectedFields();
            foreach (var f in level.fields)
                f.Attainable = false;
            foreach (var f in attainableFields)
                f.Attainable = true;
            foreach (var e in level.player.currentField.Edges)
                e.Attainable = false;
            foreach (var e in Edges)
                e.Attainable = true;
        }

        public Item PlaceItem(Item item)
        {
            PlacedItem = item;
            return item;
        }

        public void MarkAsUnattainable(bool value)
        {
            StopCoroutine(nameof(ToUnattainable));
            StartCoroutine(ToUnattainable(value));
        }

        IEnumerator ToUnattainable(bool value)
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
