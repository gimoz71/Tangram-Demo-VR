using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using Aura2API;

public class FinalFade : MonoBehaviour
{
    //public AuraBaseSettings setFade;
    public AuraVolume globalVolume;

    public float min = 0.025f;
    public float max = 2;
    public float startValue = 0.025f;
    public float changePerSecond = 0.1f;
    public bool triggerEnd = false;

    public float finalDelay = 4.0f;

    public AudioSource audioFlash;
    public AudioClip clipFlash;

    private IEnumerator coroutine;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("VALUE :" + startValue);
        if (triggerEnd)
        {
            globalVolume.densityInjection.strength = startValue;
            globalVolume.lightInjection.injectionParameters.strength = startValue;
            startValue += changePerSecond;
        }
    }

    public void enable()
    {
        triggerEnd = true;
        audioFlash.PlayOneShot(clipFlash, 1f);
    }


    public void enableAuto()
    {
        coroutine = WaitAndPrint(finalDelay);
        StartCoroutine(coroutine);
    }


    private IEnumerator WaitAndPrint(float waitTime)
    {
        while (true)
        {
            triggerEnd = true;
            audioFlash.PlayOneShot(clipFlash, 1f);
            yield return new WaitForSeconds(waitTime);
            print("WaitAndPrint " + Time.time);
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }


}
