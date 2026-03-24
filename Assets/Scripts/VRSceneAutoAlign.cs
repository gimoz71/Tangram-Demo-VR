using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;

public class VRSceneAutoAlign : MonoBehaviour
{
    [Header("Impostazioni Scena")]
    public bool resettaPosizione = true;
    public bool resettaRotazione = true;

    [Header("Offset Post-Reset")]
    [Tooltip("Aggiunge questo valore alla posizione dopo il reset. Utile per distanziarsi dal tavolo.")]
    public Vector3 offsetPosizione = Vector3.zero;

    // --- AGGIUNTA: Riferimento al nostro script di Fade ---
    [Header("Transizione")]
    [Tooltip("Trascina qui l'oggetto che contiene il VRFadeController (opzionale)")]
    public VRFadeController faderNero;

    private XROrigin _xrOrigin;

    void Start()
    {
        _xrOrigin = GetComponent<XROrigin>();

        // Lanciamo la Coroutine dinamica
        StartCoroutine(AlignUserRoutine());
    }

    private IEnumerator AlignUserRoutine()
    {
        if (_xrOrigin == null || _xrOrigin.Camera == null) yield break;

        // IL SEGRETO: Aspettiamo che il visore comunichi la vera posizione locale.
        float timeout = 2.0f; // Tempo massimo di attesa per sicurezza
        float timer = 0f;

        while (_xrOrigin.Camera.transform.localPosition == Vector3.zero && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null; // Aspetta il frame successivo
        }

        // Aspettiamo un frame extra per far stabilizzare i sistemi fisici
        yield return new WaitForEndOfFrame();

        // 1. ROTAZIONE
        if (resettaRotazione)
        {
            _xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
        }

        // 2. POSIZIONE + OFFSET
        if (resettaPosizione)
        {
            // Calcoliamo la posizione target mantenendo l'altezza Y appena letta dal tracking
            Vector3 targetPos = new Vector3(
                offsetPosizione.x,
                _xrOrigin.Camera.transform.position.y + offsetPosizione.y,
                offsetPosizione.z
            );

            _xrOrigin.MoveCameraToWorldLocation(targetPos);
        }

        Debug.Log($"[VR] Allineamento completato. Tempo di aggancio: {timer:F2}s. Offset: {offsetPosizione}");

        // --- AGGIUNTA: Facciamo partire il fade SOLO ORA che il giocatore è fermo e posizionato ---
        if (faderNero != null)
        {
            faderNero.StartFade();
        }
        else
        {
            // Fallback: se dimentichi di trascinarlo nell'Inspector, lo cerca da solo nella scena
            VRFadeController fallbackFader = FindObjectOfType<VRFadeController>();
            if (fallbackFader != null) fallbackFader.StartFade();
        }
    }
}