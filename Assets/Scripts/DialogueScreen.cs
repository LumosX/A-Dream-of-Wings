using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueScreen : MonoBehaviour {

    public Text dialogueTextbox;
    public Button[] buttons;
    public Text[] buttonTextBoxes;

    public Action<int> listenerCallback = null;


    public void SetDialogueInfo(string text, List<string> choices) {
        dialogueTextbox.text = text;
        
        for (var i = 0; i < 4; i++) {
            if (i < choices.Count) {
                buttons[i].gameObject.SetActive(true);
                buttonTextBoxes[i].text = $"{i + 1}. {choices[i]}";
            }
            else {
                buttons[i].gameObject.SetActive(false);
                buttonTextBoxes[i].text = "";
            }
        }
    }

    public void OnClick_DialogueButton(int optionSelected) {
        // NOTE: optionSelected is ZERO-BASED like everything else.
        listenerCallback?.Invoke(optionSelected);
    }
    
}