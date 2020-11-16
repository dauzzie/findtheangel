using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test3LightMess : MonoBehaviour
{
    public GameObject light;

    float secondsLeft = 0;

    private bool _isTrigger = false;

    private bool oneLoop = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && _isTrigger == false)
        {
            StartCoroutine(DelayLoadLevel(15));
        }
    }

    IEnumerator DelayLoadLevel(float seconds)
    {
        secondsLeft = seconds;

        do
        {
            if (oneLoop)
            {
                oneLoop = false;
                light.SetActive(true);

            }
            else
            {
                oneLoop = true;
                light.SetActive(false);
            }
            _isTrigger = true;
            yield return new WaitForSeconds(1);
        } while (--secondsLeft > 0);
        _isTrigger = false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
