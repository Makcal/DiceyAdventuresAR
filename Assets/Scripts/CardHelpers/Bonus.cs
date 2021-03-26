using System;
using UnityEngine;

namespace DiceyDungeonsAR.Battle
{
    public class Bonus
    {
        public BonusType type;
        public Condition condition = Condition.TrueCond;
        //public Func<int, int> func;
        public byte value;

        public Color32 GetColor()
        {
            switch (type)
            {
                case BonusType.Thorns:
                case BonusType.Heal:
                    return new Color32(90, 150, 110, 255);
                case BonusType.Freeze:
                    return new Color32(115, 200, 255, 255);
                case BonusType.Weaken:
                    return new Color32(255, 150, 70, 255);
                case BonusType.Curse:
                case BonusType.Lock:
                case BonusType.Poison:
                    return new Color32(180, 160, 240, 255);
                case BonusType.ReUse:
                case BonusType.Shock:
                    return new Color32(210, 180, 70, 255);
                default:
                    return new Color32(255, 110, 110, 255);
            }
        }

        public static Bonus NoneBonus => new Bonus();
    }
}
