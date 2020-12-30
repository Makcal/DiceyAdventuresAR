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

        public Color GetColor()
        {
            switch (type)
            {
                case BonusType.Thorns:
                case BonusType.Heal:
                    return new Color(90, 150, 110);
                case BonusType.Freeze:
                    return new Color(115, 200, 255);
                case BonusType.Weaken:
                    return new Color(255, 150, 70);
                case BonusType.Curse:
                case BonusType.Lock:
                case BonusType.Poison:
                    return new Color(180, 160, 240);
                case BonusType.ReUse:
                case BonusType.Shock:
                    return new Color(210, 180, 70);
                default:
                    return new Color(255, 110, 110);
            }
        }

        public static Bonus NoneBonus => new Bonus();
    }
}
