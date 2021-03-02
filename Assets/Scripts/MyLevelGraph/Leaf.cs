using System.Collections.Generic;
using UnityEngine;

namespace DiceyDungeonsAR.MyLevelGraph
{
    public class Leaf
    {
        public static float MIN_SIZE = 1f; // минимальный размер листа (0.5 - диаметр поля + ещё 2 по 0,25 - полполя с обеих сторон)

        public readonly float x, y, width, length; // положение и размер этого листа относительно земли
        public Leaf leftChild, rightChild; // дочерние листы
        public Vector2 fieldPos; // позиция поля, находящегося внутри листа
        public List<Vector2> connections = new List<Vector2>(); // позиции другими полям

        public Leaf(float x, float y, float width, float length)
        {
            // инициализация листа
            this.x = x;
            this.y = y;
            this.width = width;
            this.length = length;
            LevelGraph l = LevelGraph.levelGraph;
            Vector3 start = l.transform.position + new Vector3(x, 0, y);
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
            // если ширина более чем на 25% больше высоты, то разрезаем вертикально
            // если высота более чем на 25% больше ширины, то разрезаем горизонтально
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
                leftChild = new Leaf(x, y, width, split);
                rightChild = new Leaf(x, y + split, width, length - split);
                /*  _
                 * |_|
                 * |_|
                 */
            }
            else
            {
                leftChild = new Leaf(x, y, split, length);
                rightChild = new Leaf(x + split, y, width - split, length);
                /*   _ _
                 *  |_|_|
                 */
            }
            return true; // разрезание выполнено!
        }

        public Vector2 GetField()
        {
            if (fieldPos != null)
                return fieldPos;

            return Vector2.zero;
        }
    }
}
