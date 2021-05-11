using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.AR;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class FieldClickHandler : MonoBehaviour, ISelectableObject
    {
        Field field; // компонент поля у родителя (корня префаба)

        public bool IsSelected { get; set; } = false; // интерфейс

        void Start()
        {
            field = transform.parent.GetComponent<Field>();
        }

        void OnMouseDown()
        {
            OnSelectEnter(); // типа выделения для тестирования на компьютере
        }

        public void OnSelectEnter()
        {
            LevelGraph.levelGraph.player.TryToPlacePlayer(field); // при выделении посавить игрока
        }

        public void OnSelectExit()
        {
            field.MarkAsUnattainable(false); // когда выделение (AR камера) выходит, убрать красный
        }
    }
}
