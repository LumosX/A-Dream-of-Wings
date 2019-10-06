using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    public RectTransform charScreen = null;
    public RectTransform infoPanel = null;
    public RectTransform dialogueScreen = null;
    public RectTransform contextPanel = null;
    public RectTransform deathPanel = null;

    public AudioController audioController;
    
    private static UIController Instance { get; set; }

    public static bool CharScreenActive { get; private set; }
    public static bool DialogueScreenActive { get; private set; }
    public static bool ContextPanelActive { get; private set; }


    private DialogueScreen dialogueScreenScript = null;
    private Story contextActivatableStory = null; // This is "loaded" whilst the player is inside a "story" trigger.
    private List<Choice> lastSentChoices = null; // This holds all choices we SENT to the actual dialogue screen, in order.

    // SPAGHETTIIIIII
    public static AudioController GetAudioController() {
        return Instance.audioController;
    }
    
    void Start() { // not Awake to give the player controller time to hook itself up
        if (Instance == null) Instance = this;
        TriggerUIUpdate();

        dialogueScreenScript = dialogueScreen.GetComponent<DialogueScreen>();
        dialogueScreenScript.listenerCallback = OnDialogueChoiceSelectedCallback;
        
        
        deathPanel.gameObject.SetActive(false);
        charScreen.gameObject.SetActive(false);
        infoPanel.gameObject.SetActive(true);
        
        
        // Audio: start a track and the rain sound
        audioController.ToggleRainNoise(true);        
        audioController.ToggleGuitarTrack(true);        
        
        
    }

    // This handles screen changes and shit
    void Update() {
        // DEATH
        if (!DialogueScreenActive && PlayerController.PlayerDead) {
            ToggleCharScreen(false);
            ToggleContextPanel(false);
            ToggleDialogueScreen(false);
            
            deathPanel.gameObject.SetActive(true);
            deathPanel.Find("Panel/Death info").GetComponent<Text>().text =
                PlayerController.Instance.livedAtTheEnd
                ? "You survived. Whilst this means you did not die a noble death, it's likely a better result."
                : PlayerController.Instance.wasDraftedLate
                    ? "You died a noble death, trying to repel invading forces. You did well, and may rest at peace."
                    : PlayerController.Instance.wasPilot
                        ? "You died a glorious death, piloting a craft against the foes of the Scriptorium. " +
                          "Your sacrifice will be remembered."
                        : PlayerController.Instance.wasSoldier
                            ? "You died the death of a soldier, killed in the line of duty. There are worse ways to go."
                            : "You didn't even manage to join the army. Might want to try harder next time.";
        }
        
        
        
        // Show/hide context panel if need be
        if (!DialogueScreenActive && !CharScreenActive && contextActivatableStory != null && !PlayerController.PlayerDead) {
            ToggleContextPanel(true);
        }
        else ToggleContextPanel(false);
        
        
        // Can't open char screen in dialogues
        if (!DialogueScreenActive) {
            // toggle char screen
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Escape)) ToggleCharScreen(!CharScreenActive);
            
            // OR attempt to start a dialogue ("event")
            if (!CharScreenActive && Input.GetKeyDown(KeyCode.E)) StartStory();
        }
        // IF IN DIALOGUES:
        else {
            // Can use 1 2 3 4 to select from sent choices.
            if (Input.GetKeyDown(KeyCode.Alpha1)) OnDialogueChoiceSelectedCallback(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) OnDialogueChoiceSelectedCallback(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) OnDialogueChoiceSelectedCallback(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) OnDialogueChoiceSelectedCallback(3);
        }

        
        
    }

    private void StartStory() {
        //Debug.Log("test 1");
        if (contextActivatableStory == null) return;
        //Debug.Log("test 2");
        // Start story, i.e. trigger first event
        StartEvent(contextActivatableStory.StartingEvent);
        
        Instance.audioController.PlayEffect(AudioController.AudioEffect.StartDialogue);
    }

    private void EndStory() {
        // Hide dialogue screen.
        ToggleDialogueScreen(false);
    }

    private void StartEvent(Event storyEvent) {
        //Debug.Log("test 3");
        if (storyEvent == null) {
            EndStory();
            return;
        }
        
        //Debug.Log("test 4");
        
        // if we do have an event, trigger it: show the dialogue screen and fill it up with choices and such.
        // first, process event to display its result
        var eventResult = storyEvent.TriggerFunction();
        UIController.TriggerUIUpdate(); // just in case...
        
        // check which choices are currently available to the player AND SAVE THESE FOR LATER
        lastSentChoices = storyEvent.Choices.Where(x => x.Conditions()).ToList();
        dialogueScreenScript.SetDialogueInfo(eventResult, lastSentChoices.Select(x => x.Label()).ToList());
        ToggleDialogueScreen(true);
        
        //Debug.Log("test 5");
    }

    // again, options are ZERO-BASED
    private void OnDialogueChoiceSelectedCallback(int optionSelected) {
        // Fail-safe needed for the key-bound dialogue choice selections.
        if (optionSelected >= lastSentChoices.Count) return;

        //Debug.Log("DLG OPTION " + optionSelected + " CHOSEN!");

        lastSentChoices[optionSelected].Effect();
        UIController.TriggerUIUpdate(); // just in case...
        
        Instance.audioController.PlayEffect(AudioController.AudioEffect.SelectDialogueOption);
   
        StartEvent(lastSentChoices[optionSelected].NextEvent);
    }


    private void ToggleCharScreen(bool target) {
        CharScreenActive = target;
        // turn the actual panel on/off too
        charScreen.gameObject.SetActive(CharScreenActive);
    }
    
    public static void ToggleDialogueScreen(bool target) {
        DialogueScreenActive = target;
        // turn the actual panel on/off too
        Instance.dialogueScreen.gameObject.SetActive(DialogueScreenActive);
    }
    
    // This is just for the UI component;
    private void ToggleContextPanel(bool target) {
        if (ContextPanelActive == target) return;
        Instance.contextPanel.gameObject.SetActive(target);
        ContextPanelActive = target;
    }

    public static void LoadContextStory(string storyTag) {
        // abort if tag empty or fraudulent
        if (storyTag == "") return;
        if (!Storyteller.Storybook.ContainsKey(storyTag)) return;

        // Only set this. The Update function will take care of the UI as long as it is set.
        Instance.contextActivatableStory = Storyteller.Storybook[storyTag];
        
        // Update the text from the given "story" that's been loaded
        Instance.contextPanel.Find("Panel/Title").GetComponent<Text>().text =
            Instance.contextActivatableStory.ContextTitle;
        Instance.contextPanel.Find("Panel/Context info").GetComponent<Text>().text =
            Instance.contextActivatableStory.ContextDescription;
    }

    // The system assumes the player is ONLY EVER inside ONE trigger volume.
    public static void DeleteContextStory() {
        Instance.contextActivatableStory = null;
    }


    public static void TriggerUIUpdate() {
        var player = PlayerController.Instance;
        var (day, month, year) = DateStuff.GetDateFormat(player.numDaysPassed);

        // this is damn SEXY!
        string GetAttrStr(int level) => new string('■', level) + new string('□', 5 - level);
        
        // INFO PANEL
        if (Instance.infoPanel != null) {
            Instance.infoPanel.Find("Panel/Info bar").GetComponent<Text>().text =
                $"Day {player.numDaysPassed + 1} - {day}/{month}/{year} - HP {player.CurHP}/{player.MaxHP}" + 
                $" - ֎ {player.creds} - ■ {player.attrPts} ({player.CurXP}/10)";
        }
        
        // CHAR SCREEN
        if (Instance.charScreen != null) {
            Instance.charScreen.Find("Panel/Attr Title/Values").GetComponent<Text>().text =
                $"{GetAttrStr(player.strength)}\n{GetAttrStr(player.endurance)}\n{GetAttrStr(player.charisma)}\n■ {player.attrPts}";

            var talent = "---";
            switch (player.talent) {
                case PlayerController.Talent.Pilot:
                    talent = "PILOT DAD";
                    break;
                case PlayerController.Talent.Witty:
                    talent = "WITTY";
                    break;
                case PlayerController.Talent.Marksman:
                    talent = "MARKSMAN";
                    break;
            }
            
            Instance.charScreen.Find("Panel/Vitals Title/Values").GetComponent<Text>().text =
                $"{player.CurHP}/{player.MaxHP}\n֎ {player.creds}\n{player.CurXP}/10\n{talent}";

            Instance.charScreen.Find("Panel/Time Title/Values").GetComponent<Text>().text =
                $"{day}/{DateStuff.DAYS_PER_MONTH}\n{month}/{DateStuff.MONTHS_PER_YEAR}\n{year}\n{player.numDaysPassed}";
        }
    }

    public void OnClick_Dismiss() {
        ToggleCharScreen(false);
    }
    
    public void OnClick_ExitNoSave() {
        SceneManager.LoadScene("MainMenuScene");
    }
    
    public void OnClick_SaveAndExit() {
        // TODO: SAVING AND LOADING - NOT TODAY
        // This function is being appropriated for audio mute
        audioController.ToggleGuitarMute(!audioController.GuitarMuted);
    }

    public void OnClick_Restart() {
        SceneManager.LoadScene("GameScene");
    }
    

}