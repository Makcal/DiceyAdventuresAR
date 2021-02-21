using System.Collections;
using UnityEngine;
using DiceyDungeonsAR.MyLevelGraph;

public class FieldSelection : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Check());
    }

    IEnumerator Check()
    {
        Field lastField = null;

        while (true)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
            {
                var field = hit.collider.GetComponent<Field>();
                if (field != null && lastField != field)
                {
                    field.OnSelectEnter();
                    lastField = field;
                }
            }
            else
            {
                if (lastField != null)
                    lastField.OnSelectExit();
                lastField = null;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
