using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Apple : Item
    {
        public override float LocalFieldHeight { get; } = 0.4f;
        public override void UseByPlayer(Player player)
        {
            player.Heal(6);
            Destroy(gameObject);
        }
    }
}
