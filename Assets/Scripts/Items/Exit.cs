using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Exit : Item
    {
        public override void UseByPlayer(Player player)
        {
            Destroy(gameObject);
        }
    }
}
