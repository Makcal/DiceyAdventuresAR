using DiceyDungeonsAR.GameObjects.Players;

namespace DiceyDungeonsAR.GameObjects
{
    public class Shop : Item
    {
        public override float LocalFieldHeight { get; } = 0.04f;
        public override void UseByPlayer(Player player)
        {
            //Destroy(gameObject);
        }
    }
}
