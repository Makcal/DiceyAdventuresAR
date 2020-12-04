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

    private void Start()
    {
        g = GetComponent<Graphic>();

        Play();
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

        float a = Mathf.Min((Time.time - startTime) / period, 2);

        g.color = new Color(color.r, color.g, color.b, a>1 ? 2-a : a);

        var pos = transform.localPosition;
        pos.y = startHeight + yOffset * a;
        transform.localPosition = pos;
    }
}