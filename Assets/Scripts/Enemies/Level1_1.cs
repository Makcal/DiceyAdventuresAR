using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    public class Level1_1 : Enemy
    {
        public override string Name { get; } = "Осьминожка";
        public override int Level { get; } = 1;
        public override int MaxHealth { get; } = 14;

        public override void FillInventory()
        {
            Cards[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Shock },
                condition = new Condition() { type = ConditionType.Even },
            };
        }
    }
}
