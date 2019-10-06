using UnityEngine;

public class TownMove : MonoBehaviour {
    public float speed = 10;
    
    private Vector2 movementVect = Vector2.zero;
    private Rigidbody rb;

    // Update is called once per frame
    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    
    
    // NB: This really doesn't need to be here or look like this, but it's the first script I wrote, so it remains on its own.

    void Update() {
        // Just do input here.
        float inputX = 0;
        float inputZ = 0;
        if (Input.GetKey(KeyCode.W)) inputZ -= 1; // my axes are all over hte place
        if (Input.GetKey(KeyCode.S)) inputZ += 1;
        if (Input.GetKey(KeyCode.A)) inputX += 1;
        if (Input.GetKey(KeyCode.D)) inputX -= 1;
        
        // This only works when not in dialogues or the char screen (or when not dead), so overwrite input otherwise.
        if (UIController.CharScreenActive || UIController.DialogueScreenActive || PlayerController.PlayerDead)
            inputX = inputZ = 0;


        // Make a gradual decrease in movement once the key is released; "slipperiness" if you will
        if (Mathf.Abs(inputX) > Mathf.Abs(movementVect.x)) movementVect.x = inputX;
        else movementVect.x *= 0.95f;
        if (Mathf.Abs(inputZ) > Mathf.Abs(movementVect.y)) movementVect.y = inputZ;
        else movementVect.y *= 0.95f;

        movementVect = Vector2.ClampMagnitude(movementVect, 1);
    }
    
    // Do the actual movement here, because we're doing RB stuff.
    void FixedUpdate() {
        //transform.Translate(new Vector3(movementVect.x, 0, movementVect.y));
        rb.ResetInertiaTensor();
        rb.velocity = Vector2.zero;
        rb.MovePosition(transform.position + speed * Time.deltaTime * new Vector3(movementVect.x, 0, movementVect.y));
    }
    
}


