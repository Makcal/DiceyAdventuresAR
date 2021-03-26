using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;
using System.Collections;
using DiceyDungeonsAR.GameObjects;

namespace DiceyDungeonsAR.Enemies
{
    abstract public class Enemy : Character
    {
        public abstract int CubesCount { get; } // кол-во кубиков
        public abstract int Level { get; } // уровень сложности врага (определяется в наследниках)

        public override void Initialize()
        {
            base.Initialize(); // сначала инициализируем игрока как персонажа в целом

            // создаём UI
            CreateNameText(new Vector2(0.758f, 0.883f), new Vector2(0.897f, 0.950f));
            CreateHealthBar(new Vector2(0.758f, 0.828f), new Vector2(0.930f, 0.883f));
            CreateHealthIcon(new Vector2(0.723f, 0.828f), new Vector2(0.723f, 0.883f));
        }

        public override void GetDamage(int damage) // расширяем метод
        {
            // всплывающее сообщение
            var message = AppearingAnim.CreateMsg("EnemyDamage", new Vector2(0.71f, 0.83f), new Vector2(0.85f, 0.9f), $"- {damage} HP");
            message.yOffset = -20;
            message.color = Color.red;
            message.Play();

            // стандартная версия
            base.GetDamage(damage);
        }

        public override void Heal(int health) // расширяем метод
        {
            // всплывающее сообщение
            var message = AppearingAnim.CreateMsg("EnemyDamage", new Vector2(0.71f, 0.83f), new Vector2(0.85f, 0.9f), $"+ {health} HP");
            message.yOffset = -20;
            message.color = Color.green;
            message.Play();

            // стандартная версия
            base.Heal(health);
        }

        public override IEnumerator Death()
        {
            yield return StartCoroutine(LevelGraph.levelGraph.battle.EndBattle(true)); // окончить битву
            StartCoroutine(base.Death());
        }
    }
}
