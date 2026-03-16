using Unity.XR.CoreUtils;
using UnityEngine;

public class VRRecenter : MonoBehaviour
{
    [Header("Impostazioni")]
    public bool recenterOnStart = true;

    private XROrigin _xrOrigin;

    void Start()
    {
        _xrOrigin = GetComponent<XROrigin>();

        if (recenterOnStart)
        {
            // Un piccolo ritardo assicura che il tracking del visore sia pronto
            Invoke(nameof(DoRecenter), 0.2f);
        }
    }

    public void DoRecenter()
    {
        if (_xrOrigin == null) return;

        // 1. Allineiamo SOLO la rotazione. 
        // L'utente guarderà verso l'asse Z positivo (dove si trova il Tangram),
        // ma la sua posizione X, Y, Z nel mondo virtuale rimarrà invariata.
        _xrOrigin.MatchOriginUpCameraForward(Vector3.up, Vector3.forward);

        Debug.Log("[VR] Recenter Rotazionale completato. Posizione mantenuta.");
    }
}