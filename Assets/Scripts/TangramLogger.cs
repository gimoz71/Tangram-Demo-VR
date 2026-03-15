using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Networking; // Aggiunto per l'invio web
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
    [Tooltip("Endpoint del server FastAPI per l'upload batch a fine sessione")]
    public string serverEndpointUrl = "http://192.168.178.48/sessions/json";

    private string filePath;
    public string currentSessionID;

    private bool isLoggingActive = true;
    private bool fileCreated = false;

    private Dictionary<XRGrabInteractable, float> grabStartTimes = new Dictionary<XRGrabInteractable, float>();

    // Oggetto che manterrà in memoria gli eventi formattati per il server
    private SessionData sessionDataForServer;

    // SPOSTATO IN AWAKE: Viene eseguito PRIMA di qualsiasi Start() degli altri script
    void Awake()
    {
        // 1. GENERAZIONE CODICE
        currentSessionID = UnityEngine.Random.Range(1000, 10000).ToString();
        string fileName = $"Tangram_Session_{currentSessionID}.csv";

        // Inizializza l'oggetto dati per il JSON
        sessionDataForServer = new SessionData();
        sessionDataForServer.session_id = currentSessionID;
        sessionDataForServer.filename = fileName;

        // 2. DEFINIZIONE PERCORSO
        string folderPath = "";

#if UNITY_EDITOR
        folderPath = Path.Combine(Application.dataPath, "Logs"); 
#elif UNITY_ANDROID
        folderPath = "/storage/emulated/0/Documents/TangramVR_Logs";
#else
        folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "TangramVR_Logs");
#endif

        // 3. CREAZIONE CARTELLA
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

    void Start()
    {
        // 4. AUTOMAZIONE INTERAZIONI (Questo può restare in Start)
        var interactables = FindObjectsOfType<XRGrabInteractable>();
        foreach (var interactable in interactables)
        {
            interactable.selectEntered.AddListener((args) => OnGrabStart(interactable));
            interactable.selectExited.AddListener((args) => OnGrabEnd(interactable));
        }
    }

    // --- DA QUI IN GIU' E' TUTTO UGUALE A PRIMA ---

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

        // Se è la prima volta che scriviamo qualcosa (anche INFO), crea il file
        if (!fileCreated)
        {
            CreateFileHeader();
        }

        string datePart = System.DateTime.Now.ToString("dd/MM/yyyy");
        string timePart = System.DateTime.Now.ToString("HH:mm:ss.fff");
        string durationStr = duration > 0 ? duration.ToString("F2") : "";

        // -- 1. EXPORT LOCALE (CSV Completo) --
        string line = $"{datePart};{timePart};{eventType};{objectName};{durationStr}\n";

        try
        {
            File.AppendAllText(filePath, line);
        }
        catch (Exception e)
        {
            // Protezione extra nel caso il path fosse ancora null (non dovrebbe più succedere)
            if (string.IsNullOrEmpty(filePath))
                Debug.LogError("FilePath è null! Awake non ha funzionato correttamente.");
            else
                Debug.LogError($"Errore scrittura: {e.Message}");
        }

        // -- 2. PREPARAZIONE EXPORT SERVER (Solo eventi autorizzati) --
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

        // Invia i dati formattati al server a fine sessione
        StartCoroutine(SendSessionDataToServer());
    }

    // Coroutine per l'invio asincrono del JSON a fine sessione
    private IEnumerator SendSessionDataToServer()
    {
        if (sessionDataForServer.events.Count == 0)
        {
            Debug.LogWarning("[API] Nessun evento da inviare al server.");
            yield break;
        }

        string jsonPayload = JsonUtility.ToJson(sessionDataForServer);

        using (UnityWebRequest request = new UnityWebRequest(serverEndpointUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log($"[API] Invio di {sessionDataForServer.events.Count} eventi al server...");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[API] Errore invio dati a SkillVRDB: {request.error}");
            }
            else
            {
                Debug.Log($"[API] Dati inviati con successo! Risposta: {request.downloadHandler.text}");
            }
        }
        Debug.Log($"[API] JSON in partenza: {jsonPayload}");
    }
}