namespace DiceyDungeonsAR.Battle
{
    public struct Condition // условие
    {
        public ConditionType type; // тип
        public byte number; // число к условию

        public string GetDesc() // текстовые описания условия
        {
            switch (type)
            {
                case ConditionType.Max:
                    return $"Макс.\n{number}";
                case ConditionType.Min:
                    return $"Мин.\n{number}";
                case ConditionType.Even:
                    return "Чётное";
                case ConditionType.Odd:
                    return "Не-\nчётное";
                case ConditionType.Number:
                    return $"{number}";
                default:
                    return "";
            }
        }

        public bool Check(byte n) // подходит ли число условию
        {
            switch (type)
            {
                case ConditionType.Max:
                    return n <= number;
                case ConditionType.Min:
                    return n >= number;
                case ConditionType.Even:
                    return n % 2 == 0;
                case ConditionType.Odd:
                    return n % 2 != 0;
                case ConditionType.EvOd:
                    return true;
                case ConditionType.Number:
                    return n == number;
                default:
                    return true;
            }
        }

        public bool Check(byte n1, byte n2)
        {
            switch (type)
            {
                case ConditionType.Doubles:
                    if (n1 * n2 == 0)
                        return true;
                    return n1 == n2;
                default:
                    return true;
            };
        }

        public byte GetPriority()
        {
            switch (type)
            {
                case ConditionType.Number:
                    return 0;
                case ConditionType.Doubles:
                    return 1;
                case ConditionType.Max:
                    return 2;
                case ConditionType.Min:
                    return 3;
                case ConditionType.Even:
                case ConditionType.Odd:
                    return 4;
                default:
                    return 5;
            }
        }
    }
}
