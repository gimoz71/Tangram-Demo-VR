using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VREnabler : MonoBehaviour
{


    public bool vrOn;

    void Update()
    {
        XRSettings.enabled = vrOn;
    }
    
}
