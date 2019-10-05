using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownMove : MonoBehaviour {
    public float speed = 10;
    
    private Vector2 movementVect = Vector2.zero;
    private Rigidbody rb;

    // Update is called once per frame
    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        // Just do input here.
        float inputX = 0;
        float inputZ = 0;
        if (Input.GetKey(KeyCode.W)) inputZ -= 1; // my axes are all over hte place
        if (Input.GetKey(KeyCode.S)) inputZ += 1;
        if (Input.GetKey(KeyCode.A)) inputX += 1;
        if (Input.GetKey(KeyCode.D)) inputX -= 1;

        // Bounds
        if (inputX > movementVect.x) movementVect.x = inputX;

        movementVect = inputVector;
        //transform.Translate(new Vector3(movementVect.x, 0, movementVect.y));
        rb.MovePosition(transform.position + speed * Time.deltaTime * new Vector3(movementVect.x, 0, movementVect.y));
    }
}
