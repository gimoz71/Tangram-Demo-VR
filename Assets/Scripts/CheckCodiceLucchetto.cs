using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckCodiceLucchetto : MonoBehaviour
{

    public TextMeshPro codice1;
    public TextMeshPro codice2;
    public TextMeshPro codice3;
    public TextMeshPro codice4;
    public TextMeshPro codice5;

    private string codice;

    public GameObject treasure;
    public ParticleSystem pSystem;
    private Animation treasureAnimation;
    public Text risultato;


    private AudioSource eventSound;
    public AudioClip errorSound;
    public AudioClip successSound;

    // Start is called before the first frame update
    void Start()
    {
        eventSound = GetComponent<AudioSource>();
        var configManager = JSonConfigManager.Instance;
        if (SceneChanger.team == 1)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathA);
        }
        if (SceneChanger.team == 2)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathB);
        }
        codice = configManager.getCombinazioneLucchetto();
        treasureAnimation = treasure.GetComponent<Animation>();
        //codice = "12345";
        Debug.Log("Codice: " + configManager.getCombinazioneLucchetto());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void CodiceCheck()
    {
        risultato.text = string.Concat(codice1.text, codice2.text, codice3.text, codice4.text, codice5.text);

        if(risultato.text == codice)
        {
            Debug.Log("BRAVO!!!!!!!!");
            risultato.text = risultato.text + " OK!";
            treasureAnimation.Play();
            GameObject pannelloLucchetto = GameObject.Find("Panello Codice");
            Collider chiave = GameObject.Find("chiave").GetComponent<Collider>();
            pannelloLucchetto.SetActive(false);
            chiave.enabled = false;
            eventSound.PlayOneShot(successSound, 1f);
            pSystem.Stop();
            pSystem.Clear();
            AudioSource ghostWisper = GameObject.Find("Area Ghost Generator").GetComponent<AudioSource>();
            ghostWisper.Stop();
        } else
        {
            Debug.Log("ERRORE!!");
            risultato.text = risultato.text + " NO!!!";
            eventSound.PlayOneShot(errorSound, 1f);
        }
    }
}
