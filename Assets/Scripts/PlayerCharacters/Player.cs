using System;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public abstract class Player : MonoBehaviour
    {
        LevelGraph levelGraph;
        [NonSerialized] public Field currentField = null;
        Field targetField = null;
        float targetTime = -1;

        public abstract int MaxHealth { get; protected set; }
        public abstract int UpgradeHealth { get; protected set; }
        protected int health;
        public int Health
        {
            get => health;
            protected set
            {
                health = Mathf.Clamp(value, 0, MaxHealth);
            }
        }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; } = 0;
        public int MaxXP { get; private set; } = 2;
        public int Coins { get; private set; } = 0;
        public CardDescription[,] Inventory { get; } = new CardDescription[4, 2];

        public void Initialize()
        {
            health = MaxHealth;
            FillInventory();
            
            levelGraph = LevelGraph.levelGraph;
            currentField = levelGraph.fields[0];
            targetField = currentField;
            transform.position = currentField.transform.position + new Vector3(0, 1f * targetField.transform.localScale.y, 0);
            currentField.MarkAttainable();
        }

        void FixedUpdate()
        {
            if (targetField != currentField)
            {
                var offset = new Vector3(0, 1f * targetField.transform.localScale.y, 0);
                transform.position = Vector3.Lerp(currentField.transform.position, targetField.transform.position, Mathf.Min(1 - (targetTime - Time.time) / 0.3f, 1)) + offset;
                if (Time.time >= targetTime)
                {
                    currentField = targetField;
                    if (targetField.PlacedItem != null)
                        targetField.PlacedItem.UseByPlayer(this);
                }
            }
        }

        public bool PlacePlayer(Field field, bool force = false)
        {
            if (!force && (!currentField.ConnectedFields().Contains(field) || targetField != currentField))
            {
                return false;
            }
            field.MarkAttainable();

            var targetPosition = field.transform.position;
            targetPosition.y = transform.position.y;
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);

            targetField = field;
            targetTime = Time.time + 0.3f;

            return true;
        }

        public void AddXP(int experience)
        {
            if (experience <= 0)
                throw new ArgumentException();
            Experience += experience;
        }

        private void LevelUp()
        {
            Level += 1;
            Experience = 0;
            MaxXP += Level;
            MaxHealth += UpgradeHealth;
            Health = MaxHealth;
        }

        public void DealDamage(int damage)
        {
            damage = Mathf.Abs(damage);
            Health -= damage;

            var message = AppearingAnim.CreateMsg("PlayerDamage", $"- {damage} HP", 48);
            var transf = message.GetComponent<RectTransform>();
            transf.anchorMin = transf.anchorMax = Vector2.zero;
            transf.anchoredPosition = new Vector2(250, 70);

            message.yOffset = 20;
            message.color = Color.red;
            message.Play();

            if (health == 0)
                Death();
        }

        public void Heal(int health)
        {
            health = Mathf.Abs(health);
            Health += health;

            var message = AppearingAnim.CreateMsg("HealMessage", $"+ {health} HP", 48);

            var transf = message.GetComponent<RectTransform>();
            transf.anchorMin = transf.anchorMax = Vector2.zero;
            transf.anchoredPosition = new Vector2(250, 70);

            message.yOffset = 20;
            message.color = new Color(0, 255, 0);
            message.Play();
        }

        private void Death()
        {
            Destroy(gameObject);
            levelGraph.battle.EndBattle(false);
        }

        private void FillInventory()
        {
            Inventory[0, 0] = Inventory[2, 0] = new CardDescription()
            {
                slotsCount = true,
                action = CardAction.Damage,
            };
            Inventory[1, 0] = new CardDescription()
            {
                size = false,
                condition = new Condition() { number = 3, type = ConditionType.Max },
                bonus = new Bonus() { type = BonusType.Freeze },
                action = CardAction.Damage,
            };
        }
    }
}
