using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.Enemies
{
    abstract public class Enemy : MonoBehaviour
    {
        abstract public string Name { get; }
        abstract public int Level { get; }
        abstract public int MaxHealth { get; }
        protected int health;
        public int Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, MaxHealth);
            }
        }
        public CardDescription[,] Cards { get; } = new CardDescription[4, 2];

        void Start()
        {
            health = MaxHealth;
            FillInventory();
        }

        public abstract void FillInventory();

        public void DealDamage(int damage)
        {
            damage = Mathf.Abs(damage);
            Health -= damage;
            LevelGraph.levelGraph.battle.enemyBar.CurrentValue -= damage;

            var message = AppearingAnim.CreateMsg("EnemyDamage", $"- {damage} HP");
            var transf = message.GetComponent<RectTransform>();
            transf.anchorMin = transf.anchorMax = new Vector2(0.76f, 0.87f);

            message.yOffset = -20;
            message.color = Color.red;
            message.Play();

            if (health == 0)
                Death();
        }

        public void Death()
        {
            Destroy(gameObject);
            StartCoroutine(LevelGraph.levelGraph.battle.EndBattle(true));
        }
    }
}
