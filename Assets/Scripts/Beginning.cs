using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Beginning : MonoBehaviour
{
    // Start is called before the first frame update

    public OVRInput.Button doorButton;

    public OVRInput.Controller controller;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.Get(doorButton, controller))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            //SceneManager.LoadScene(0);
        }
    }
}
