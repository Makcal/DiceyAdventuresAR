using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Battle;
using System.Collections;
using System;
using DiceyDungeonsAR.UI;

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
        [NonSerialized] public Bar healthBar;
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
            healthBar.CurrentValue -= damage;

            var message = AppearingAnim.CreateMsg("EnemyDamage", new Vector2(0.71f, 0.83f), new Vector2(0.85f, 0.9f), $"- {damage} HP");

            message.yOffset = -20;
            message.color = Color.red;
            message.Play();

            if (health == 0)
                StartCoroutine(Death());
        }

        public IEnumerator Death()
        {
            yield return StartCoroutine(LevelGraph.levelGraph.battle.EndBattle(true));
            Destroy(gameObject);
        }
    }
}
