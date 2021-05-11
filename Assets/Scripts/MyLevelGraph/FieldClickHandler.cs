using UnityEngine;
using DiceyAdventuresAR.MyLevelGraph;
using DiceyAdventuresAR.AR;

namespace DiceyAdventuresAR.MyLevelGraph
{
    public class FieldClickHandler : MonoBehaviour, ISelectableObject
    {
        Field field; // ��������� ���� � �������� (����� �������)

        public bool IsSelected { get; set; } = false; // ���������

        void Start()
        {
            field = transform.parent.GetComponent<Field>();
        }

        void OnMouseDown()
        {
            OnSelectEnter(); // ���� ��������� ��� ������������ �� ����������
        }

        public void OnSelectEnter()
        {
            LevelGraph.levelGraph.player.TryToPlacePlayer(field); // ��� ��������� �������� ������
        }

        public void OnSelectExit()
        {
            field.MarkAsUnattainable(false); // ����� ��������� (AR ������) �������, ������ �������
        }
    }
}
