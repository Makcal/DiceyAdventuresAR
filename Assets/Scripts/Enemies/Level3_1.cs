using DiceyAdventuresAR.Battle;

namespace DiceyAdventuresAR.Enemies
{
    public class Level3_1 : Enemy
    {
        //public override string Name { get; } = "Банка ужасов";
        //protected override int StartHealth { get; } = 34;
        //public override int CubesCount { get; } = 3;
        //public override int Level { get; } = 3;

        protected override void FillInventory()
        {
            inventory[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Poison },
                condition = new Condition() { type = ConditionType.Min, number = 4 },
            };

            inventory[1, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Poison },
                condition = new Condition(),
            };
        }
    }
}
