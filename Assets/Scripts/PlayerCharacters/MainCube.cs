using DiceyDungeonsAR.GameObjects;
using UnityEngine;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public class MainCube : Player
    {
        public override int StartHealth { get; } = 24;
        public override int UpgradeHealth { get; } = 4;
    }
}
