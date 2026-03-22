using UnityEngine;

public class HeadGazeTracker : MonoBehaviour
{
    [Header("Impostazioni Raycast")]
    public float maxDistance = 20f; // Quanto lontano vede il giocatore
    public LayerMask layerMask;     // Quali layer può colpire

    [Header("Filtri Anti-Spam")]
    [Tooltip("Ignora qualsiasi sguardo nei primi X secondi (tempo per inizializzare il visore)")]
    public float warmupTime = 1.0f;
    [Tooltip("Imposta a 0 per tenere tutti i microsguardi. Alzalo (es. 0.1) solo se noti glitch fisici.")]
    public float minGazeDuration = 0.0f;

    [Header("Debug")]
    [SerializeField] private string currentLookingAt = "Niente";
    [SerializeField] private float currentGazeTimer = 0f;

    // Variabili interne per la logica
    private InterestZone currentZone = null;
    private float gazeStartTime;

    // Riferimento al Logger
    private TangramLogger logger;

    void Start()
    {
        // Trova automaticamente il logger nella scena
        logger = FindObjectOfType<TangramLogger>();

        // Imposta la maschera su "Tutto" se ti sei dimenticato di settarla
        if (layerMask == 0) layerMask = LayerMask.GetMask("Default");
    }

    void Update()
    {
        // --- FILTRO WARMUP ---
        if (Time.time < warmupTime) return;

        // 1. Spara il raggio dal centro della telecamera in avanti
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Se colpiamo qualcosa
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            // Cerchiamo se l'oggetto colpito è una "Zona di Interesse"
            InterestZone hitZone = hit.collider.GetComponent<InterestZone>();

            // CASO A: Stiamo guardando una NUOVA zona
            if (hitZone != null && hitZone != currentZone)
            {
                if (currentZone != null)
                {
                    StopGaze(currentZone);
                }

                StartGaze(hitZone);
            }
            // CASO B: Abbiamo colpito un oggetto che NON è una zona
            else if (hitZone == null && currentZone != null)
            {
                StopGaze(currentZone);
            }
        }
        else
        {
            // CASO C: Non stiamo colpendo nulla
            if (currentZone != null)
            {
                StopGaze(currentZone);
            }
        }

        // Aggiorna il timer per debug nell'Inspector
        if (currentZone != null)
        {
            currentGazeTimer = Time.time - gazeStartTime;
        }
    }

    void StartGaze(InterestZone zone)
    {
        currentZone = zone;
        gazeStartTime = Time.time;
        currentLookingAt = zone.zoneName;
        currentGazeTimer = 0f;

        zone.gazeCount++;
    }

    void StopGaze(InterestZone zone)
    {
        float duration = Time.time - gazeStartTime;

        // --- MODIFICA: FILTRO DURATA MINIMA ---
        // Se la durata è inferiore alla soglia, resettiamo senza loggare (evita doppioni)
        if (duration < minGazeDuration)
        {
            // Siccome in StartGaze avevamo aumentato il conteggio, lo annulliamo
            if (zone.gazeCount > 0) zone.gazeCount--;

            ResetGazeState();
            return;
        }

        // Aggiorna il totale della zona
        zone.totalGazeDuration += duration;

        // --- SCRITTURA NEL CSV ---
        if (logger != null)
        {
            logger.LogGaze(zone.zoneName, duration);
        }

        ResetGazeState();
    }

    // --- MODIFICA: Helper per pulire il codice ---
    private void ResetGazeState()
    {
        currentZone = null;
        currentLookingAt = "Niente";
        currentGazeTimer = 0f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
    }
}