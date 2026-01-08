using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigItem : MonoBehaviour
{

    //public GameObject item;
    private GameObject itemChild;
    private GameObject markerChild;
    private MeshRenderer itemChildRenderer;
    private int picconataCounter;

    public ParticleSystem scintille;
    //public Texture symbolTexture;
    public AudioSource hitSound;

    // Start is called before the first frame update
    void Start()
    {
        itemChild = transform.Find("item").gameObject;
        markerChild = transform.Find("marker").gameObject;

        scintille.GetComponent<ParticleSystem>().Stop();

        //itemChild.GetComponent<Renderer>().material.mainTexture = symbolTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.name == "Piccone")
        {
            picconataCounter++;
            Debug.Log(picconataCounter);

            if (picconataCounter == 8) {
                itemChild.SetActive(true);
                markerChild.SetActive(false);
            }
            if (picconataCounter < 8)
            {
                hitSound.Play();
                Explode();
            }
        }
    }

    void Explode()
    {
        scintille.GetComponent<ParticleSystem>().Play();
    }
}
