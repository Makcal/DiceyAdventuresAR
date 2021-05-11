using UnityEngine;
using DiceyAdventuresAR.Battle;
using DiceyAdventuresAR.GameObjects.Players;

namespace DiceyAdventuresAR.GameObjects
{
    public class Chest : Item
    {
        public override void UseByPlayer(Player player)
        {
            if (player.inventory[1, 0] == null)
                player.inventory[1, 0] = new CardDescription()
                {
                    size = false,
                    condition = new Condition() { number = 3, type = ConditionType.Max },
                    bonus = new Bonus() { type = BonusType.Freeze },
                    action = CardAction.Damage,
                };
            else if (player.inventory[2, 0] == null)
                player.inventory[2, 0] = new CardDescription()
                {
                    action = CardAction.DoubleDamage,
                    condition = new Condition() { type = ConditionType.Doubles },
                    slotsCount = true,
                    bonus = new Bonus() { type = BonusType.Thorns },
                };

            var msg = AppearingAnim.CreateMsg("New card", new Vector2(0.75f, 0.22f), new Vector2(0.97f, 0.33f), "Получена новая карта");
            msg.color = Color.green;
            msg.period = 2;
            msg.yOffset = 20;
            msg.Play();

            Destroy(gameObject);
        }
    }
}
