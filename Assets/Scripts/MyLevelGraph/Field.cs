using DiceyDungeonsAR.GameObjects;
using DiceyDungeonsAR.AR;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class Field : MonoBehaviour, ISelectableObject
    {
        new public string name;
        private bool attainable = false;
        private Item placedItem = null;
        public Material defaultMaterial, attainableMaterial;
        LevelGraph level;
        public List<LevelEdge> Edges = new List<LevelEdge>();
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
                value.transform.localPosition = new Vector3(0, 1f * transform.localScale.y, 0);
                value.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }

        public bool Attainable
        {
            get => attainable;
            set
            {
                attainable = value;
                GetComponent<MeshRenderer>().material = value ? attainableMaterial : defaultMaterial;
            }
        }

        public void Initialize(LevelGraph level, float x, float y, float z)
        {
            this.level = level;
            name = (level.fields.Count() + 1).ToString();
            level.fields.Add(this);

            transform.parent.localPosition = new Vector3(x, y, z) * 2f;
        }
        void OnMouseDown()
        {
            this.OnSelectEnter();
        }
        public void OnSelectEnter()
        {
            level.player.PlacePlayer(this);
        }

        public void OnSelectExit() { }

        public override string ToString()
        {
            return "Field " + name;
        }

        public void AddEdge(LevelEdge newEdge)
        {
            Edges.Add(newEdge);
        }

        public List<Field> ConnectedFields()
        {
            var fields = new List<Field>();
            foreach (var e in Edges)
            {
                fields.Add(e.startField.Equals(this) ? e.connectedField : e.startField);
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
    }
}
