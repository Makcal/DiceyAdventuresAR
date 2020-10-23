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
 
        public void Initialize(Field startField, Field connectedField, int weight)
        {
            this.startField = startField;
            this.connectedField = connectedField;
            edgeWeight = weight;
        }

        public override string ToString()
        {
            return $"Edge from {startField} to {connectedField}";
        }
    }
 
}
