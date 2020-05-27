using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMDateTimeOverrides : MonoBehaviour {

	static DateTime time { get { return DateTime.Now; } }

	static string _day { get { return time.ToString("dd"); } }
	static string _dayAbbr { get { return time.ToString("ddd"); } }
	static string _dayName { get { return time.ToString("dddd"); } }
	static string _hour { get { return time.ToString("hh"); } }
	static string _hourGlobal { get { return time.ToString("HH"); } }
	static string _minutes { get { return time.ToString("mm"); } }
	static string _month { get { return time.ToString("MM"); } }
	static string _monthAbbr { get { return time.ToString("MMM"); } }
	static string _monthName { get { return time.ToString("MMMM"); } }
	static string _seconds { get { return time.ToString("ss"); } }
	static string _ampm { get { return time.ToString("tt"); } }
	static string _year { get { return time.ToString("yyyy"); } }
	static string _timeDiff { get { return time.ToString("%K"); } }
	
	internal string day;
	internal string dayAbbr;
	internal string dayName;
	internal string hour;
	internal string hourGlobal;
	internal string minutes;
	internal string month;
	internal string monthAbbr;
	internal string monthName;
	internal string seconds;
	internal string ampm;
	internal string year;
	internal string timeDiff; 

	public void Reset()
	{
		day = _day;
		dayAbbr = _dayAbbr;
		dayName = _dayName;
		hour = _hour;
		hourGlobal = _hourGlobal;
		minutes = _minutes;
		month = _month;
		monthAbbr = _monthAbbr;
		monthName = _monthName;
		seconds = _seconds;
		ampm = _ampm;
		year = _year;
		timeDiff = _timeDiff;
	}
}

internal class DateTimeOverrideSettings
{
	public bool setOverride = false;
}