using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(KMDateTimeOverrides))]
public class DevDateTimeExtensions : MonoBehaviour
{
	public KMDateTimeOverrides ex;
	public string day;
	public string dayAbbr;
	public string dayName;
	public string hour;
	public string hourGlobal;
	public string minutes;
	public string month;
	public string monthAbbr;
	public string monthName;
	public string seconds;
	public string ampm;
	public string year;
	public string timeDiff;

	public void Reset()
	{
		if (ex == null)
			ex = GetComponent<KMDateTimeOverrides>();
		ex.Reset();
		day = ex.day;
		dayAbbr = ex.dayAbbr;
		dayName = ex.dayName;
		hour = ex.hour;
		hourGlobal = ex.hourGlobal;
		minutes = ex.minutes;
		month = ex.month;
		monthAbbr = ex.monthAbbr;
		monthName = ex.monthName;
		seconds = ex.seconds;
		ampm = ex.ampm;
		year = ex.year;
		timeDiff = ex.timeDiff;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(DevDateTimeExtensions))]
public class Updater : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if (GUILayout.Button("Update!"))
		{
			((DevDateTimeExtensions)target).Reset();
		}
	}
}
#endif