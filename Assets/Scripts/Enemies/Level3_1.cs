using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    public class Level3_1 : Enemy
    {
        public override string Name { get; } = "Банка ужасов";
        public override int Level { get; } = 3;
        public override int MaxHealth { get; } = 34;

        public override void FillInventory()
        {
            Cards[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Poison },
                condition = new Condition() { type = ConditionType.Min, number = 4 },
            };

            Cards[1, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Poison },
                condition = new Condition(),
            };
        }
    }
}
