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

    private XROrigin _xrOrigin;

    void Start()
    {
        _xrOrigin = GetComponent<XROrigin>();
        Invoke(nameof(AlignUser), 0.2f);
    }

    void AlignUser()
    {
        if (_xrOrigin == null) return;

        // 1. ROTAZIONE
        if (resettaRotazione)
        {
            _xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);
        }

        // 2. POSIZIONE + OFFSET
        if (resettaPosizione)
        {
            // Calcoliamo la posizione target: 0,0,0 + il tuo offset personalizzato
            // Manteniamo sempre la coordinata Y della camera reale per l'altezza
            Vector3 targetPos = new Vector3(offsetPosizione.x, _xrOrigin.Camera.transform.position.y + offsetPosizione.y, offsetPosizione.z);

            _xrOrigin.MoveCameraToWorldLocation(targetPos);
        }

        Debug.Log($"[VR] Allineamento completato. Offset applicato: {offsetPosizione}");
    }
}