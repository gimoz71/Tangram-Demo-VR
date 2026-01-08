using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighLighterMask : MonoBehaviour
{
    private GameObject highLighterObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        highLighterObj = GameObject.Find("FilterHolder");
        if (highLighterObj)
        {
            highLighterObj.layer = LayerMask.NameToLayer("internal");
        }

            

    }
}
