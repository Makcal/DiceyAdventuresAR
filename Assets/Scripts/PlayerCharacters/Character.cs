using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DiceyDungeonsAR.MyLevelGraph;
using DiceyDungeonsAR.UI;
using DiceyDungeonsAR.Battle;

namespace DiceyDungeonsAR.GameObjects
{
    public abstract class Character : MonoBehaviour
    {
        // общие параметры классов персонажа (abstract нужен, чтобы каждый присвоил своё значение)
        public abstract string Name { get; } // имя персонажа
        protected abstract int StartHealth { get; } // начальное (максимальное) здоровье

        protected LevelGraph levelGraph; // быстрая ссылка на уровень
        protected Transform canvasTr; // быстрая ссылка на трансформ канваса

        public Sprite healthSprite; // иконка жизней
        protected Bar healthBar; // шкала жизней
        protected GameObject healthIcon; // иконка жизней
        protected Text nameText; // текст с именем персонажа

        int health; // здоровье
        public int Health
        {
            get => health;
            protected set
            {
                health = Mathf.Clamp(value, 0, MaxHealth); // границы для здоровья
                healthBar.CurrentValue = health; // отразить изменения на шкале
            }
        }

        int maxHealth; // макс. здоровье
        public int MaxHealth
        {
            get => maxHealth;
            set
            {
                maxHealth = value; // новый максимум
                healthBar.MaxValue = value; // отразить изменения на шкале
            }
        }

        public readonly CardDescription[,] inventory = new CardDescription[4, 2]; // инвентарь с описаниями карточек

        public virtual void Initialize() // первичная настройка персонажа, вместо конструктора (он недоступен из-за Unity)
        {
            levelGraph = LevelGraph.levelGraph; // получаем уровень
            canvasTr = GameObject.FindGameObjectWithTag("Canvas").transform; // ищем канвас для 2d графики

            health = maxHealth = StartHealth; // задать начальное здоровье (без свойств Health и MaxHealth, так как полоски ещё не созданы)
            FillInventory(); // придумываем карточки персонажу
        }

        protected void CreateHealthBar(Vector2 lowerLeftBarCornerPos, Vector2 topRightBarCornerPos)
        {
            // создаём полоску на нужных координатах (доля относительно всего экрана)
            healthBar = Bar.CreateBar(canvasTr, lowerLeftBarCornerPos, topRightBarCornerPos);

            healthBar.maxValue = MaxHealth; // параметры шкалы
            healthBar.startValue = Health;
        }

        protected void CreateHealthIcon(Vector2 lowerLeftIconCornerPos, Vector2 topRightIconCornerPos)
        {
            var img = new GameObject("Health icon"); // создаём объект иконки

            var imgTr = img.AddComponent<RectTransform>(); 
            imgTr.SetParent(canvasTr);

            // rect.size == sizeDelta + distanceBetweenAnchors
            // distanceBetweenAnchors.x == (anchorMax.x - anchorMin.x) * canvasTr.sizeDelta.x
            imgTr.anchorMin = lowerLeftIconCornerPos; // устанавливаем якоря (координаты углов как доля (0-1) от всего экрана)
            imgTr.anchorMax = topRightIconCornerPos;
            imgTr.pivot = new Vector2(0, 0.5f); // центр прямоугольника - левая сторона иконки
            imgTr.offsetMin = imgTr.offsetMax = Vector2.zero; // сначала углы прямоугольника совпадают с якорями (нет отступа)
            imgTr.sizeDelta = new Vector2(imgTr.rect.height, 0); // ширина прямоугольника увеличивается вправо на высоту (квадрат)

            img.AddComponent<Image>().sprite = healthSprite; // установка картинки
        }

        protected void CreateNameText(Vector2 lowerLeftTextCornerPos, Vector2 topRightTextCornerPos)
        {
            var obj = new GameObject(Name, typeof(Outline)); // создаём объект текста сразу с обводкой

            var tr = obj.AddComponent<RectTransform>(); // компонент 2d трансформа
            tr.SetParent(canvasTr); // вся 2d графика принадлежит канвасу

            tr.anchorMin = lowerLeftTextCornerPos; // устанавливаем якоря (координаты углов как доля (0-1) от всего экрана)
            tr.anchorMax = topRightTextCornerPos;
            tr.offsetMin = tr.offsetMax = Vector2.zero; // нет отступов, углы прямоугольника совпадают с якорями

            nameText = obj.AddComponent<Text>(); // компонент текста
            nameText.text = Name;
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // получить шрифт
            nameText.alignment = TextAnchor.LowerLeft;
            nameText.resizeTextForBestFit = true; // автоподбор максимального размера
            nameText.resizeTextMaxSize = 300;
            nameText.resizeTextMinSize = 2;
        }

        protected abstract void FillInventory(); // метод для заполнения инвентаря описаниями карточек

        public virtual IEnumerator Death() // смерть
        {
            Destroy(healthBar);
            Destroy(healthIcon);
            Destroy(nameText);
            Destroy(gameObject);
            yield break;
        }

        public virtual void GetDamage(int damage) // получение урона
        {
            Health -= damage;

            if (Health == 0)
                StartCoroutine(Death());
        }

        public virtual void Heal(int health) // лечение
        {
            Health += health;
        }
    }
}