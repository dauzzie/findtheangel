using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]

public class Test2Sound : MonoBehaviour
{
    // Start is called before the first frame update
    AudioSource audioData;
    float secondsLeft = 0;
    bool isPlaying;
    bool oneLoop;
    void Start()
    {
        audioData = GetComponent<AudioSource>();
        isPlaying = false;
        oneLoop = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && isPlaying == false & oneLoop == false)
        {
            audioData.Play(0);
            isPlaying = true;
            StartCoroutine(DelayLoadLevel(15));
        }
    }

    private void OnTriggerExit(Collider other)
    {

    }

    IEnumerator DelayLoadLevel(float seconds)
    {
        secondsLeft = seconds;

        do
        {
            oneLoop = true;
            yield return new WaitForSeconds(1);
        } while (--secondsLeft > 0);
        isPlaying = false;
        oneLoop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (oneLoop == true)
        {
            isPlaying = true;
        }
        else
        {
            isPlaying = false;
        }
    }
}