using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DiceyAdventuresAR.GameObjects.Players;
using DiceyAdventuresAR.UI;

namespace DiceyAdventuresAR.GameObjects
{
    public class Exit : Item
    {
        public override void UseByPlayer(Player player)
        {
            player.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
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
