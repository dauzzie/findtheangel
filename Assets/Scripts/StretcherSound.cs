using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class StretcherSound : MonoBehaviour
{
    // Start is called before the first frame updat

    // Update is called once per frame

    AudioSource audioData;
    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (transform.hasChanged)
        {
            audioData.Play(0);
        }
    }
}
