using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Chest : Item
    {
        public override void UseByPlayer(Player player)
        {
            Destroy(gameObject);
        }
    }
}
