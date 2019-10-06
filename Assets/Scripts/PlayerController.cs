using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

public class PlayerController : MonoBehaviour {
    // HP and XP

    private int curHP = 10;
    public int CurHP {
        get => curHP;
        // if Cur + added > Max, revert to max
        set => curHP = value > MaxHP ? MaxHP : value;
    }

    public static bool PlayerDead => PlayerController.Instance.curHP <= 0;

    public int MaxHP => 5 + endurance * 5;
    public int creds = 0;

    private int curXP = 0; // Extra attribute points are gained every 10 XP
    public int CurXP {
        get => curXP;
        set {
            if (value == 10) {
                attrPts += 1;
                curXP = 0;
            }
            else curXP = value;
        }
    }

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

    public int warState = 0; // the greater this is, the worse things are.

    public bool wasDraftedLate = false;
    public bool wasSoldier;
    public bool wasPilot = false;
    public bool livedAtTheEnd = false;

    public static PlayerController Instance = null;


    // Start is called before the first frame update
    void Awake() {
        if (Instance == null) Instance = this;
    }

    // Update is called once per frame
    void Update() { }
    
    
    
    
    ////////////////// THESE THINGS NEED TO BE EXTRACTED FROM HERE BUT I DON'T GIVE A FUUUUUUUUUUCK
    public static readonly Random RNG = new Random();
    
    // string arg because I can't be arsed
    public static AttrCheckResult AttributeCheck(string attribute) {
        // Roll 1d5, success if rolled equal or under the selected attribute.
        var roll = RNG.Next(1, 6);
        var attrVal = attribute == "STR"
            ? Instance.strength
            : attribute == "END"
                ? Instance.endurance
                : Instance.charisma;
        // Is it a success or a failure? Is it critical?
        // "Critical" is when the difference between the two is 3 or more.
        if (roll <= attrVal && Mathf.Abs(attrVal - roll) >= 3) return AttrCheckResult.CritSuccess;
        if (roll <= attrVal) return AttrCheckResult.Success;
        if (roll > attrVal && Mathf.Abs(attrVal - roll) >= 3) return AttrCheckResult.CritFail;
        return AttrCheckResult.Fail;
    }

    public enum AttrCheckResult {
        Success, Fail, CritSuccess, CritFail
    }
    
}