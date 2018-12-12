using System.Collections;
using UnityEngine;

public class StartTimeWidget : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public TextMesh DisplayTime, TimeBacking;
    private string time;
    bool scroll;

    void Start()
    {
        GetComponent<KMWidget>().OnWidgetActivate += delegate { Activate(); };
        time = BombInfo.GetFormattedTime();
        if (time.Length > 5) scroll = true;
    }

    void Activate()
    {
        gameObject.SetActive(true);
        DisplayTime.text = time.Substring(0,5);
        if (scroll)
        {
            StartCoroutine(ScrollTime());
        }
    }

    IEnumerator ScrollTime()
    {
        var curIndex = -1;
        while (scroll)
        {
            if (curIndex < time.Length - 5)
                curIndex++;
            else curIndex = 0;
            DisplayTime.text = time.Substring(curIndex, 5);
            if (!DisplayTime.text.Contains(":"))
            {
                DisplayTime.text = time.Substring(0, 4);
                TimeBacking.text = "8888";
            }
            else
            {
                var colonIndex = DisplayTime.text.IndexOf(":");
                TimeBacking.text = "8888";
                TimeBacking.text = TimeBacking.text.Substring(0, colonIndex) + ":" + TimeBacking.text.Substring(colonIndex, TimeBacking.text.Length - colonIndex);
            }
            yield return new WaitForSeconds(1);
        }
    }
}
