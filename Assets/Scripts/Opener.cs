using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opener : MonoBehaviour
{


    private bool _isInsideTrigger = false;

    public Animator _animator;

    public OVRInput.Button doorButton;

    public OVRInput.Controller controller;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            _isInsideTrigger = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _isInsideTrigger = false;
            _animator.SetBool("open", false);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (_isInsideTrigger)
        {
            if (OVRInput.Get(doorButton, controller))
            {
                _animator.SetBool("open", true);
            }
        }
    }
}
