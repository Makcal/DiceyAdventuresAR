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
        bool ISelectableObject.IsSelected { get; set; } = false;

        public Item PlacedItem
        {
            get => placedItem;
            set
            {
                if (value.GetComponent<Item>() == null)
                    return;
                if (placedItem != null)
                    Destroy(placedItem.gameObject);
                placedItem = value;
                value.field = this;
                value.transform.parent = transform.parent;
                value.transform.localScale = Vector3.one;
                value.transform.localPosition = new Vector3(0, value.LocalFieldHeight, 0);
            }
        }

        public bool Attainable
        {
            get => attainable;
            set
            {
                attainable = value;
                GetComponentInChildren<MeshRenderer>().material = value ? attainableMaterial : defaultMaterial;
            }
        }

        public void Initialize(LevelGraph level, float x, float y, float z)
        {
            this.level = level;
            name = (level.fields.Count() + 1).ToString();
            level.fields.Add(this);

            x -= 2f; z -= 2f;
            transform.parent.localPosition = new Vector3(x, y, z) * 2f;
        }
        private void OnMouseDown()
        {
            (this as ISelectableObject).OnSelectEnter();
        }
        void ISelectableObject.OnSelectEnter()
        {
            level.player.PlacePlayer(this);
        }

        void ISelectableObject.OnSelectExit() { }

        public override string ToString()
        {
            return "Field #" + name;
        }

        public void AddEdge(LevelEdge newEdge)
        {
            Edges.Add(newEdge);
        }

        public List<Field> ConnectedFields()
        {
            var fields = new List<Field>();
            foreach (var f in Edges)
            {
                fields.Add(f.startField.Equals(this) ? f.connectedField : f.startField);
            }
            return fields;
        }

        public void MarkAdjacentFields()
        {
            var attainableFields = ConnectedFields();
            foreach (var f in attainableFields)
                f.Attainable = true;
            foreach (var f in level.fields.Except(attainableFields))
                f.Attainable = false;
        }

        public Item PlaceItem(Item item)
        {
            PlacedItem = item;
            return item;
        }

        public Item PlaceItem(GameObject prefab)
        {
            if (prefab.GetComponent<Item>() != null) 
            {
                Item item = Instantiate(prefab).GetComponent<Item>();
                PlacedItem = item;
                return item;
            }
            return null;
        }
    }
}
