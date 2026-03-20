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
        if (eventType == "GAZE" || eventType == "GRAB" || eventType == "FINE")
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
}