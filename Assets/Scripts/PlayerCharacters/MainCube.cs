using DiceyDungeonsAR.GameObjects;
using UnityEngine;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public class MainCube : Player
    {
        public override string Name { get; } = "Воин";
        protected override int StartHealth { get; } = 24;
        protected override int UpgradeHealth { get; } = 4;
    }
}
