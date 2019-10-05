using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public RectTransform charScreen = null;
    public RectTransform infoPanel = null;
    public RectTransform dialogueScreen = null;
    public RectTransform contextPanel = null;

    public static UIController Instance { get; private set; }

    private static bool charScreenActive = false;
    private static bool dialogueScreenActive = false;
    private static bool contextPanelActive = false;
    
    void Start() { // not Awake to give the player controller time to hook itself up
        if (Instance == null) Instance = this;
        TriggerUIUpdate();
    }

    // This handles screen changes and shit
    void Update() {

        // Can't open char screen in dialogues
        if (!dialogueScreenActive) {
            // toggle char screen
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape)) ToggleCharScreen();
        }
        // IF IN DIALOGUES:
        else {
            
        }

        
        
    }


    private void ToggleCharScreen() {
        charScreenActive = !charScreenActive;
        // turn the actual panel on/off too
        if (charScreen != null) charScreen.gameObject.SetActive(charScreenActive);
        else Debug.LogError("CHAR SCREEN NOT LINKED");
    }
    
    private void ToggleDialogueScreen() {
        dialogueScreenActive = !dialogueScreenActive;
        // turn the actual panel on/off too
        if (dialogueScreen != null) dialogueScreen.gameObject.SetActive(dialogueScreenActive);
        else Debug.LogError("DIALOGUE SCREEN NOT LINKED");
    }

    private void ShowContextPanel(string title, string description) {
        // Set the text for the panel
        
    }

    public static void TriggerUIUpdate() {
        var player = PlayerController.instance;
        var (day, month, year) = DateStuff.GetDateFormat(player.numDaysPassed);

        // this is damn SEXY!
        string GetAttrStr(int level) => new string('■', level) + new string('□', 5 - level);
        
        // INFO PANEL
        if (Instance.infoPanel != null) {
            Instance.infoPanel.Find("Panel/Info bar").GetComponent<Text>().text =
                $"{day}/{month}/{year} - HP {player.curHP}/{player.maxHP} - ֎ {player.creds} - ■ {player.attrPts}";
        }
        
        // CHAR SCREEN
        if (Instance.charScreen != null) {
            Instance.charScreen.Find("Panel/Attr Title/Values").GetComponent<Text>().text =
                $"{GetAttrStr(player.strength)}\n{GetAttrStr(player.endurance)}\n{GetAttrStr(player.charisma)}\n■ {player.attrPts}";
            
            Instance.charScreen.Find("Panel/Vitals Title/Values").GetComponent<Text>().text =
                $"{player.curHP}/{player.maxHP}\n֎ {player.creds}\n{player.curXP}/10\n{player.talent}";

            Instance.charScreen.Find("Panel/Time Title/Values").GetComponent<Text>().text =
                            $"{day}/{DateStuff.DAYS_PER_MONTH}\n{month}/{DateStuff.MONTHS_PER_YEAR}\n{year}\n{player.numDaysPassed}";
        }
        
    }


    public void OnClick_Dismiss() {
        ToggleCharScreen();
    }
    
    public void OnClick_ExitNoSave() {
        Application.Quit();
    }
    
    public void OnClick_SaveAndExit() {
        
    }

}