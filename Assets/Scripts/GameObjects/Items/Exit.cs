using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Exit : Item
    {
        public override float LocalFieldHeight { get; } = 0.1f;
        public override void UseByPlayer(Player player)
        {
            Destroy(gameObject);
        }
    }
}
