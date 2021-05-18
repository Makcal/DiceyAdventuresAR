using System;
using System.Collections.Generic;

namespace DiceyAdventuresAR.Battle
{
    [System.Serializable]
    public class CardDescription // описание карточек (чтобы сохранять в инвентаре)
    {
        public CardAction action; // тип карточки
        public bool size = true, slotsCount = false; // размер и кол-во слотов
        public byte uses = 1; // использования
        public Condition condition; // структуры не обязательно создавать словом new
        public Bonus bonus;

        public static bool operator ==(CardDescription first, CardDescription second)
        {
            return first is null == second is null && 
                first.action == second.action &&
                first.size == second.size &&
                first.slotsCount == second.slotsCount &&
                first.uses == second.uses &&
                Equals(first.condition, second.condition) &&
                Equals(first.bonus, second.bonus);
        }

        public static bool operator !=(CardDescription first, CardDescription second)
        {
            return !(first == second);
        }
    }
}
