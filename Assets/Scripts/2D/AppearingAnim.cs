using UnityEngine;
using UnityEngine.UI;

public class AppearingAnim : MonoBehaviour
{
    bool started = false;
    float startTime;
    float startHeight;
    Graphic g;
    public Color color;
    public float period = 1;
    public float yOffset = 0;

    static public AppearingAnim CreateMsg(string name, string text="", int fontSize = 32, Font font = null)
    {
        GameObject msg = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(AppearingAnim)) { layer = 5 };
        
        var transf = (RectTransform)msg.transform;
        transf.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
        transf.anchorMin = transf.anchorMax = Vector2.one / 2;
        transf.anchoredPosition = Vector2.zero;

        var textComp = msg.GetComponent<Text>();
        textComp.text = text;
        textComp.alignment = TextAnchor.MiddleCenter;
        if (font == null)
            font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textComp.font = font;
        textComp.fontSize = fontSize;

        return msg.GetComponent<AppearingAnim>();
    }

    private void Start()
    {
        g = GetComponent<Graphic>();
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

        g.color = new Color(color.r, color.g, color.b, a>1 ? 2-a : a);

        var pos = transform.localPosition;
        pos.y = startHeight + yOffset * a;
        transform.localPosition = pos;

        if (a == 2)
            Destroy(gameObject);
    }
}