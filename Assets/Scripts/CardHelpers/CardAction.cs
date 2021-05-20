namespace DiceyAdventuresAR.Battle
{
    public enum CardAction // перечисление действий (классы) карточек (Damage по умолчанию)
    {
        None, // нет карточки
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
