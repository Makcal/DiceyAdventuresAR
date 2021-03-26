using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    public class Level1_1 : Enemy
    {
        public override string Name { get; } = "Осьминожка";
        protected override int StartHealth { get; } = 14;
        public override int CubesCount { get; } = 3;
        public override int Level { get; } = 1;

        protected override void FillInventory()
        {
            inventory[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
                bonus = new Bonus() { type = BonusType.Shock },
                condition = new Condition() { type = ConditionType.Even },
            };
        }
    }
}
