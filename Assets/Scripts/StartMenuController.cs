using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour {
    public RectTransform panelPreClick;
    public RectTransform panelPostClick;
    public Text hoverTextbox;

    // Start is called before the first frame update
    void Start() {
        panelPreClick.gameObject.SetActive(true);
        panelPostClick.gameObject.SetActive(false);
    }

    public void OnClick_StartGame() {
        // turn the panels around
        panelPreClick.gameObject.SetActive(false);
        panelPostClick.gameObject.SetActive(true);
    }

    public void OnClick_QuitGame() {
        Application.Quit();
    }

    public void OnClick_TalentNone() {
        NewGameDataCarrier.ChosenTalent = PlayerController.Talent.None;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClick_TalentPilot() {
        NewGameDataCarrier.ChosenTalent = PlayerController.Talent.Pilot;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClick_TalentMarksman() {
        NewGameDataCarrier.ChosenTalent = PlayerController.Talent.Marksman;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClick_TalentWitty() {
        NewGameDataCarrier.ChosenTalent = PlayerController.Talent.Witty;
        SceneManager.LoadScene("GameScene");
    }
    
    
    // this is dumb, but fuck it
    public void OnEnter_TalentNone() {
        hoverTextbox.text = "You have no special talents. You truly start with nothing.\n(You don't even get an Attribute point.)";
    }

    public void OnEnter_TalentPilot() {
        hoverTextbox.text = "Your dad was a pilot and a war hero. It doesn't sound like much of a personal talent, but you'd be " +
                            "surprised.\nYou start with one Attribute point, as well as an innate talent for piloting.";
    }

    public void OnEnter_TalentMarksman() {
        hoverTextbox.text = "You used to participate in marksmanship contests back in the day.\n" +
                            "You start with one Attribute point, +1 to Strength, and a mostly irrelevant marksmanship skill.";
    }

    public void OnEnter_TalentWitty() {
        hoverTextbox.text = "You're naturally witty and charismatic, and tend to get along with people well.\n" +
                            "You start with one Attrbiute point and +1 to Charisma.";
    }

    public void OnLeave_Any() {
        hoverTextbox.text = "Hover over one of the options for more information.";
    }
}