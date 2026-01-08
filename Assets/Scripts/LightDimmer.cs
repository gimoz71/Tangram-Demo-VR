using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDimmer : MonoBehaviour
{
    Light lights;
    //public float dimmerValue;
    static public float value;
    // Start is called before the first frame update
    void Start()
    {
        //Invoke("ExecuteDimmer", 2);
        lights = GetComponent<Light>();
        lights.intensity = lights.intensity - lights.intensity / value;
        Debug.Log("Light intensity: " + lights.intensity);
    }

    void ExecuteDimmer()
    {
        lights = GetComponent<Light>();
        lights.intensity = lights.intensity - lights.intensity / value;
        Debug.Log("Light intensity: " + lights.intensity);
    }
}
