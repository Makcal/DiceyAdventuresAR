using DiceyDungeonsAR.MyLevelGraph;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public abstract class Player : MonoBehaviour
    {
        LevelGraph levelGraph;
        [NonSerialized] public Field currentField = null;
        Field targetField = null;
        float targetTime = -1;

        public abstract int MaxHealth { get; protected set; }
        public abstract int UpgradeHeal { get; protected set; }
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

        public void Initialize()
        {
            health = MaxHealth;

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

        public bool PlacePlayer(Field field)
        {
            if (!currentField.ConnectedFields().Contains(field) || targetField != currentField)
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
            MaxHealth += UpgradeHeal;
            Health = MaxHealth;
        }

        public void DealDamage(int damage)
        {
            Health -= damage;
            if (health == 0)
                Death();
        }

        public void Heal(int health)
        {
            health = Mathf.Abs(health);
            Health += health;

            var message = AppearingAnim.CreateMsg("HealMessage", GameObject.FindGameObjectWithTag("Canvas").transform, $"+{health} HP");

            var transf = message.GetComponent<RectTransform>();
            transf.anchorMin = transf.anchorMax = Vector2.zero;
            transf.anchoredPosition = new Vector2(250, 70);

            message.yOffset = 20;
            message.color = new Color(0, 255, 0);
            message.Play();
        }

        private void Death()
        {

        }
    }
}
