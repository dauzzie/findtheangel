using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Crawler : MonoBehaviour
{
    // Start is called before the first frame update
    private bool _isInsideTrigger = false;

    public Animator _animator;
    float secondsLeft = 0;
    // Update is called once per frame
    void Update()
    {
        if (_isInsideTrigger)
        {
            _animator.SetBool("isIn", true);
            StartCoroutine(DelayLoadLevel(15));
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _isInsideTrigger = true;
        }
    }
}
