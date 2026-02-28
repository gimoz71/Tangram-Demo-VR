using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TangramTimer : MonoBehaviour
{
    [Header("--- Impostazioni Tempo ---")]
    [Tooltip("Tempo totale a disposizione in secondi (es. 60 = 1 minuto)")]
    public float totalTimeInSeconds = 60f;

    [Tooltip("A quanti secondi dalla fine deve partire il ticchettio ansiogeno? (es. 15)")]
    public float pressureThreshold = 15f;

    [Tooltip("Velocità del lampeggio (in secondi)")]
    public float blinkInterval = 0.5f;

    [Header("--- Audio e UI ---")]
    [Tooltip("L'AudioSource che contiene il SUONO SINGOLO del ticchettio")]
    public AudioSource tickAudioSource;

    [Tooltip("L'AudioSource che contiene il SUONO TRISTE di fine tempo")]
    public AudioSource timeUpAudioSource;

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

    void Start()
    {
        currentTime = totalTimeInSeconds;
        lastSecondRecorded = Mathf.CeilToInt(currentTime);
        UpdateUI();
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
        // Se il tempo era GIA' scaduto, ferma il lampeggio e lascia il testo fisso
        if (!isRunning && currentTime <= 0f)
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            if (timerText != null) timerText.enabled = true; // Assicura che 00:00 rimanga visibile
        }
        else
        {
            // Vittoria entro il tempo limite: ferma timer e audio, inizia a lampeggiare
            isRunning = false;
            if (tickAudioSource != null) tickAudioSource.Stop();

            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(BlinkTextCoroutine());
        }

        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null) logger.LogData("EVENT", "Timer_Stopped_On_Win", currentTime);
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