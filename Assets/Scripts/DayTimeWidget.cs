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
    internal bool check = false, keep = true;
    internal string serial;
    static List<DayTimeWidget> isCheck = new List<DayTimeWidget>(), checks = new List<DayTimeWidget>();
    public KMBombInfo BombInfo;
    private List<Func<bool>> func;
    InternationalSettings Settings = new InternationalSettings();

    void Start()
    {
        ModConfig<InternationalSettings> modConfig = new ModConfig<InternationalSettings>("InternationalSettings");
        Settings = modConfig.Settings;
        widgetID = widgetIDcounter++;
        if (maxWidget == 0)
        {
            manufactureDate = false;
            dayofWeekDate = false;
        }
        maxWidget++;
        GetComponent<KMWidget>().OnWidgetActivate += delegate { StartCoroutine(Activate()); };
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
        isCheck.Add(this);
        KeepWidget();
        var either = !manufactureDate || !dayofWeekDate;
        if (either)
        {
            if (!manufactureDate && !dayofWeekDate) widget = UnityEngine.Random.Range(0, 4);
            else if (!manufactureDate)
                widget = UnityEngine.Random.Range(0, 2) * 2;
            else if (!dayofWeekDate)
                widget = UnityEngine.Random.Range(0, 2);
        }
        check = true;
        func[widget]();
        checks.Add(this);
    }

    //This happens when the bomb turns on, don't turn on any lights or unlit shaders until activate
    public IEnumerator Activate()
    {
        yield return null;
        while (checks.Any(x => x.serial == null))
        {
            var c = checks.Where(x => x.serial == null).First();
            c.serial = c.BombInfo.QueryWidgets(KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER, null).Select(x => JsonConvert.DeserializeObject<Dictionary<string, string>>(x)).First()["serial"];
            c.check = false;
            c.keep = true;
        }
        if (!isCheck.Contains(this)) isCheck.Add(this);
        KeepWidget();
        if (checks.Where(x => x.serial == serial && new[] { 1, 2, 3 }.Contains(x.widget)).Count() == 0)
        {
            widget = 3;
            thisTime = false;
            func[widget]();
        }
        else if (checks.Where(x => x.serial == serial && new[] { 1, 3 }.Contains(x.widget)).Count() == 0)
        {
            widget = 1;
            thisManufactureDate = false;
            thisTime = false;
            thisDayofWeekDate = true;
        }
        else if (checks.Where(x => x.serial == serial && new[] { 2, 3 }.Contains(x.widget)).Count() == 0 && checks.Count != 1)
        {
            widget = 2;
            thisDayofWeekDate = false;
            thisTime = false;
            thisManufactureDate = true;
        }
        check = true;
        if (checks.All(x => x.check == true))
        {
            isCheck.Clear();
            checks.Clear();
        }
        var choose = new[] { ": Date of Manufacture", ": Day of Week", ": Randomized Time" };
        var concat = new List<string> { string.Format("s: {0}, {1}", choose[0].Replace(": ", ""), choose[1].Replace(": ", "")) };
        choose = choose.Concat(concat).ToArray();
        var t = widget == 3 ? widget : 2 - widget;
        DebugLog("Chosen widget{0}", choose[t]);
        GetComponent<KMWidget>().OnQueryRequest += GetQueryResponse;
        maxWidget = 0;
        widgetData = new Data
        {
            Month = months[number[0]],
            Year = number[1],
            wordDay = days[number[4]],
            dayColor = number[4],
            numberDay = number[3],
            numberMonth = number[2],
            colorEnabled = true,
            monthColor = 1,
            Time = time.Replace(":", ""),
            AmPm = Texts[8].gameObject.activeSelf ? "AM" : Texts[9].gameObject.activeSelf ? "PM" : "MIL"
        };

        Texts[0].text = widgetData.Month;
        Texts[1].text = "" + widgetData.Year;
        Texts[2].text = widgetData.wordDay;
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
        //BombInfo.OnBombSolved += delegate () { manufactureDate = false; dayofWeekDate = false; };
        //BombInfo.OnBombExploded += delegate () { manufactureDate = false; dayofWeekDate = false; };
    }

    private void KeepWidget()
    {
        while (isCheck.Contains(this) && keep)
        {
            var pass = isCheck.Where(x => x.check == true);
            if (pass.Count() > 0) isCheck.Remove(pass.First());
            if (isCheck.All(x => x.keep)) keep = false;
        }
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
                { "day", widgetData.wordDay },
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
            Texts[6].gameObject.SetActive(true);
            yield return new WaitUntil(() => BombInfo.GetTime() - (int)BombInfo.GetTime() < .325);
        }
    }

    void DebugLog(string log, params object[] args)
    {
        var logData = string.Format(log, args);
        Debug.LogFormat("[DayTime #{0}] {1}", widgetID, logData);
    }

    class Data
    {
        public string Month;
        public int Year;
        public string wordDay;
        public int dayColor;
        public int numberDay;
        public int numberMonth;
        public bool colorEnabled;
        public int monthColor;
        public string Time;
        public string AmPm;
    }

    class InternationalSettings
    {
        public bool EnableColors = true;
        public bool ForcePreference = false;
        public string InternationalStrings = "Choose between \"International\" and \"American\"";
        public Preferred International = Preferred.International;
    }

    enum Preferred
    {
        American,
        International
    }
}
