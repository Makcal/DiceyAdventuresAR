namespace DiceyDungeonsAR.Battle
{
    public class CardDescription // описание карточек (чтобы сохранять в инвентаре)
    {
        public CardAction action; // тип карточки
        public bool size = true, slotsCount = false; // размер и кол-во слотов
        public byte uses = 1; // использования
        public Condition condition; // структуры не обязательно создавать словом new
        public Bonus bonus;
    }
}
