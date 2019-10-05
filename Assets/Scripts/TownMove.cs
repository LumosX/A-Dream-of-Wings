﻿using System.Collections;
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

        // Make a gradual decrease in movement once the key is released; "slipperiness" if you will
        if (Mathf.Abs(inputX) > Mathf.Abs(movementVect.x)) movementVect.x = inputX;
        else movementVect.x *= 0.95f;
        if (Mathf.Abs(inputZ) > Mathf.Abs(movementVect.y)) movementVect.y = inputZ;
        else movementVect.y *= 0.95f;

        movementVect = Vector2.ClampMagnitude(movementVect, 1);

        //transform.Translate(new Vector3(movementVect.x, 0, movementVect.y));
        rb.ResetInertiaTensor();
        rb.velocity = Vector2.zero;
        rb.MovePosition(transform.position + speed * Time.deltaTime * new Vector3(movementVect.x, 0, movementVect.y));
    }
}
