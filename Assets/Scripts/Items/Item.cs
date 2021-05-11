using System;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.GameObjects.Players;
using UnityEngine;

namespace DiceyAdventuresAR.GameObjects
{
    public abstract class Item : MonoBehaviour
    {
        [NonSerialized] public Field field;
 
        public void PlaceItem(Field field)
        {
            field.PlacedItem = this;
        }

        public abstract void UseByPlayer(Player player);
    }
}
