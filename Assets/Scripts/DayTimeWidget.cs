using KModkit;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class DayTimeWidget : MonoBehaviour
{
    static int widgetIDcounter = 1, maxWidget = 0;
    private int widgetID = 0, widget;
    readonly private string manufacture_Key = "manufacture";
    readonly private string dayDate_Key = "day";
    readonly private string time_Key = "time";
    public TextMesh[] Texts;
    int[] number = new int[6];
    readonly int[] startingValues = new[] { 0, 1980, 1, 1, 0, 0 };
    readonly int[] values = new[] { 12, DateTime.Now.Year, 13, 0, 7, 2 };
    string time = "88:88";
    public Color[] dayColors, requiredColors;
    private readonly string[] colorNames = new[] { "Yellow", "Brown", "Blue", "White", "Magenta", "Green", "Orange" };
    readonly string[] months = new[] { "JAN", "FEB", "MAR", "APR", "MAY", "JUNE", "JULY", "AUG", "SEPT", "OCT", "NOV", "DEC" };
    readonly string[] days = new[] { "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT" };
    private Data widgetData;
    static bool manufactureDate;
    static bool dayofWeekDate;
    bool thisManufactureDate;
    bool thisDayofWeekDate;
    bool thisTime;
    bool ampm;
    internal string serial;
    public KMBombInfo BombInfo;
    private List<Func<bool>> func;
    InternationalSettings Settings = new InternationalSettings();

    void Awake()
    {
        var modConfig = new ModConfig<InternationalSettings>("InternationalSettings");
        Settings = modConfig.Settings;
        modConfig.Settings = Settings;
        widgetID = widgetIDcounter++;
        if (maxWidget == 0)
        {
            manufactureDate = false;
            dayofWeekDate = false;
        }
        maxWidget++;
        GetComponent<KMWidget>().OnWidgetActivate += delegate { Activate(); };
        for (int i = 0; i < number.Count(); i++)
        {
            if (i == 3)
            {
                values[3] = DateTime.DaysInMonth(DateTime.Now.Year, number[2]) + 1;
            }
            number[i] = UnityEngine.Random.Range(startingValues[i], values[i]);
        }
        ampm = number.Last() == 0;
        var str = time.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
        if (ampm)
        {
            str[0] = UnityEngine.Random.Range(1, 13).ToString();
            Texts[UnityEngine.Random.Range(0, 2) + 8].gameObject.SetActive(true);
        }
        else str[0] = UnityEngine.Random.Range(0, 24).ToString();
        str[1] = UnityEngine.Random.Range(0, 60).ToString();
        for (int i = 0; i < str.Count(); i++)
        {
            str[i] = str[i].Length == 1 ? "0" + str[i] : str[i];
        }
        time = string.Format("{0}:{1}", str[0], str[1]);
        func = new List<Func<bool>> { () => thisTime = true, () => { thisDayofWeekDate = true; dayofWeekDate = true; return true; },
        () => { thisManufactureDate = true; manufactureDate = true; return true; } };
        func.Add(() => func[1]() && func[2]());
        if (!manufactureDate || !dayofWeekDate)
        {
            if (!manufactureDate && !dayofWeekDate) widget = UnityEngine.Random.Range(0, 2) * 2 + 1;
            else if (!dayofWeekDate)
                widget = 1;
            else if (!manufactureDate)
                widget = 2;
        }
        else widget = 0;
        func[widget]();
        var choose = new[] { ": Date of Manufacture", ": Day of Week", ": Randomized Time" };
        var concat = new List<string> { string.Format("s: {0}, {1}", choose[0].Replace(": ", ""), choose[1].Replace(": ", "")) };
        choose = choose.Concat(concat).ToArray();
        var t = widget == 3 ? widget : 2 - widget;
        DebugLog("Chosen widget{0}", choose[t]);
        GetComponent<KMWidget>().OnQueryRequest += GetQueryResponse;
        widgetData = new Data
        {
            Month = months[number[0]],
            Year = number[1],
            wordDay = number[4],
            dayColor = number[4],
            numberDay = number[3],
            numberMonth = number[2],
            colorEnabled = true,
            monthColor = 1,
            Time = time.Replace(":", ""),
            AmPm = Texts[8].gameObject.activeSelf ? "AM" : Texts[9].gameObject.activeSelf ? "PM" : "MIL"
        };
    }

    //This happens when the bomb turns on, don't turn on any lights or unlit shaders until activate
    public void Activate()
    {
        maxWidget = 0;

        Texts[0].text = widgetData.Month;
        Texts[1].text = "" + widgetData.Year;
        Texts[2].text = days[widgetData.wordDay];
        Texts[2].color = dayColors[widgetData.dayColor];
        if (!Settings.EnableColors)
        {
            Settings.ForcePreference = true;
            widgetData.colorEnabled = false;
            widgetData.dayColor = 5;
            Texts[2].color = new Color(9 / 255f, 1, 0);
        }
        if ((UnityEngine.Random.Range(0, 540) > 235 && !Settings.ForcePreference) || Settings.ForcePreference && Settings.International == Preferred.International)
            International(3);
        else
        {
            International(4);
            widgetData.monthColor = 0;
        }
        Texts[5].text = "" + time[0] + time[1];
        Texts[7].text = "" + time[3] + time[4];
        var inactive = transform.GetComponentsInChildren<Transform>(true).Where(x => !x.gameObject.activeSelf).ToList();
        if (widget == 3)
        {
            for (int i = 0; i < 2; i++)
                inactive[i].gameObject.SetActive(true);
        }
        else
        {
            inactive[2 - widget].gameObject.SetActive(true);
        }
        if (widget == 0)
        {
            StartCoroutine(Blinking());
        }
        ShowWidget();
        //BombInfo.OnBombSolved += delegate () { manufactureDate = false; dayofWeekDate = false; };
        //BombInfo.OnBombExploded += delegate () { manufactureDate = false; dayofWeekDate = false; };
    }

    private void ShowWidget()
    {
        var order = widgetData.monthColor == 0 ? widgetData.numberMonth + "-" + widgetData.numberDay + " (MM/DD)" : widgetData.numberDay + "-" + widgetData.numberMonth + " (DD/MM)";
        var strings = new[] { "Chosen time: " + time + widgetData.AmPm, "Day of the week: " + (Settings.EnableColors ? "(colors enabled) " : "(colors not enabled) ") + colorNames[widgetData.dayColor] + " " + days[widgetData.wordDay] + "-" + order, "Manufacture Date: " + widgetData.Month + "-" + widgetData.Year, ""};
        strings[3] = strings[1] + "\n" + strings[2];
        DebugLog(strings[widget]);
    }

    private void International(int index)
    {
        Texts[index].text = "" + widgetData.numberDay;
        Texts[12 / index].text = "" + widgetData.numberMonth;
        if (Settings.EnableColors)
        {
            Texts[index].color = requiredColors[0];
            Texts[12 / index].color = requiredColors[1];
        }
        else
        {
            Texts[index].color = new Color(9 / 255f, 1, 0);
            Texts[12 / index].color = new Color(9 / 255f, 1, 0);
        }
    }

    public string GetQueryResponse(string queryKey, string queryInfo)
    {
        if (queryKey == manufacture_Key && thisManufactureDate)
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                { "month", widgetData.Month },
                { "year", "" + widgetData.Year }
            };
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }

        if (queryKey == dayDate_Key && thisDayofWeekDate)
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                { "day", Enum.GetName(typeof(DayOfWeek), widgetData.wordDay) },
                { "daycolor", colorNames[widgetData.dayColor] },
                { "date", "" + widgetData.numberDay },
                { "month", "" + widgetData.numberMonth },
                { "colorenabled", "" + widgetData.colorEnabled },
                { "monthcolor", "" + widgetData.monthColor }
            };
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }

        if (queryKey == time_Key && thisTime)
        {
            Dictionary<string, string> response = new Dictionary<string, string>
            {
                { "time", widgetData.Time },
                { "am", widgetData.AmPm.Equals("AM").ToString() },
                { "pm", widgetData.AmPm.Equals("PM").ToString() }
            };
            string responseStr = JsonConvert.SerializeObject(response);
            return responseStr;
        }

        return "";
    }

    IEnumerator Blinking()
    {
        var first = true;
        while (isActiveAndEnabled)
        {
            if (!first) Texts[6].gameObject.SetActive(false);
            first = false;
            var time = (int)BombInfo.GetTime();
            yield return new WaitUntil(() => time != (int)BombInfo.GetTime());
            Func<bool>[] f = { () => BombInfo.GetTime() - (int)BombInfo.GetTime() < .325,
            () => (int)BombInfo.GetTime() + 1 - BombInfo.GetTime() < .325};
            var i = 0;
            if ((int)BombInfo.GetTime() - time > 0 || time - BombInfo.GetTime() > 2) i = 1;
            Texts[6].gameObject.SetActive(true);
            yield return new WaitUntil(() => f[i]());
        }
    }

    void DebugLog(string log, params object[] args)
    {
        var logHeader = string.Format("[DayTime #{0}] ", widgetID);
        var logData = string.Format(log, args);
        logData = logData.Replace("\n", "\n" + logHeader);
        Debug.LogFormat(logHeader + logData);
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "InternationalSettings.json" },
            { "Name", "Day of Week Widget" },
            { "Listings", new List<Dictionary<string, object>> {
                new Dictionary<string, object>
                {
                    { "Key", "EnableColors" },
                    { "Text", "Enable Colors    " },
                    { "Description", "Determine if each day uses a different color, or if the text is always one color.\n(Note, if disabled, Preference will always be forced.)" }
                },
                new Dictionary<string, object>
                {
                    { "Key", "ForcePreference" },
                    { "Text", "Force Preference" },
                    { "Description", "By default, the Day of Week widget alters between International and American layouts for the date.\nSetting this to true will always force the preferred layout." }
                },
                new Dictionary<string, object>
                {
                    { "Key", "International" },
                    { "Type", "Dropdown" },
                    { "Text", "Preferred layout  " },
                    { "Description", "Choose which layout is preferred if\ncolors are disabled or preference is forced." },
                    { "DropdownItems", new List<object>{ "American", "International" } }
                },
                new Dictionary<string, object>
                {
                    { "Key", "EnableStartTime" },
                    { "Text", "Start Time Widget" },
                    { "Description", "A separate widget that shows the start time of the bomb that modules are looking for.\nFor use in Zen Mode." }
                }
            }}
        }
    };

    class Data
    {
        public string Month;
        public int Year;
        public int wordDay;
        public int dayColor;
        public int numberDay;
        public int numberMonth;
        public bool colorEnabled;
        public int monthColor;
        public string Time;
        public string AmPm;
    }
}
