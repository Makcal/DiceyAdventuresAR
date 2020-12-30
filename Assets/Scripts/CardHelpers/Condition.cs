namespace DiceyDungeonsAR.Battle
{
    public struct Condition
    {
        public ConditionType type;
        public byte number;

        public static Condition TrueCond 
        { 
            get
            {
                return new Condition();
            }
        }

        public string GetDesc()
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
                    return "Нечётное";
                case ConditionType.Number:
                    return $"{number}";
                default:
                    return "";
            }
        }

        public bool Check(byte n)
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
    }
}
