using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Networking;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

// Classi serializzabili per la formattazione JSON compatibile con SkillVRDB
[System.Serializable]
public class SessionEvent
{
    public string date;
    public string time;
    public string event_type;
    public string object_name;
    public float duration;
}

[System.Serializable]
public class SessionData
{
    public string session_id;
    public string filename;
    public List<SessionEvent> events = new List<SessionEvent>();
}

public class TangramLogger : MonoBehaviour
{
    [Header("--- API Backend ---")]
    [Tooltip("IP del server di default (usato se il file config non esiste)")]
    public string serverIpDefault = "192.168.178.48";

    [Header("--- UI ---")]
    [Tooltip("Trascina qui il bottone Rinuncia (o il suo Canvas) per nasconderlo a fine partita")]
    public GameObject pulsanteRinuncia;

    // --- AGGIUNTA: Riferimenti per la gestione testi Rinuncia ---
    [Tooltip("Il componente Text (TMP) dentro il bottone di rinuncia")]
    public TMPro.TMP_Text testoBottoneRinuncia;

    [Tooltip("Un testo (TMP) extra per mostrare l'ID sessione solo dopo la rinuncia")]
    public TMPro.TMP_Text testoIdSessioneRinuncia;

    // --- AGGIUNTA: Evento per fermare il Timer ---
    [Header("--- Eventi ---")]
    public UnityEngine.Events.UnityEvent onRinunciaEvent;
    // ---------------------------------------------

    private bool haGiaRinunciato = false;
    // ------------------------------------------------------------

    private string serverEndpointUrl; // L'URL completo costruito a runtime

    private string filePath;
    public string currentSessionID;

    private bool isLoggingActive = true;
    private bool fileCreated = false;

    private Dictionary<XRGrabInteractable, float> grabStartTimes = new Dictionary<XRGrabInteractable, float>();

    // Oggetto che manterrà in memoria gli eventi formattati per il server
    private SessionData sessionDataForServer;

    void Awake()
    {
        // 1. CARICAMENTO CONFIGURAZIONE IBRIDA (EDITOR VS STANDALONE)
        LoadExternalConfig();

        // 2. GENERAZIONE CODICE SESSIONE
        currentSessionID = UnityEngine.Random.Range(1000, 10000).ToString();
        string fileName = $"Tangram_Session_{currentSessionID}.csv";

        // Inizializza l'oggetto dati per il JSON
        sessionDataForServer = new SessionData();
        sessionDataForServer.session_id = currentSessionID;
        sessionDataForServer.filename = fileName;

        // 3. DEFINIZIONE PERCORSO LOG CSV
        string folderPath = "";

#if UNITY_EDITOR
        folderPath = Path.Combine(Application.dataPath, "Logs"); 
#elif UNITY_ANDROID
        folderPath = "/storage/emulated/0/Documents/TangramVR_Logs";
#else
        folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "TangramVR_Logs");
#endif

        // 4. CREAZIONE CARTELLA LOG
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore cartella: {e.Message}");
            folderPath = Application.persistentDataPath;
        }

        filePath = Path.Combine(folderPath, fileName);

        Debug.Log($"[CSV] Logger inizializzato in Awake. ID: {currentSessionID}");
    }

    // Carica l'IP da un file di testo esterno (Assets in Editor, PersistentDataPath su Build)
    private void LoadExternalConfig()
    {
        string configPath;

#if UNITY_EDITOR
        // Allineato allo script di test: cerca il file direttamente in Assets
        configPath = Path.Combine(Application.dataPath, "server_config.txt");
#else
        // Su Build cerca nel percorso persistente del visore
        configPath = Path.Combine(Application.persistentDataPath, "server_config.txt");
#endif

        string finalIP = serverIpDefault;

        if (File.Exists(configPath))
        {
            try
            {
                string ipFromFile = File.ReadAllText(configPath).Trim();
                if (!string.IsNullOrEmpty(ipFromFile))
                {
                    finalIP = ipFromFile;
                    Debug.Log($"[API] IP caricato da config esterna ({configPath}): {finalIP}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[API] Errore lettura server_config.txt: {e.Message}");
            }
        }
        else
        {
            // Se in Editor il file manca, lo creiamo per comodità
#if UNITY_EDITOR
            try { File.WriteAllText(configPath, serverIpDefault); } catch { }
#endif
            Debug.Log($"[API] Config non trovata. Uso default: {finalIP}");
        }

        // Costruisce l'URL finale (allineato alla porta 80)
        serverEndpointUrl = $"http://{finalIP}/sessions/json";
    }

    void Start()
    {
        // 5. AUTOMAZIONE INTERAZIONI
        var interactables = FindObjectsOfType<XRGrabInteractable>();
        foreach (var interactable in interactables)
        {
            interactable.selectEntered.AddListener((args) => OnGrabStart(interactable));
            interactable.selectExited.AddListener((args) => OnGrabEnd(interactable));
        }
    }

    // --- LOGICA DI LOGGING ---

    void CreateFileHeader()
    {
        string header = "Date;Time;Event;ObjectName;Duration\n";
        try
        {
            File.WriteAllText(filePath, header);
            fileCreated = true;
            Debug.Log($"[CSV] File creato fisicamente: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore creazione file CSV: {e.Message}");
        }
    }

    void OnGrabStart(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (!grabStartTimes.ContainsKey(item))
            grabStartTimes.Add(item, Time.time);
        else
            grabStartTimes[item] = Time.time;
    }

    void OnGrabEnd(XRGrabInteractable item)
    {
        if (!isLoggingActive) return;

        if (grabStartTimes.ContainsKey(item))
        {
            float duration = Time.time - grabStartTimes[item];
            grabStartTimes.Remove(item);
            LogData("GRAB", item.name, duration);
        }
    }

    public void LogData(string eventType, string objectName, float duration)
    {
        if (!isLoggingActive) return;

        if (!fileCreated)
        {
            CreateFileHeader();
        }

        string datePart = System.DateTime.Now.ToString("dd/MM/yyyy");
        string timePart = System.DateTime.Now.ToString("HH:mm:ss.fff");
        string durationStr = duration > 0 ? duration.ToString("F2") : "";

        // -- 1. EXPORT LOCALE (CSV) --
        string line = $"{datePart};{timePart};{eventType};{objectName};{durationStr}\n";

        try
        {
            File.AppendAllText(filePath, line);
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore scrittura: {e.Message}");
        }

        // -- 2. PREPARAZIONE EXPORT SERVER --
        if (eventType == "GAZE" || eventType == "GRAB" || eventType == "FINE" || eventType == "RINUNCIA")
        {
            SessionEvent newEvent = new SessionEvent
            {
                date = datePart,
                time = timePart,
                event_type = eventType,
                object_name = objectName,
                duration = duration
            };
            sessionDataForServer.events.Add(newEvent);
        }
    }

    public void LogGaze(string zoneName, float duration)
    {
        LogData("GAZE", zoneName, duration);
    }

    public void LogVictory()
    {
        if (!isLoggingActive) return;

        // --- NASCONDE IL PULSANTE QUANDO VINCI ---
        if (pulsanteRinuncia != null)
        {
            pulsanteRinuncia.SetActive(false);
        }

        LogData("FINE", "Tangram completato", 0f);
        isLoggingActive = false;

        Debug.Log($"Vittoria! Sessione conclusa: {currentSessionID}");

        StartCoroutine(SendSessionDataToServer());
    }

    private IEnumerator SendSessionDataToServer()
    {
        if (sessionDataForServer.events.Count == 0) yield break;

        string jsonPayload = JsonUtility.ToJson(sessionDataForServer);
        Debug.Log($"[API] JSON in partenza per {serverEndpointUrl}: {jsonPayload}");

        int maxRetries = 3;
        int retryCount = 0;
        bool success = false;
        float delayBetweenRetries = 2.0f;

        while (retryCount < maxRetries && !success)
        {
            using (UnityWebRequest request = new UnityWebRequest(serverEndpointUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[API] Dati inviati con successo!");
                    success = true;
                }
                else
                {
                    if (request.responseCode == 409) { success = true; yield break; }

                    retryCount++;
                    Debug.LogError($"[API] Fallimento {retryCount}: {request.error}");
                    yield return new WaitForSeconds(delayBetweenRetries);
                }
            }
        }
    }

    // --- FUNZIONE PER IL TASTO RINUNCIA ---
    public void LogGiveUp()
    {
        // Se l'utente clicca una seconda volta dopo la rinuncia, cambia scena
        if (haGiaRinunciato)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
            return;
        }

        if (!isLoggingActive) return;

        // FASE 1: LOG E CHIUSURA SESSIONE
        LogData("RINUNCIA", "Sessione interrotta dall'utente", 0f);
        isLoggingActive = false;
        haGiaRinunciato = true;

        // --- AGGIUNTA: Richiama l'evento per fermare il timer ---
        if (onRinunciaEvent != null)
        {
            onRinunciaEvent.Invoke();
        }
        // ---------------------------------------------------------

        Debug.Log($"Utente arreso. Sessione conclusa: {currentSessionID}");

        // AGGIORNAMENTO UI
        if (testoBottoneRinuncia != null)
        {
            testoBottoneRinuncia.text = "Torna al menu";
        }

        if (testoIdSessioneRinuncia != null)
        {
            testoIdSessioneRinuncia.text = "Session ID: " + currentSessionID;
            testoIdSessioneRinuncia.gameObject.SetActive(true);
        }

        // Avviamo l'invio dati al server (senza cambiare scena subito)
        StartCoroutine(SendSessionDataToServer());
    }

    private IEnumerator SendSessionDataAndExit()
    {
        yield return StartCoroutine(SendSessionDataToServer());
        yield return new WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartMenu");
    }
}