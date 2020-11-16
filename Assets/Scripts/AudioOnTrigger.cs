using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOnTrigger : MonoBehaviour {
    public GameObject trigger;
    public AudioSource audioClip;

    void OnTriggerEnter (Collider collider) {
        if (collider.tag == "Player") {
            trigger.SetActive (true);
            audioClip.Play ();
        }
    }
}