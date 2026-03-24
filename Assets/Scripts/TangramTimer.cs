using System.Collections;
using System.IO;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Networking; // Aggiunto per comunicare col server

// --- AGGIUNTA: Classe per decodificare il JSON dal server FastAPI ---
[System.Serializable]
public class TimerServerConfig
{
    public float totalTimeInSeconds;
    public float initialDelay;
    public float pressureThreshold;
}
// --------------------------------------------------------------------

public class TangramTimer : MonoBehaviour
{
    [Header("--- Impostazioni Tempo (Default se manca il file) ---")]
    [Tooltip("Tempo totale a disposizione in secondi (es. 60 = 1 minuto)")]
    public float totalTimeInSeconds = 60f;

    [Tooltip("Ritardo prima che il timer inizi a scalare (per attendere il fade iniziale)")]
    public float initialDelay = 2.0f;

    [Tooltip("A quanti secondi dalla fine deve partire il ticchettio ansiogeno? (es. 15)")]
    public float pressureThreshold = 15f;

    [Tooltip("Velocità del lampeggio (in secondi)")]
    public float blinkInterval = 0.5f;

    [Header("--- Audio e UI ---")]
    [Tooltip("L'AudioSource che contiene il SUONO SINGOLO del ticchettio")]
    public AudioSource tickAudioSource;

    [Tooltip("L'AudioSource che contiene il SUONO TRISTE di fine tempo")]
    public AudioSource timeUpAudioSource;

    [Tooltip("Suono per il countdown iniziale (es. i semafori rossi della F1)")]
    public AudioSource countdownBeepAudioSource;

    [Tooltip("Suono per il via effettivo (es. semaforo verde)")]
    public AudioSource startBeepAudioSource;

    [Tooltip("Testo dell'interfaccia per mostrare il countdown")]
    public TextMeshProUGUI timerText;

    [Header("--- Eventi ---")]
    [Tooltip("(Opzionale) Lascialo vuoto se vuoi che il giocatore continui a giocare!")]
    public UnityEvent OnTimeUp;

    // Variabili interne
    private float currentTime;
    private bool isRunning = false;
    private bool pressurePhaseStarted = false;
    private int lastSecondRecorded;
    private Coroutine blinkCoroutine;

    // --- MODIFICA: Rimosso l'IP hardcoded. Ora lo decideremo a runtime ---
    private string serverIP;

    void Awake()
    {
        LoadLocalFallbackAndIP();
    }

    private void LoadLocalFallbackAndIP()
    {
        // --- MODIFICA: Centralizzazione dell'IP ---
        // Prima cosa: andiamo a leggere l'IP di default dal ServerConnectionCheck
        serverIP = "127.0.0.1"; // Fallback di assoluta emergenza
        ServerConnectionCheck serverCheck = FindObjectOfType<ServerConnectionCheck>();
        if (serverCheck != null)
        {
            serverIP = serverCheck.defaultIp;
        }
        // ------------------------------------------

        string configPath;

#if UNITY_EDITOR
        configPath = Path.Combine(Application.dataPath, "server_config.txt");
#else
        configPath = Path.Combine(Application.persistentDataPath, "server_config.txt");
#endif

        if (File.Exists(configPath))
        {
            try
            {
                string[] lines = File.ReadAllLines(configPath);

                // Riga 1: IP Server (sovrascrive il default del ServerConnectionCheck se presente)
                if (lines.Length > 0 && !string.IsNullOrEmpty(lines[0].Trim()))
                    serverIP = lines[0].Trim();

                // Riga 2: Total Time (Piano B)
                if (lines.Length >= 2 && float.TryParse(lines[1].Trim(), out float newTotalTime))
                    totalTimeInSeconds = newTotalTime;

                // Riga 3: Initial Delay (Piano B)
                if (lines.Length >= 3 && float.TryParse(lines[2].Trim(), out float newDelay))
                    initialDelay = newDelay;

                // Riga 4: Pressure Threshold (Piano B)
                if (lines.Length >= 4 && float.TryParse(lines[3].Trim(), out float newPressure))
                    pressureThreshold = newPressure;

                Debug.Log($"[TIMER] Piano B locale preparato. IP Server: {serverIP}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TIMER] Errore lettura server_config.txt: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"[TIMER] server_config.txt non trovato. Uso i default dell'Inspector e l'IP: {serverIP}");
        }
    }

    IEnumerator Start()
    {
        yield return StartCoroutine(FetchConfigFromServer());

        currentTime = totalTimeInSeconds;
        lastSecondRecorded = Mathf.CeilToInt(currentTime);
        UpdateUI();

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Timer_Blinking_Phase_Started", initialDelay);

        if (blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkTextCoroutine());

        StartCoroutine(StartWithDelay());
    }

    private IEnumerator FetchConfigFromServer()
    {
        string url = $"http://{serverIP}/config";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    TimerServerConfig apiConfig = JsonUtility.FromJson<TimerServerConfig>(request.downloadHandler.text);
                    totalTimeInSeconds = apiConfig.totalTimeInSeconds;
                    initialDelay = apiConfig.initialDelay;
                    pressureThreshold = apiConfig.pressureThreshold;

                    Debug.Log($"[TIMER] Dati ricevuti dal Server: Tot:{totalTimeInSeconds}s | Delay:{initialDelay}s | Press:{pressureThreshold}s");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TIMER] Errore nel parsing JSON del server. Uso il Piano B. Errore: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[TIMER] Server non raggiungibile o in errore ({request.responseCode}). Uso il Piano B locale.");
            }
        }
    }

    private IEnumerator StartWithDelay()
    {
        isRunning = false;

        float delayTimer = initialDelay;
        while (delayTimer > 0f)
        {
            if (countdownBeepAudioSource != null)
                countdownBeepAudioSource.PlayOneShot(countdownBeepAudioSource.clip);

            float waitTime = Mathf.Min(1f, delayTimer);
            yield return new WaitForSeconds(waitTime);
            delayTimer -= waitTime;
        }

        if (startBeepAudioSource != null)
            startBeepAudioSource.PlayOneShot(startBeepAudioSource.clip);

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (timerText != null) timerText.enabled = true;

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Timer_Countdown_Started", totalTimeInSeconds);

        isRunning = true;
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            TimeRanOut();
            return;
        }

        int currentSecond = Mathf.CeilToInt(currentTime);

        if (currentSecond < lastSecondRecorded)
        {
            lastSecondRecorded = currentSecond;
            UpdateUI();

            if (currentSecond <= pressureThreshold)
            {
                if (!pressurePhaseStarted) StartPressurePhase();

                if (tickAudioSource != null) tickAudioSource.PlayOneShot(tickAudioSource.clip);
            }
        }
    }

    private void StartPressurePhase()
    {
        pressurePhaseStarted = true;
        if (timerText != null) timerText.color = Color.red;

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Pressure_Phase_Started", currentTime);
    }

    private void TimeRanOut()
    {
        currentTime = 0f;
        lastSecondRecorded = 0;
        isRunning = false;
        UpdateUI();

        if (tickAudioSource != null) tickAudioSource.Stop();
        if (timeUpAudioSource != null) timeUpAudioSource.Play();

        if (blinkCoroutine == null)
            blinkCoroutine = StartCoroutine(BlinkTextCoroutine());

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Timer_Reached_Zero", 0f);

        OnTimeUp.Invoke();
    }

    public void StopTimerOnWin()
    {
        if (!isRunning && currentTime <= 0f)
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            if (timerText != null) timerText.enabled = true;
        }
        else
        {
            isRunning = false;
            if (tickAudioSource != null) tickAudioSource.Stop();

            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkTextCoroutine());
        }

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Timer_Stopped_On_Win", currentTime);
    }

    public void StopTimerOnGiveUp()
    {
        isRunning = false;

        if (tickAudioSource != null) tickAudioSource.Stop();

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (timerText != null) timerText.enabled = true;
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = lastSecondRecorded / 60;
            int seconds = lastSecondRecorded % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private IEnumerator BlinkTextCoroutine()
    {
        while (true)
        {
            if (timerText != null)
            {
                timerText.enabled = !timerText.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}