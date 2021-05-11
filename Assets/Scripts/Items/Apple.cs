using DiceyAdventuresAR.GameObjects.Players;

namespace DiceyAdventuresAR.GameObjects
{
    public class Apple : Item
    {
        public override void UseByPlayer(Player player)
        {
            player.Heal(6);
            Destroy(gameObject);
        }
    }
}
