using System;
using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.UI;
using System.Collections;
using UnityEngine.UI;

namespace DiceyAdventuresAR.GameObjects.Players
{
    public class Player : Character
    {
        [SerializeField] Sprite XP_Sprite; // иконка и шкала опыта
        Image XP_Icon;
        Bar XP_Bar;

        [SerializeField] private int upgradeHealth; // повышение максимума здоровья за уровень

        [NonSerialized] public Field currentField = null; // текущее поле
        Field targetField = null; // поле, на которое сейчас игрок идёт
        float targetTime = -1; // время, к которому игрок придёт на целевое поле

        // опыт и уровень
        public int Level { get; private set; } = 1; // уровень

        int experience = 0;
        public int Experience // свойство опыта
        {
            get => experience;
            private set 
            {
                experience = value;
                XP_Bar.CurrentValue = value; // обновить шкалу
            }
        }

        int maxXP = 2;
        public int MaxXP // свойство максимального опыта
        {
            get => maxXP;
            private set
            {
                maxXP = value;
                XP_Bar.MaxValue = value; // обновить шкалу
            }
        }

        int coins;
        public int Coins  // монеты (не реализованы)
        {
            get => coins;
            private set
            {
                coins = value;
            }
        }
        
        // методы
        public override void Initialize() // дополняем инициализатор
        {
            base.Initialize(); // сначала инициализируем игрока как персонажа в целом

            // создаём надпись героя
            CreateNameText(new Vector2(0.068f, 0.185f), new Vector2(0.177f, 0.251f));

            // создаём полоску и иконку здоровья
            CreateHealthBar(new Vector2(0.068f, 0.131f), new Vector2(0.24f, 0.185f));
            CreateHealthIcon(new Vector2(0.033f, 0.131f), new Vector2(0.033f, 0.185f));

            // создаём полоску и иконку опыта
            CreateXPBar(new Vector2(0.068f, 0.067f), new Vector2(0.24f, 0.121f));
            CreateXPIcon(new Vector2(0.033f, 0.067f), new Vector2(0.033f, 0.121f));

            currentField = targetField = levelGraph.startField; // устанавливаем игрока на первое место
            transform.parent = levelGraph.transform; // игрок принадлежит уровню
            transform.SetSiblingIndex(1); // объект земли и игрока должны стоять первыми (после идут поля и рёбра)

            // позиция игрока над его полем (стоит на поле)
            transform.position = currentField.transform.GetChild(0).position + new Vector3(0, 1f * currentField.transform.GetChild(0).localScale.y, 0);
            transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0); // случайный поворот
            currentField.MarkAttainable(); // покрасить соседние поля
        }

        void CreateXPBar(Vector2 lowerLeftBarCornerPos, Vector2 topRightBarCornerPos)
        {
            // создаём полоску на нужных якорях (доля относительно всего экрана)
            XP_Bar = Bar.CreateBar(canvasTr, lowerLeftBarCornerPos, topRightBarCornerPos);

            XP_Bar.maxValue = MaxXP; // параметры полосок
            XP_Bar.startValue = 0;
            XP_Bar.backgroundColor = new Color32(35, 97, 28, 255); // цвета полосок
            XP_Bar.mainColor = new Color32(88, 255, 23, 255);
        }

        void CreateXPIcon(Vector2 lowerLeftIconCornerPos, Vector2 topRightIconCornerPos)
        {
            var img = new GameObject("XP icon"); // создаём объект иконки

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

            XP_Icon = img.AddComponent<Image>();
            XP_Icon.sprite = XP_Sprite; // установка картинки
        }

        void FixedUpdate() // бесконечная проверка
        {
            if (targetField != currentField) // цель отличается от текущего поля (есть куда идти)
            {
                var offset = new Vector3(0, 1f * targetField.transform.GetChild(0).localScale.y, 0); // сдвиг вверх, чтобы стоять НА поле

                transform.position = Vector3.Lerp(
                    targetField.transform.GetChild(0).position,
                    currentField.transform.GetChild(0).position,
                    (targetTime - Time.time) / 0.3f // 0.3 - выделенное время на весь путь, разница - времени осталось времени идти
                    // в самом начале осталось идти 0.3, отношение - 1, берём текущее положение
                    // с течение времени отношение уменьшается и, позиция игрока сближается с целью и достигает при отношении, равном 0
                ) + offset; // смешиваем целевую позицию с текущей на основании пройденного времени

                if (Time.time >= targetTime) // если время настало
                {
                    currentField = targetField; // цели бьльше нет
                    if (targetField.PlacedItem != null)
                        targetField.PlacedItem.UseByPlayer(this); // используем предмет на поле, если он есть
                }
            }
        }

        public bool TryToPlacePlayer(Field field) // поставить игрока на поле
        {
            if (targetField != currentField)
                return false; // нельзя, если игрок ещё в движении (есть текущая цель)

            if (!currentField.ConnectedFields().Contains(field))
            {
                if (field != currentField)
                    field.MarkAsUnattainable(true); // покрасить красным, если пытаемя перейти на другое поле, кроме текущего же
                return false; // нельзя, если пытаемя перейти НЕ на соседнее поле
            }

            field.MarkAttainable(); // покрасить новые соседние поля

            var targetPosition = field.transform.GetChild(0).position;
            targetPosition.y = transform.position.y; // целевая позиция, но y текущий
            // смотреть по направлению текущая позиция -> целевая позиция
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);

            targetField = field; // целевое поле
            targetTime = Time.time + 0.3f; // надо прийти ко времени через 0.3 секунды

            return true; // удалось поставить игрока
        }

        public IEnumerator AddXP(int experience)
        {
            Experience += experience; // получить опыт

            while (Experience >= MaxXP) // пока есть лишний опыт
            {
                LevelUp(); // повышаем уровень
                yield return new WaitForSeconds(0.5f); // через каждые полсекунды. Не всё же сразу
            }
        }

        private void LevelUp() // повышаем уровень
        {
            Level += 1; // плюс 1
            Experience -= MaxXP; // тратим опыт
            MaxXP += Level; // новый порог - на число уровня больше, чем предыдущий
            MaxHealth += upgradeHealth; // повышаем живучесть :)
            Health = MaxHealth; // регенирируем

            // всплывающее сообщение
            var msg = AppearingAnim.CreateMsg($"LevelUpTo{Level}", new Vector2(0.055f, 0.251f), new Vector2(0.255f, 0.305f), "Уровень повышен!");
            msg.color = Color.green;
            msg.period = 2;
            msg.yOffset = 20;
            msg.Play();
        }

        public override void GetDamage(int damage) // расширяем метод
        {
            // всплывающее сообщение
            var message = AppearingAnim.CreateMsg("PlayerDamage", new Vector2(0.17f, 0.08f), new Vector2(0.31f, 0.15f), $"- {damage} HP");
            message.yOffset = 20;
            message.color = Color.red;
            message.Play();

            base.GetDamage(damage);
        }

        public override void Heal(int health) // расширяем метод
        {
            // всплывающее сообщение
            var message = AppearingAnim.CreateMsg("PlayerDamage", new Vector2(0.17f, 0.08f), new Vector2(0.31f, 0.15f), $"+ {health} HP");
            message.yOffset = 20;
            message.color = Color.green;
            message.Play();

            base.Heal(health);
        }

        public override IEnumerator Death() // смерть
        {
            Destroy(XP_Bar.gameObject);
            Destroy(XP_Icon.gameObject);
            yield return StartCoroutine(levelGraph.battle.EndBattle(false)); // битва окончена поражением
            StartCoroutine(base.Death());
        }
    }
}
