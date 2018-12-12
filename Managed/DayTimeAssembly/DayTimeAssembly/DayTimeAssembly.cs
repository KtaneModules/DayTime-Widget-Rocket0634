using System.Collections;
using System.Linq;
using UnityEngine;

public class DayTimeAssembly : MonoBehaviour
{
    private KMGameInfo gameInfo = null;
    private void Start()
    {
        gameInfo = GetComponent<KMGameInfo>();
        gameInfo.OnStateChange += OnStateChange;
        ModConfig<InternationalSettings> modConfig = new ModConfig<InternationalSettings>("InternationalSettings");
        Settings = modConfig.Settings;
    }

    public KMGameInfo.State CurrentState = KMGameInfo.State.Unlock;
    private KMGameInfo.State prevState = KMGameInfo.State.Unlock;
    private Coroutine AddWidget;
    public KMWidget dayTimeWidget;
    public StartTimeWidget startTimeWidget;
    private InternationalSettings Settings = new InternationalSettings();

    private void OnStateChange(KMGameInfo.State state)
    {
        if (AddWidget != null)
        {
            StopCoroutine(AddWidget);
            AddWidget = null;
        }
        if ((prevState == KMGameInfo.State.Setup || prevState == KMGameInfo.State.PostGame) && CurrentState == KMGameInfo.State.Transitioning && state == KMGameInfo.State.Transitioning)
        {
            AddWidget = StartCoroutine(AddWidgetToBomb(dayTimeWidget, startTimeWidget.GetComponent<KMWidget>()));
        }
        prevState = CurrentState;
        CurrentState = state;
    }

    private IEnumerator AddWidgetToBomb(KMWidget widget1, KMWidget widget2)
    {
        var modwidget = widget1.GetComponent<ModWidget>();
        var startWidget = widget2.GetComponent<ModWidget>();
        if (modwidget == null)
            modwidget = widget1.gameObject.AddComponent<ModWidget>();
        if (startWidget == null && Settings.EnableStartTime)
            startWidget = widget2.gameObject.AddComponent<ModWidget>();

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
            DebugLog(Settings.EnableStartTime.ToString() + "" + (startWidget == null).ToString() );
            if (!g.RequiredWidgets.Contains(modwidget))
                g.RequiredWidgets.Add(modwidget);
            if (!g.RequiredWidgets.Contains(startWidget) && Settings.EnableStartTime)
                g.RequiredWidgets.Add(startWidget);
        }

        yield break;
    }

    void DebugLog(string log, int i = 0, params object[] logData)
    {
        log = string.Format(log, logData);
        Debug.LogFormat("[Daytime Widget #{0}] {1}", i, log);
    }

    class InternationalSettings
    {
        public bool EnableColors;
        public bool ForcePreference;
        public bool EnableStartTime;
        public string InternationalStrings;
        public string International;
    }
}
