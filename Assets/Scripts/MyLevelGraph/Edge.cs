using System;
using UnityEngine;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class Edge : MonoBehaviour
    {
        public Material attainableMaterial, defaultMaterial;

        [NonSerialized] public Field startField, connectedField;
        [NonSerialized] public int weight;
        private bool attainable;

        public bool Attainable 
        {
            get => attainable;
            set
            {
                attainable = value;
                GetComponentInChildren<MeshRenderer>().material = value ? attainableMaterial : defaultMaterial;
            }
        }
 
        public void Initialize(LevelGraph level, Field startField, Field connectedField, int weight)
        {
            this.startField = startField;
            this.connectedField = connectedField;
            this.weight = weight;

            transform.parent = level.transform;

            Vector3 scale = Vector3.one;
            scale.z *= (connectedField.transform.localPosition - startField.transform.localPosition).magnitude;
            transform.localScale = new Vector3(1, 1, scale.z);

            transform.rotation = Quaternion.LookRotation(connectedField.transform.GetChild(0).position - startField.transform.GetChild(0).position);

            Vector3 offset = (connectedField.transform.localPosition - startField.transform.localPosition) / 2;
            transform.localPosition = startField.transform.localPosition + offset; // центр ребра - середина расстояния между точками
        }

        public bool HasField(Field field)
        {
            return startField == field || connectedField == field;
        }

        public override string ToString()
        {
            return $"Edge from {startField} to {connectedField}";
        }
    }
 
}
