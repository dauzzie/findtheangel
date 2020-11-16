using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour {
    float smooth = 2f;
    float openAngle = -150f;
    float closeAngle = 0f;

    private Quaternion doorOpenAngle;
    private Quaternion doorCloseAngle;
    bool isOpen;
    bool canEnter;

    // Initialise any variables we will use later. Rather than creating them each frame in update.
    void Start () {
        doorOpenAngle = Quaternion.Euler (0, openAngle, 0); // set the properties of the open angle
        doorCloseAngle = Quaternion.Euler (0, closeAngle, 0);

        isOpen = false; // always initialise booleans to true/false. This avoids logical crashes when comparing an uninitialised variable.
        canEnter = false;
    }

    // Call non essential updates on LateUpdate (ie. enviroment changes)
    void Update () {
        // By calculating the smoothTime outside of the if statement, we only need to calculate it in one place in memory.
        var smoothTime = Time.deltaTime * smooth; // this is a LOCAL variable. It can only be accessed by LateUpdate().

        if (isOpen == true) {
            // Dampen towards the target rotation
            transform.localRotation = Quaternion.Slerp (transform.localRotation, doorOpenAngle,
                smoothTime);
        }

        if (isOpen == false) {
            // Dampen towards the target rotation
            transform.localRotation = Quaternion.Slerp (transform.localRotation, doorCloseAngle,
                smoothTime / 5);
        }

        // If the player can enter the door AND the player has pressed 'e'
        // use  (And), || (Or) to combine comparison checks
        if (canEnter == true && Input.GetKeyDown ("e")) {
            isOpen = !isOpen;
        }
    }

    // When the collider has entered the trigger..
    void OnTriggerEnter (Collider other) {
        //Debug.Log("Hit Door");
        // ..and the player tag is correct..
        if (other.gameObject.tag == "Player") {
            // ..allow entry
            canEnter = true;
            // return from this level
            return;
        }
    }

    // When the collider has exited the trigger..
    void OnTriggerExit (Collider other) {
        // ..disable the ability to enter
        canEnter = false;

        // wait and close
        isOpen = false;

    }
}