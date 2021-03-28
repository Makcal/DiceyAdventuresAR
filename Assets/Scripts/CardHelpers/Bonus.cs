using System;
using UnityEngine;

namespace DiceyDungeonsAR.Battle
{
    public struct Bonus // бонус (не реализован)
    {
        public BonusType type; // тип
        public Condition condition; // условие бонуса
        public byte value; // значение бонуса

        public Color32 GetColor() // цвет бонуса
        {
            switch (type)
            {
                case BonusType.Thorns:
                case BonusType.Heal:
                    return new Color32(90, 150, 110, 255); // зелёный
                case BonusType.Freeze:
                    return new Color32(115, 200, 255, 255); // синий
                case BonusType.Weaken:
                    return new Color32(255, 150, 70, 255); // оранжевый
                case BonusType.Curse:
                case BonusType.Lock:
                case BonusType.Poison:
                    return new Color32(180, 160, 240, 255); // фиолетовый
                case BonusType.ReUse:
                case BonusType.Shock:
                    return new Color32(210, 180, 70, 255); // жёлтый
                default:
                    return new Color32(255, 110, 110, 255); // красный
            }
        }
    }
}
