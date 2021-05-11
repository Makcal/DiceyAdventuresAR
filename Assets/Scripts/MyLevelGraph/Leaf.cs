using System.Collections.Generic;
using UnityEngine;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class Leaf
    {
        public static float MIN_SIZE = 1f; // минимальный размер листа (0.5 - диаметр поля + ещё 2 по 0,25 - полполя с обеих сторон)

        public readonly float x, z, width, length; // положение и размер этого листа относительно земли
        public Leaf leftChild, rightChild; // дочерние листы
        public Vector2? fieldPos; // позиция поля, находящегося внутри листа
        public Field field = null; // поле на листе
        public readonly List<Leaf> connections = new List<Leaf>(); // позиции другими полям

        public Leaf(float x, float z, float width, float length)
        {
            // инициализация листа
            this.x = x;
            this.z = z;
            this.width = width;
            this.length = length;

            LevelGraph l = LevelGraph.levelGraph;
            Vector3 start = l.transform.position + new Vector3(x, 0, z);
            Debug.DrawLine(start, start + new Vector3(width, 0, 0), Color.red, 100000);
            Debug.DrawLine(start, start + new Vector3(0, 0, length), Color.red, 100000);
            Debug.DrawLine(start + new Vector3(width, 0, 0), start + new Vector3(width, 0, length), Color.red, 100000);
            Debug.DrawLine(start + new Vector3(0, 0, length), start + new Vector3(width, 0, length), Color.red, 100000);
        }

        public Leaf(Vector2 position, Vector2 size) : this(position.x, position.y, size.x, size.y) { }

        public bool Split()
        {
            // начинаем разрезать лист на два дочерних листа
            if (leftChild != null || rightChild != null)
                return false; // мы уже его разрезали! прекращаем!

            // определяем направление разрезания (H - horizontally)
            // если ширина более чем на 25% больше длины, то разрезаем вертикально
            // если длина более чем на 25% больше ширины, то разрезаем горизонтально
            // иначе выбираем направление разрезания случайным образом
            bool splitH;
            if (width / length >= 1.25)
                splitH = false;
            else if (length / width >= 1.25)
                splitH = true;
            else
                splitH = Random.value > 0.5;

            float max = (splitH ? length : width) - MIN_SIZE; // определяем максимальную высоту или ширину дочернего листа
            if (max <= MIN_SIZE) // слишком короткая сторона для деления
            {
                splitH = !splitH; // попробуем другой разрез
                max = (splitH ? length : width) - MIN_SIZE;
                if (max <= MIN_SIZE)
                    return false; // область слишком мала, больше её делить нельзя...
            }

            float split = Random.Range(MIN_SIZE, max); // определяемся, где будем разрезать

            // создаём левый и правый дочерние листы на основании направления разрезания
            if (splitH)
            {
                leftChild = new Leaf(x, z, width, split);
                rightChild = new Leaf(x, z + split, width, length - split);
                /*  _
                 * |_|
                 * |_|
                 */
            }
            else
            {
                leftChild = new Leaf(x, z, split, length);
                rightChild = new Leaf(x + split, z, width - split, length);
                /*   _ _
                 *  |_|_|
                 */
            }
            return true; // разрезание выполнено!
        }

        public void CalculateFieldPositions()
        {
            // эта функция рекурсивно высчитывает позиции полей и коридоры для этого листа и всех его дочерних листьев.

            if (leftChild != null || rightChild != null)
            {
                // этот лист был разрезан, поэтому переходим к его дочерним листьям
                if (leftChild != null)
                {
                    leftChild.CalculateFieldPositions();
                }
                if (rightChild != null)
                {
                    rightChild.CalculateFieldPositions();
                }

                // если у этого листа есть и левый, и правый дочерние листья, то создаём между ними связь
                if (leftChild != null && rightChild != null)
                {
                    leftChild.connections.Add(rightChild);
                    rightChild.connections.Add(leftChild);
                }
            }
            else
            {
                for (int i = 0; i < 200; i++) // 200 попыток
                {
                    // этот лист готов к созданию поля
                    // располагаем центр платформы внутри листа, но не помещаем её прямо рядом со стороной листа (иначе полябудут стоять вплотную)
                    // центр платформы может находиться в промежутке от половины минимума листа до длины стороны минус полминимума,
                    // так как минимум - два минимальных расстояния центра от сторон
                    var localPos = new Vector2(Random.Range(MIN_SIZE / 2, width - MIN_SIZE / 2), Random.Range(MIN_SIZE / 2, length - MIN_SIZE / 2));
                    // localPos относительно левого нижнего (-x, -z) угла листа!!!
                    fieldPos = new Vector2(x, z) + localPos; // прибавляем вектор угла листа, чтобы получить позицию относительно LevelGraph

                    if (((Vector2)fieldPos).magnitude + 0.25f <= LevelGraph.levelGraph.MAP_RADIUS) // если отрезок от цента до поля + полрадиуса поля меньше радиуса уровня
                        break; // (поле не выходит за границы), то прекращаем перебор
                }
                if (((Vector2)fieldPos).magnitude + 0.25f > LevelGraph.levelGraph.MAP_RADIUS) // если за все попытки не найдено подходящего места
                    fieldPos = null;
            }
        }

        public Field GetNearestField(Leaf otherLeaf)
        {
            // рекурсивно проходим весь путь по этим листьям, чтобы найти ближайшее поле, если оно существует.
            if (field != null)
                return field;
            else
            {
                float border;
                List<Field> fields = GetAllFields();
                if (fields.Count == 0)
                    return null; // полей нет (например, в этом листе нет места)

                if (x == otherLeaf.x)
                {
                    border = Mathf.Max(z, otherLeaf.z); // z горизонтальной границы между листами
                    // сравнение полей по их удалённости от границы
                    fields.Sort((f1, f2) => Mathf.Abs(f1.transform.parent.localPosition.z - border) > Mathf.Abs(f2.transform.parent.localPosition.z - border) ? 1 : -1);
                    return fields[0]; // на первом месте будет стоять самый ближайший
                }
                else if (z == otherLeaf.z)
                {
                    border = Mathf.Max(x, otherLeaf.x); // x вертикальной границы между листами
                    // сравнение полей по их удалённости от границы
                    fields.Sort((f1, f2) => Mathf.Abs(f1.transform.parent.localPosition.x - border) > Mathf.Abs(f2.transform.parent.localPosition.x - border) ? 1 : -1);
                    return fields[0]; // на первом месте будет стоять самый ближайший
                }
                else
                    return null;
            }
        }

        List<Field> GetAllFields() // собрать список всех полей у листа и его дочерних листов
        {
            var fields = new List<Field>();

            if (field != null)
                fields.Add(field); // своё поле
            else
            {
                // собрать поля детей
                if (leftChild != null)
                    fields.AddRange(leftChild.GetAllFields());

                if (rightChild != null)
                    fields.AddRange(rightChild.GetAllFields());
            }

            return fields;
        }
    }
}
