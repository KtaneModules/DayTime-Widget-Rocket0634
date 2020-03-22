using System.Collections;
using System.Linq;
using UnityEngine;

public class DayTimeAssembly : MonoBehaviour
{
    private KMGameInfo gameInfo = null;
    private void Start()
    {
        Instance = this;
        gameInfo = GetComponent<KMGameInfo>();
        gameInfo.OnStateChange += OnStateChange;
        modConfig = new ModConfig<InternationalSettings>("InternationalSettings");
        ReadSettings();
    }

    private void ReadSettings()
    {
        Settings = modConfig.Settings;
        modConfig.Settings = Settings;
    }

    public KMGameInfo.State CurrentState = KMGameInfo.State.Unlock;
    private KMGameInfo.State prevState = KMGameInfo.State.Unlock;
    private Coroutine AddWidget;
    public KMWidget dayTimeWidget;
    public StartTimeWidget startTimeWidget;
    public InternationalSettings Settings = new InternationalSettings();
    ModConfig<InternationalSettings> modConfig;
    public static DayTimeAssembly Instance;

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
            if (Settings != modConfig.Settings) ReadSettings();
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
        Debug.LogFormat("[Daytime Widget Manager] {1}", i, log);
    }
}

public class InternationalSettings
{
    public bool EnableColors = true;
    public bool ForcePreference = false;
    public bool EnableStartTime = false;
    public string HowToUseInternational = "Choose between 'International' and 'American'";
    public Preferred International = Preferred.International;
}

public enum Preferred
{
    American,
    International
}