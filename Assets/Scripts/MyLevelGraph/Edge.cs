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
            scale.z *= (connectedField.transform.parent.localPosition - startField.transform.parent.localPosition).magnitude;
            transform.localScale = new Vector3(1, 1, scale.z);

            transform.rotation = Quaternion.LookRotation(connectedField.transform.position - startField.transform.position);
            var radians = transform.rotation.eulerAngles.y * Mathf.PI / 180;

            var offsetX = new Vector3(scale.z / 2 * Mathf.Sin(radians), 0, 0);
            var offsetZ = new Vector3(0, 0, scale.z / 2 * Mathf.Cos(radians));
            transform.localPosition = startField.transform.parent.localPosition + offsetX + offsetZ;
            transform.localPosition -= new Vector3(0, transform.localPosition.y, 0);
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
