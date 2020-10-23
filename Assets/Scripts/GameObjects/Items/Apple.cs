using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Apple : Item
    {
        public override float LocalFieldHeight { get; } = 0.2f;
        public override void UseByPlayer(Player player)
        {
            Destroy(gameObject);
        }
    }
}
