using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DiceyAdventuresAR.GameObjects.Players;
using DiceyAdventuresAR.MyLevelGraph;

namespace DiceyAdventuresAR.GameObjects
{
    public class Exit : Item
    {
        public override void UseByPlayer(Player player)
        {
            player.GetComponentInChildren<MeshRenderer>().enabled = false;
            StartCoroutine(EndGame(player));
        }

        IEnumerator EndGame(Player player)
        {
            var message = AppearingAnim.CreateMsg("EndGameMsg", new Vector2(0.29f, 0.37f), new Vector2(0.7f, 0.68f), "Уровень пройден!");

            message.yOffset = 50;
            message.color = Color.green;
            message.period = 2;
            message.Play();

            yield return new WaitForSeconds(2);

            IEnumerable<GameObject> levels = FindObjectsOfType<LevelGraph>(true).Select(lvl => lvl.gameObject);
            levels.First(obj => obj.name == "CrystalLevel").SetActive(false);
            levels.First(obj => obj.name == "MoonLevel").SetActive(true);

            player.transform.parent = null;
            player.GetComponentInChildren<MeshRenderer>().enabled = true;
        }
    }
}
