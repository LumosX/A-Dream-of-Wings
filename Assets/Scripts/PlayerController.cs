using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {
    // HP and XP
    public int curHP = 10;
    public int maxHP = 10;
    public int creds = 0;
    public int curXP = 0; // Extra attribute points are gained every 10 XP

    public int attrPts = 1;
    public int strength = 1;
    public int endurance = 1;
    public int charisma = 1;

    public Talent talent = Talent.None; 

    public enum Talent {
        None,
        Pilot,
        Witty,
        Marksman
    }
    
    
    public int numDaysPassed = 0;

    public static PlayerController instance = null;


    // Start is called before the first frame update
    void Awake() {
        if (instance == null) instance = this;
    }

    // Update is called once per frame
    void Update() { }


    private void OnTriggerEnter(Collider other) {
        Debug.Log("ENTERED " + other.name);
    }
    
    private void OnTriggerExit(Collider other) {
        Debug.Log("LEFT " + other.name);
    }
}


public static class DateStuff {
    // I'll just shove these here because who cares
    public const int STARTING_DAY = 14;
    public const int STARTING_MONTH = 10;
    public const int STARTING_YEAR = 3279;

    private static int absoluteStartingDay => (STARTING_YEAR - 1) * MONTHS_PER_YEAR * DAYS_PER_MONTH +
                                              (STARTING_MONTH - 1) * DAYS_PER_MONTH +
                                              (STARTING_DAY - 1);

    public const int DAYS_PER_MONTH = 22;
    public const int MONTHS_PER_YEAR = 10;

    private static int daysPerYear = DAYS_PER_MONTH * MONTHS_PER_YEAR;

    // tuples are pretty nifty, aren't they
    public static (int, int, int) GetDateFormat(int numDaysPassed) {
        var absoluteDay = absoluteStartingDay + numDaysPassed;
    
        var curYear = absoluteDay / daysPerYear;
        var curMonth = (absoluteDay - daysPerYear * curYear) / DAYS_PER_MONTH;
        var curDay = (absoluteDay % daysPerYear) % DAYS_PER_MONTH;
        
        return (curDay + 1, curMonth + 1, curYear + 1);
    }
}