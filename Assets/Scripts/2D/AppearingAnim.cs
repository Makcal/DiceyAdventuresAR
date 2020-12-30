using UnityEngine;
using UnityEngine.UI;

public class AppearingAnim : MonoBehaviour
{
    bool started = false;
    float startTime;
    float startHeight;
    Graphic grapic;
    RectTransform transf;
    float canvasHeight;

    public Color color;
    public float period = 1;
    public float yOffset = 0;

    static public AppearingAnim CreateMsg(string name, Vector2 anchorsMin, Vector2 anchorsMax, string text ="", Font font = null)
    {
        GameObject msg = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline), typeof(AppearingAnim)) { layer = 5 };
        
        var transf = (RectTransform)msg.transform;
        var canvasTr = (RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform;
        transf.SetParent(canvasTr);
        transf.anchorMin = anchorsMin;
        transf.anchorMax = anchorsMax;

        var textComp = msg.GetComponent<Text>();
        textComp.text = text;
        textComp.alignment = TextAnchor.MiddleCenter;
        if (font == null)
            font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textComp.font = font;
        textComp.resizeTextForBestFit = true;
        textComp.resizeTextMinSize = 2;
        textComp.resizeTextMaxSize = 300;

        return msg.GetComponent<AppearingAnim>();
    }

    private void Start()
    {
        transf = (RectTransform)transform;
        transf.offsetMin = transf.offsetMax = Vector2.zero;

        canvasHeight = ((RectTransform)GameObject.FindGameObjectWithTag("Canvas").transform).sizeDelta.y;

        grapic = GetComponent<Graphic>();
    }

    public void Play()
    {
        started = true;
        startTime = Time.time;
        startHeight = transform.localPosition.y;
    }

    void Update()
    {
        if (!started)
            return;

        float a = Mathf.Min((Time.time - startTime) * 2 / period, 2);

        grapic.color = new Color(color.r, color.g, color.b, a>1 ? 2-a : a);

        var pos = new Vector2(0, 0);
        pos.y = yOffset * canvasHeight / 515 * a / 2;
        transf.offsetMin = transf.offsetMax = pos;

        if (a == 2)
            Destroy(gameObject);
    }
}