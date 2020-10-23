using DiceyDungeonsAR.GameObjects;
using UnityEngine;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public class MainCube : Player
    {
        public override int MaxHealth { get; protected set; } = 24;
    }
}
