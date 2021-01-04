using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiceyDungeonsAR.GameObjects.Players;
using DiceyDungeonsAR.UI;

namespace DiceyDungeonsAR.GameObjects
{
    public class Exit : Item
    {
        public override void UseByPlayer(Player player)
        {
            StartCoroutine(EndGame());
        }

        IEnumerator EndGame()
        {
            var message = AppearingAnim.CreateMsg("EndGameMsg", new Vector2(0.29f, 0.37f), new Vector2(0.7f, 0.68f), "Уровень пройден.\nИгра окончена!");

            message.yOffset = 50;
            message.color = Color.green;
            message.period = 2;
            message.Play();

            yield return new WaitForSeconds(3);

            SceneManager.LoadScene(MenuHandler.mainMenuScene_);
        }
    }
}
