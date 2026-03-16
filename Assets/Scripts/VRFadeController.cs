using UnityEngine;
using System.Collections;

public class VRFadeController : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDuration = 1.5f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Forza l'alpha a 1 immediatamente nel caso non lo fosse nell'editor
        canvasGroup.alpha = 1f;
    }

    IEnumerator Start()
    {
        // 1. Aspettiamo che la fisica e il tracking XR si stabilizzino.
        // Aspettare due frame "EndOfFrame" garantisce che l'utente non veda 
        // assolutamente nulla del setup iniziale.
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 2. Ora iniziamo il Fade In (da nero a trasparente)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.InverseLerp(fadeDuration, 0f, timer);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        // 3. Disattiva l'intero Canvas per liberare risorse GPU
        transform.parent.gameObject.SetActive(false);
    }
}