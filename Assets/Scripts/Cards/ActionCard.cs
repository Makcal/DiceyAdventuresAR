using UnityEngine;
using UnityEngine.UI;
using DiceyAdventuresAR.MyLevelGraph;
using TMPro;

namespace DiceyAdventuresAR.Battle
{
    // карточка действия
    public abstract class ActionCard : MonoBehaviour
    {
        public Cube[] slots; // слоты для кубиков (пустые кубики со значением 0)
        public bool slotsCount = false; // true - 2 слота, false - 1 слот
        public bool size; // true - большая карточка, false - маленькая

        public Condition condition; // структура (как класс) с условием для карточки

        public byte Uses // кол-во оставшихся использований
        {
            get
            {
                var tmpTr = transform.GetChild(2); // третий ребёнок - текст использований
                var text = tmpTr.GetComponent<TextMeshProUGUI>().text; // компонент текста
                return text != "" ? byte.Parse(text.Replace(" осталось", "")) : (byte)1;
                // стираем текст и получаем число (1, если нет текста)
            }
            set
            {
                var tmpTr = transform.GetChild(2); // объект с текстом
                tmpTr.GetComponent<TextMeshProUGUI>().text = $"{value} осталось"; // установить текст
            }
        }

        public Color32 Color // цвет карточки
        {
            get => GetComponent<Image>().color; // обращаемся к картинке
            set
            {
                GetComponent<Image>().color = value;
            }
        }

        public abstract void DoAction(); // действие

        public byte GetSum() // посчитать сумму кубиков
        {
            return (byte)(slots[0].Value + (slotsCount ? slots[1].Value : 0)); // первый кубик + возможный второй кубик
        }

        public bool TryToDoAction() // попытка выполнить действие
        {
            foreach (var c in slots)
                if (c.Value == 0)
                    return false; // нельзя, если есть ещё пустые слоты

            Uses--; // теряем одно использование
            DoAction();
            foreach (var c in slots)
                c.Value = 0; // очищаем слоты

            var battle = LevelGraph.levelGraph.battle;
            if (Uses == 0) // если кончились использования
            {
                if (battle.cards.FindAll(c => c).Count == 1) // найти все действующие карточки
                    // Unity приводит объект к true, если он не уничтожен
                    battle.turnEnded = true; // ход окончен, если это была последняя карточка

                Destroy(gameObject); // карточка уничтожается
            }

            return true;
        }

        // создать карточку в целом (нет конкретного действия)
        static T CreateCard<T>(bool size, bool slotsCount, Condition condition, byte uses, Color32 color, string text, string bonus)
            where T : ActionCard
        {
            RectTransform tr = Instantiate(LevelGraph.levelGraph.battle.cardPrefab); // основа карточки
            RectTransform canvasTr = LevelGraph.levelGraph.battle.canvasTr; // канвас
            tr.SetParent(canvasTr);
            tr.sizeDelta = new Vector2(200, size ? 200 : 130); // большие карточки шире

            T card = tr.gameObject.AddComponent<T>(); // компонент карточки данного типа
            card.size = size; // параметры
            card.condition = condition;
            card.Uses = uses;
            card.slotsCount = slotsCount;
            card.Color = color;

            card.slots = slotsCount ? new Cube[2] : new Cube[1]; // создаём массив для слотов
            for (int i = 0; i < card.slots.Length; i++) // настройка каждого слота
            {
                Cube cube = Cube.CreateCube(card.transform, card); // создать кубик с привязкой к созданной карточке

                var cubeTr = cube.GetComponent<RectTransform>();
                if (slotsCount) // позиция слотов
                    // маленькой карточки с двумя слотами не бывает, не рассматриваем такой случай
                    cubeTr.anchorMin = cubeTr.anchorMax = new Vector2(0.25f + 0.5f * i, 0.5f); // 2 слота (0.25; 0.5) и (0.75; 0.5)
                else
                    // если маленькая карточка, то кубик выше середины (0.5; 0.61), чтобы оставить место на текст снизу
                    cubeTr.anchorMin = cubeTr.anchorMax = new Vector2(0.5f, size ? 0.5f : 0.61f);
                cubeTr.anchoredPosition = Vector2.zero; // нет конкретной позиции по умолчанию

                card.slots[i] = cube; // записываем в массив

                if(condition.type != (ConditionType.None | ConditionType.EvOd | ConditionType.Doubles))
                    // для обычных условий добавить описание условия в слот
                    cube.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = condition.GetDesc();
            }

            if (condition.type == ConditionType.Doubles && slotsCount) // если условие двойнушек и два слота, то добавить символ "="
            {
                var textObj = new GameObject("Condition");

                var textTr = textObj.AddComponent<RectTransform>();
                textTr.SetParent(tr);
                textTr.anchorMin = textTr.anchorMax = Vector2.one / 2; // ровно посередине карточки
                textTr.anchoredPosition = Vector2.zero;

                var doublesText = textObj.AddComponent<TextMeshProUGUI>();
                doublesText.text = "=";
                doublesText.fontSize = 50;
                doublesText.alignment = TextAlignmentOptions.Center;
                doublesText.enableKerning = false;
            }

            // теоритическая ширина карточки - 40% от высоты экрана
            // если карточки будут лежать на поле, а не на экране, не будет заморочек с формулами
            float width = 0.4f * canvasTr.sizeDelta.y;
            if (!size)
                width /= 1.54f; // (в 1.54 раза меньше для маленьких карточек)
            tr.localScale *= width / tr.sizeDelta.y; // нужный масштаб для карточки

            var tmpTr = (RectTransform)tr.GetChild(0); // описание карточки
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.31f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = text;

            tmpTr = (RectTransform)tr.GetChild(1); // описание бонуса карточки
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.23f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = bonus;

            tmpTr = (RectTransform)tr.GetChild(2); // исползования
            tmpTr.anchorMin = tmpTr.anchorMax = new Vector2(0.5f, 0.15f);
            tmpTr.GetComponent<TextMeshProUGUI>().text = uses > 1 ? $"{uses} осталось" : "";

            return card;
        }
        static public DamageCard CreateDamageCard(CardDescription description) // карта урона
        {
            var card = CreateCard<DamageCard>( // параметры из описания
                description.size,
                description.slotsCount,
                description.condition,
                description.uses,
                description.bonus.GetColor(), // цвет зависит от бонуса
                "Нанести <sprite index=0> урона", "" // бонус не реализован
            );
            card.bonus = description.bonus;
            return card;
        }

        static public ChangeDiceCard CreateChangeDiceCard(byte uses = 3) // карта перебрасывания
        {
            var diceCard = CreateCard<ChangeDiceCard>(
                true, // всегда большая
                false, // всегда один слот
                new Condition(), // всегда без условия
                uses,
                new Color32(123, 123, 123, 255), // серый цвет
                "Перебросить кубик", ""
            );
            return diceCard;
        }
        static public DoubleDamageCard CreateDoubleDamageCard(CardDescription description) // карта двойного урона
        {
            var card = CreateCard<DoubleDamageCard>( // параметры из описания
                description.size,
                description.slotsCount,
                description.condition,
                description.uses,
                description.bonus.GetColor(),
                "Нанести 2x<sprite index=0> урона", ""
            );
            card.bonus = description.bonus;
            return card;
        }
    }
}
