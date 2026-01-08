using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class PartCollision : MonoBehaviour
{
    public ParticleSystem part;
    public List<ParticleCollisionEvent> collisionEvents;

    public GameObject sword;
    public GameObject player;
    public AudioSource particleDeath;
    public FinalFade fade;

    void Start()
    {
        if (SceneChanger.scene == 3)
        {
            part = GetComponent<ParticleSystem>();
            if (part.isPaused == false || part.isStopped == false)
            {
                part.Stop();
            }
            collisionEvents = new List<ParticleCollisionEvent>();
            particleDeath = GetComponent<AudioSource>();

            fade = GameObject.Find("Finale").GetComponent<FinalFade>();
        }
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        //Rigidbody rb = other.GetComponent<Rigidbody>();
        int i = 0;

        while (i < numCollisionEvents)
        {
            //Debug.Log(i);
            if (other == player)
            {
                //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                fade.enableAuto();
                Debug.Log("COLLISION!!!");
            } else if (other == sword)
            {
                Debug.Log("KILL PARTICLE!!!!!");
                particleDeath.Play();
            }
            i++;
        }
    }
}