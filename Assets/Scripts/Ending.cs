using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ending : MonoBehaviour
{
    // Start is called before the first frame update

    float secondsLeft = 0;
    void Start()
    {
        StartCoroutine(DelayLoadLevel(60));
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator DelayLoadLevel(float seconds)
    {
        secondsLeft = seconds;

        do
        {
            yield return new WaitForSeconds(1);
        } while (--secondsLeft > 0);
        SceneManager.LoadScene(0);
    }
}
