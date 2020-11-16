using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightLerp : MonoBehaviour
{
    // Start is called before the first frame update

    public float maxRange = 50f;
    public float minRange = 0f;
    public float flickerSpeed = 0.3f;

    public GameObject light;

    Light lt;
    void Start()
    {
        lt = light.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        lt.intensity = Mathf.Lerp(minRange, maxRange, Mathf.PingPong(Time.time, flickerSpeed));
    }
}
