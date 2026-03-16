using UnityEngine;

public class FixInitialPosition : MonoBehaviour
{
    void Awake()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; // Disabilita per prevenire collisioni al frame 0

            // Imposta qui la posizione desiderata se la conosci
            // transform.position = nuovaPosizione; 

            Invoke("EnableCC", 0.1f); // Lo riattiva dopo un istante
        }
    }

    void EnableCC()
    {
        GetComponent<CharacterController>().enabled = true;
    }
}