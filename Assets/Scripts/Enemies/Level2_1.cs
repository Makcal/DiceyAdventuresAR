using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    public class Level2_1 : Enemy
    {
        public override string Name { get; } = "Горячая голова";
        public override int Level { get; } = 2;
        public override int MaxHealth { get; } = 26;

        public override void FillInventory()
        {
            Cards[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                slotsCount = true,
                bonus = new Bonus() { type = BonusType.Fire },
                condition = new Condition() { type = ConditionType.Doubles },
            };
        }
    }
}
