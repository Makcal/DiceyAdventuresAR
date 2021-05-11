namespace DiceyAdventuresAR.Battle
{
    public enum ConditionType // перечисление типов условий (None по умолчанию)
    {
        None,
        Max, // максимальное число
        Min, // минимальное число
        Even, // чётное
        Odd, // нечётное
        EvOd, // два варианта (не реализовано)
        Number, // конкретное число
        Doubles, // двойнушки (равные числа)
    }
}
