using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class XRInteractableAudio : MonoBehaviour
{
    [Header("Clip Audio")]
    [Tooltip("Suono quando afferri il pezzo (es. un 'clack' di legno)")]
    public AudioClip grabSound;

    [Tooltip("Suono quando rilasci il pezzo")]
    public AudioClip dropSound;

    // Variabili interne
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Forziamo le impostazioni migliori per la VR via codice
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 1 = 100% Audio 3D
        audioSource.minDistance = 0.5f;  // A che distanza inizia a calare il volume
        audioSource.maxDistance = 5f;    // A che distanza non si sente più
    }

    void OnEnable()
    {
        // Colleghiamo i suoni agli eventi di presa di Unity XR
        grabInteractable.selectEntered.AddListener(PlayGrabSound);
        grabInteractable.selectExited.AddListener(PlayDropSound);
    }

    void OnDisable()
    {
        // Pulizia quando l'oggetto viene disattivato
        grabInteractable.selectEntered.RemoveListener(PlayGrabSound);
        grabInteractable.selectExited.RemoveListener(PlayDropSound);
    }

    private void PlayGrabSound(SelectEnterEventArgs args)
    {
        if (grabSound != null)
        {
            // PlayOneShot permette di sovrapporre i suoni senza tagliarli
            audioSource.PlayOneShot(grabSound);
        }
    }

    private void PlayDropSound(SelectExitEventArgs args)
    {
        if (dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }
}