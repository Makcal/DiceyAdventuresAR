using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.GameObjects;
using DiceyAdventuresAR.Battle;

namespace DiceyAdventuresAR.Enemies
{
    public class Enemy : Character
    {
        public int cubesCount = 3; // кол-во кубиков
        public int level; // уровень сложности врага (определяется в наследниках)
        [SerializeField] CardDescription[] mainSlots = new CardDescription[4]; // нижний ряд для маленьких и больших карточек
        [SerializeField] CardDescription[] extraSlots = new CardDescription[4]; // верхний ряд только для маленьких карточек

        void OnValidate()
        {
            if (mainSlots.Length != 4)
                Array.Resize(ref mainSlots, 4);
            if (extraSlots.Length != 4)
                Array.Resize(ref extraSlots, 4);
            //for (int i = 0; i < 4; i++)
            //    if (mainSlots[i] != null)
            //        extraSlots[i] = null;
        }

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

        protected override void FillInventory()
        {
            //inventory = 
        }
    }
}
