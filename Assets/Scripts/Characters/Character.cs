using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.UI;
using DiceyAdventuresAR.Battle;

namespace DiceyAdventuresAR.GameObjects
{
    [RequireComponent(typeof(ObjectSetter))]
    public abstract class Character : MonoBehaviour
    {
        // общие параметры классов персонажа (abstract нужен, чтобы каждый присвоил своё значение)
        public string charName; // имя персонажа
        [SerializeField] int startHealth; // начальное (максимальное) здоровье

        protected static LevelGraph levelGraph; // быстрая ссылка на уровень
        protected static Transform canvasTr; // быстрая ссылка на трансформ канваса

        [SerializeField] Sprite healthSprite; // картинка для жизней
        Bar healthBar; // шкала жизней
        Image healthIcon; // иконка жизней
        Text nameText; // текст с именем персонажа

        public readonly CardDescription[,] inventory = new CardDescription[4, 2]; // инвентарь с описаниями карточек
        // [i, 0] - главная линия, [i, 1] - вторая линия маленьких карточек
        [SerializeField] CardDescription[] mainSlots = new CardDescription[4]; // нижний ряд для маленьких и больших карточек
        [SerializeField] CardDescription[] extraSlots = new CardDescription[4]; // верхний ряд только для маленьких карточек

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

        void OnValidate()
        {
            if (mainSlots.Length != 4)
                Array.Resize(ref mainSlots, 4);
            if (extraSlots.Length != 4)
                Array.Resize(ref extraSlots, 4);
        }

        public virtual void Initialize() // первичная настройка персонажа, вместо конструктора (он недоступен из-за Unity)
        {
            levelGraph = LevelGraph.levelGraph; // получаем уровень
            canvasTr = GameObject.FindGameObjectWithTag("Canvas").transform; // ищем канвас для 2d графики
            if (health == 0) // игрок инициализируется первый раз
            {
                health = maxHealth = startHealth; // задать начальное здоровье (без свойств Health и MaxHealth, так как полоски ещё не созданы)
                FillInventory(); // придумываем карточки персонажу
            }
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

            // sizeDelta - сумма отступов вправо двух сторон прямоугольника от соответствующих сторон якорей
            // rect.size == sizeDelta + distanceBetweenAnchors - расстояние между углами прямоугольника (не якорями)
            // distanceBetweenAnchors.x == (anchorMax.x - anchorMin.x) * canvasTr.sizeDelta.x - буквально расстояние между якорями
            imgTr.anchorMin = lowerLeftIconCornerPos; // устанавливаем якоря (координаты углов как доля (0-1) от всего экрана)
            imgTr.anchorMax = topRightIconCornerPos;
            imgTr.pivot = new Vector2(0, 0.5f); // иконка левой стороной прилегает к якорям (y не важен, так как совпадают только x якорей)
            imgTr.offsetMin = imgTr.offsetMax = Vector2.zero; // сначала углы прямоугольника прилегают к якорям (нет отступа)
            imgTr.sizeDelta = new Vector2(imgTr.rect.height, 0); // ширина прямоугольника увеличивается вправо на высоту, чтобы был квадрат

            healthIcon = img.AddComponent<Image>();
            healthIcon.sprite = healthSprite; // установка картинки
        }

        protected void CreateNameText(Vector2 lowerLeftTextCornerPos, Vector2 topRightTextCornerPos)
        {
            var obj = new GameObject(charName, typeof(Outline)); // создаём объект текста сразу с обводкой

            var tr = obj.AddComponent<RectTransform>(); // компонент 2d трансформа
            tr.SetParent(canvasTr); // вся 2d графика принадлежит канвасу

            tr.anchorMin = lowerLeftTextCornerPos; // устанавливаем якоря (координаты углов как доля (0-1) от всего экрана)
            tr.anchorMax = topRightTextCornerPos;
            tr.offsetMin = tr.offsetMax = Vector2.zero; // нет отступов, углы прямоугольника совпадают с якорями

            nameText = obj.AddComponent<Text>(); // компонент текста
            nameText.text = charName;
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // получить шрифт
            nameText.alignment = TextAnchor.LowerLeft;
            nameText.resizeTextForBestFit = true; // автоподбор максимального размера
            nameText.resizeTextMaxSize = 300;
            nameText.resizeTextMinSize = 2;
        }

        void FillInventory() // метод для заполнения инвентаря описаниями карточек
        {
            for (int i = 0; i < inventory.GetUpperBound(0) + 1; i++)
            {
                inventory[i, 0] = mainSlots[i].action != CardAction.None ? mainSlots[i] : null; // null если действия нет по умолчанию
                inventory[i, 1] = !mainSlots[i].size && extraSlots[i].action != CardAction.None ? extraSlots[i] : null;
                // дополнительная карточка, только если у основной карточки маленький размер
            }
        }

        public virtual IEnumerator Death() // смерть
        {
            Destroy(healthBar.gameObject);
            Destroy(healthIcon.gameObject);
            Destroy(nameText.gameObject);
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