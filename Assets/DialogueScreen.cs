using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScreen : MonoBehaviour {

    public Text dialogueTextbox;
    public Button[] buttons;
    public Text[] buttonTextBoxes;


    public void SetDialogueInfo(string text, params string[] choices) {
        dialogueTextbox.text = text;
        
        for (var i = 0; i < 4; i++) {
            if (i < choices.Length) {
                buttons[i].gameObject.SetActive(true);
                buttonTextBoxes[i].text = choices[i];
            }
            else {
                buttons[i].gameObject.SetActive(false);
                buttonTextBoxes[i].text = "";
            }
        }
    }

    public void OnClick_DialogueButton(int optionSelected) {
        // NOTE: optionSelected is ONE-BASED, NOT ZERO-BASED!
        
    }
    
}