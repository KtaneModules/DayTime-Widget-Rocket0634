using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public class DayTimeAssembly : MonoBehaviour
{
    private KMGameInfo gameInfo = null;
    private void Start()
    {
        gameInfo = GetComponent<KMGameInfo>();
        gameInfo.OnStateChange += OnStateChange;
    }

    public KMGameInfo.State CurrentState = KMGameInfo.State.Unlock;
    private KMGameInfo.State prevState = KMGameInfo.State.Unlock;
    private Coroutine AddWidget;
    public KMWidget dayTimeWidget;

    private void OnStateChange(KMGameInfo.State state)
    {
        if (AddWidget != null)
        {
            StopCoroutine(AddWidget);
            AddWidget = null;
        }
        if ((prevState == KMGameInfo.State.Setup || prevState == KMGameInfo.State.PostGame) && CurrentState == KMGameInfo.State.Transitioning && state == KMGameInfo.State.Transitioning)
            AddWidget = StartCoroutine(AddWidgetToBomb(dayTimeWidget));
        prevState = CurrentState;
        CurrentState = state;
    }

    private IEnumerator AddWidgetToBomb(KMWidget widget)
    {
        var modwidget = widget.GetComponent<ModWidget>();
        if (modwidget == null)
        {
            modwidget = widget.gameObject.AddComponent<ModWidget>();
        }

        var generators = FindObjectsOfType<WidgetGenerator>();
        DebugLog($"{generators.Length} Widget Generators found");
        while (generators.Length == 0)
        {
            yield return null;
            generators = FindObjectsOfType<WidgetGenerator>();
            DebugLog($"{generators.Length} Widget Generators found");
        }

        foreach (var g in generators)
        {
            if (modwidget == null) break;
            DebugLog("Adding required widget");
            if (!g.RequiredWidgets.Contains(modwidget))
            {
                g.RequiredWidgets.Add(modwidget);
            }
        }

        yield break;
    }

    void DebugLog(string log, int i = 0, params object[] logData)
    {
        log = string.Format(log, logData);
        Debug.LogFormat("[Daytime Widget #{0}] {1}", i, log);
    }
}
