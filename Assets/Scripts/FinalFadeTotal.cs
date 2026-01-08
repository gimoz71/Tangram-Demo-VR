using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aura2API;

public class FinalFadeTotal : MonoBehaviour
{
    //public AuraBaseSettings setFade;
   /* public AuraVolume globalVolume;

    public float min = 0.025f;
    public float max = 2;
    public float startValue = 0.025f;
    public float changePerSecond = 0.1f;
    public bool triggerEnd = false;*/

    /*public FinalFade fade;


    public float finalFadeDelay = 6.0f;*/

    public GameObject fraseFinale;
    public AudioSource FinalPhraseAudio;

    public AudioClip clipFinalPhrase;
    public AudioClip clipFlash;
    public ParticleSystem FinalPhraseSparkle;

    //private IEnumerator coroutine;

    // Start is called before the first frame update
    void Start()
    {
        fraseFinale.SetActive(false);
        FinalPhraseSparkle.GetComponent<ParticleSystem>().Stop();
        //fade = GameObject.Find("Finale").GetComponent<FinalFade>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("VALUE :" + startValue);
        /*if (triggerEnd)
        {
            globalVolume.densityInjection.strength = startValue;
            globalVolume.lightInjection.injectionParameters.strength = startValue;
            startValue += changePerSecond;
        }*/
    }
    
    public void enableFinalScene()
    {
        fraseFinale.SetActive(true);
        //GameObject fraseFinale = GameObject.Find("Frase Finale");
        if (FinalPhraseAudio)
        {
            FinalPhraseAudio.PlayOneShot(clipFinalPhrase, 1f);
        }
        if (FinalPhraseSparkle)
        {
            FinalPhraseSparkle.GetComponent<ParticleSystem>().Play();
        }
       
        /*coroutine = WaitAndPrint(finalFadeDelay);
        StartCoroutine(coroutine);*/
    }

    /*
    private IEnumerator WaitAndPrint(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            print("WaitAndPrint " + Time.time);
           // triggerEnd = true;
            FinalPhraseAudio.PlayOneShot(clipFlash, 1f);
            //fade.enableAuto();
        }
    }*/
}
