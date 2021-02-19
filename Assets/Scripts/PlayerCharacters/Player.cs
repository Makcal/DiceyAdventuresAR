using System;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.Battle;
using DiceyDungeonsAR.UI;
using System.Collections;
using UnityEngine.UI;

namespace DiceyDungeonsAR.GameObjects.Players
{
    public abstract class Player : MonoBehaviour
    {
        public Sprite healthSprite, XPSprite;

        LevelGraph levelGraph;
        [NonSerialized] public Field currentField = null;
        Field targetField = null;
        float targetTime = -1;
        [NonSerialized] public Bar playerHealthBar, playerXPBar;

        public abstract int StartHealth { get; }
        private int maxHealth;
        public int MaxHealth
        {
            get => maxHealth;
            private set
            {
                maxHealth = value;
                playerHealthBar.MaxValue = value;
            }
        }
        public abstract int UpgradeHealth { get; }
        protected int health;
        public int Health
        {
            get => health;
            protected set
            {
                health = Mathf.Clamp(value, 0, MaxHealth);
                playerHealthBar.CurrentValue = health;
            }
        }
        public int Level { get; private set; } = 1;
        private int experience = 0;
        public int Experience
        {
            get => experience;
            private set {
                experience = value;
                playerXPBar.CurrentValue = value;
            }
        }
        public int MaxXP { get; private set; } = 2;
        public int Coins { get; private set; } = 0;
        public CardDescription[,] Inventory { get; } = new CardDescription[4, 2];

        public void Initialize()
        {
            health = maxHealth = StartHealth;
            FillInventory();

            var canvasTr = GameObject.FindGameObjectWithTag("Canvas").transform;
            playerHealthBar = Bar.CreateBar(canvasTr, new Vector2(0.068f, 0.131f), new Vector2(0.24f, 0.185f));
            playerHealthBar.maxValue = MaxHealth;
            playerHealthBar.startValue = Health;

            var img = new GameObject("Health icon");
            var imgTr = img.AddComponent<RectTransform>();
            imgTr.SetParent(canvasTr);
            imgTr.offsetMin = imgTr.offsetMax = Vector2.zero;
            imgTr.anchorMin = new Vector2(0.033f, 0.131f);
            imgTr.anchorMax = new Vector2(0.033f, 0.185f);
            imgTr.pivot = new Vector2(0, 0.5f);
            imgTr.sizeDelta = new Vector2(imgTr.rect.height, 0);
            img.AddComponent<Image>().sprite = healthSprite;

            playerXPBar = Bar.CreateBar(canvasTr, new Vector2(0.068f, 0.067f), new Vector2(0.24f, 0.121f));
            playerXPBar.maxValue = MaxXP;
            playerXPBar.startValue = 0;
            playerXPBar.backgroundColor = new Color(35, 97, 28);
            playerXPBar.mainColor = new Color(88, 255, 23);

            img = new GameObject("XP icon");
            imgTr = img.AddComponent<RectTransform>();
            imgTr.SetParent(canvasTr);
            imgTr.offsetMin = imgTr.offsetMax = Vector2.zero;
            imgTr.anchorMin = new Vector2(0.003f, 0.067f);
            imgTr.anchorMax = new Vector2(0.064f, 0.121f);
            var textComp = img.AddComponent<Text>();
            textComp.text = "Опыт:";
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textComp.resizeTextForBestFit = true;
            textComp.resizeTextMinSize = 2;
            textComp.resizeTextMaxSize = 300;
            img.AddComponent<Outline>();

            BattleController.CreateText(canvasTr, new Vector2(0.068f, 0.185f), new Vector2(0.177f, 0.251f), "Ты");

            levelGraph = LevelGraph.levelGraph;
            currentField = targetField = levelGraph.fields[0];
            transform.parent = levelGraph.transform;
            transform.SetSiblingIndex(1);

            transform.position = currentField.transform.position + new Vector3(0, 1f * currentField.transform.localScale.y, 0);
            transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
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

        public IEnumerator AddXP(int experience)
        {
            Experience += experience;

            while (Experience >= MaxXP)
            {
                LevelUp();
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void LevelUp()
        {
            Level += 1;
            Experience -= MaxXP;
            MaxXP += Level;
            MaxHealth += UpgradeHealth;
            Health = MaxHealth;

            playerXPBar.MaxValue = MaxXP;
            var msg = AppearingAnim.CreateMsg($"LevelUpTo{Level}", new Vector2(0.055f, 0.251f), new Vector2(0.255f, 0.305f), "Уровень повышен!");
            msg.color = Color.green;
            msg.period = 2;
            msg.yOffset = 20;
            msg.Play();
        }

        public void DealDamage(int damage)
        {
            damage = Mathf.Abs(damage);
            Health -= damage;

            var message = AppearingAnim.CreateMsg("PlayerDamage", new Vector2(0.17f, 0.08f), new Vector2(0.31f, 0.15f), $"- {damage} HP");

            message.yOffset = 20;
            message.color = Color.red;
            message.Play();

            if (health == 0)
                StartCoroutine(Death());
        }

        public void Heal(int health)
        {
            health = Mathf.Abs(health);
            Health += health;

            var message = AppearingAnim.CreateMsg("HealMessage", new Vector2(0.17f, 0.08f), new Vector2(0.31f, 0.15f), $"+ {health} HP");

            message.yOffset = 20;
            message.color = new Color(0, 255, 0);
            message.Play();
        }

        private IEnumerator Death()
        {
            yield return StartCoroutine(levelGraph.battle.EndBattle(false));
            Destroy(gameObject);
        }

        protected virtual void FillInventory()
        {
            Inventory[0, 0] = new CardDescription()
            {
                action = CardAction.Damage,
            };

            Inventory[3, 0] = new CardDescription()
            {
                uses = 3,
                action = CardAction.ChangeDice,
            };
        }
    }
}
