using DiceyDungeonsAR.GameObjects;
using UnityEngine;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public class MainCube : Player
    {
        public override int MaxHealth { get; protected set; } = 24;
        public override int UpgradeHealth { get; protected set; } = 4;
    }
}
