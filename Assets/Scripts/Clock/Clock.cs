using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField]private Transform hoursPivot;
    [SerializeField]private Transform minutesPivot;
    [SerializeField]private Transform secondsPivot;
    private const float HoursToDegrees = 30f;
    private const float MinutesToDegrees = 6f;
    private const float SecondsToDegrees = 6f;
    private void Update()
    {
        var nowTime = DateTime.Now.TimeOfDay;
        hoursPivot.transform.localRotation = Quaternion.Euler(0, HoursToDegrees * (float)nowTime.TotalHours, 0);
        minutesPivot.transform.localRotation = Quaternion.Euler(0, MinutesToDegrees * (float)nowTime.TotalMinutes, 0);
        secondsPivot.transform.localRotation = Quaternion.Euler(0, SecondsToDegrees * (float)nowTime.TotalSeconds, 0);
    }
}
