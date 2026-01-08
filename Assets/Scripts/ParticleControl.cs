using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleControl : MonoBehaviour
{
    
    public ParticleSystem pSystem;
    public Text controlParticle;

    public void ParticleToggle()
    {
        AudioSource ghostWisper = GameObject.Find("Area Ghost Generator").GetComponent<AudioSource>();

        if (pSystem.isPaused == true || pSystem.isStopped == true)
        {
            pSystem.Play();
            ghostWisper.Play();
            controlParticle.text = "Ghost stop";
        } else
        {
            //pSystem.Pause();
            pSystem.Stop();
            pSystem.Clear();
            ghostWisper.Stop();
            controlParticle.text = "Ghost play";
        }
    }
}