using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swash : MonoBehaviour
{
    public AudioSource swoshAudio;
    public ParticleSystem splash;
    public int StoneCounter;

    void Start()
    {
        splash.GetComponent<ParticleSystem>().Stop();
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "itemChild")
        {
            swoshAudio.Play();
            splash.GetComponent<ParticleSystem>().Play();
        }
    }
}
