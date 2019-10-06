using UnityEngine;

public class EventTrigger : MonoBehaviour {
    public string targetEvent = "";

    private void OnTriggerEnter(Collider other) {
        UIController.LoadContextStory(targetEvent);
        //Debug.Log("ENTERED " + other.name);
    }

    private void OnTriggerExit(Collider other) {
        UIController.DeleteContextStory();
        //Debug.Log("LEFT " + other.name);
    }
}
