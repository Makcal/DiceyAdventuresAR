using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class LevelEdge : MonoBehaviour
    {
        public Field startField;
        public Field connectedField;
        public int edgeWeight;
        private bool attainable;
        public Material attainableMaterial;
        public Material defaultMaterial;
        public string start, connected;

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
            start = startField.name;
            connected = connectedField.name;

            this.startField = startField;
            this.connectedField = connectedField;
            edgeWeight = weight;

            var scale = transform.localScale;
            scale.x *= (connectedField.transform.position - startField.transform.position).magnitude;
            transform.localScale = scale;

            transform.rotation = Quaternion.LookRotation(connectedField.transform.position - startField.transform.position);
            var radians = transform.rotation.eulerAngles.y * Mathf.PI / 180;

            var offsetX = new Vector3(scale.x / 2 * Mathf.Sin(radians), 0, 0);
            var offsetZ = new Vector3(0, 0, scale.x / 2 * Mathf.Cos(radians));
            transform.position = startField.transform.position + offsetX + offsetZ;

            transform.parent = level.transform;

        }

        public override string ToString()
        {
            return $"Edge from {startField} to {connectedField}";
        }
    }
 
}
