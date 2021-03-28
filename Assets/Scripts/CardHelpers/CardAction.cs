namespace DiceyDungeonsAR.Battle
{
    public enum CardAction // перечисление действий (классы) карточек (Damage по умолчанию)
    {
        Damage, // урон
        DoubleDamage, // двойной урон
        ChangeDice, // перебросить
        // не реализованы:
        Curse, // проклянуть
        Thorns, // шипы
        Poison, // яд
        Freeze, // заморозить
        NewDices, // получить новые кубики
        Shield, // щит
        Blind, // ослепить
    }
}
