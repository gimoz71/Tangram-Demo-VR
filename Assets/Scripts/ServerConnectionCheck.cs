using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.IO;

public class ServerConnectionCheck : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI statusText;

    [Header("Settings")]
    public string defaultIp = "192.168.178.48";
    public float timeout = 3.0f;

    private string currentIp;
    private string finalUrl;

    void Start()
    {
        currentIp = LoadIpFromConfig();

        // Puntiamo allo stesso endpoint del Logger per massima compatibilità
        finalUrl = $"http://{currentIp}/sessions/json";

        StartCoroutine(TestConnectionRoutine());
    }

    private string LoadIpFromConfig()
    {
        string path;
#if UNITY_EDITOR
        path = Path.Combine(Application.dataPath, "server_config.txt");
#else
        path = Path.Combine(Application.persistentDataPath, "server_config.txt");
#endif
        if (File.Exists(path))
        {
            try
            {
                string content = File.ReadAllText(path).Trim();
                if (!string.IsNullOrEmpty(content)) return content;
            }
            catch { }
        }
        return defaultIp;
    }

    private IEnumerator TestConnectionRoutine()
    {
        // Ritardo per permettere ad Android di stabilizzare la rete
        yield return new WaitForSeconds(2.0f);

        if (statusText != null)
        {
            statusText.text = $"Verifica: {currentIp}...";
            statusText.color = Color.white;
            statusText.enabled = true;
        }

        // CREIAMO UN PAYLOAD MINIMO (Uguale alla struttura del Logger)
        // Questo evita il "Codice 0" perché simula un invio dati reale
        string dummyJson = "{\"session_id\":\"CHECK\",\"filename\":\"test.csv\",\"events\":[]}";

        // Usiamo la stessa configurazione manuale del Logger
        using (UnityWebRequest request = new UnityWebRequest(finalUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dummyJson);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Header fondamentale per Android
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(timeout);

            yield return request.SendWebRequest();

            Debug.Log($"[Check] Risposta: {request.responseCode} | Errore: {request.error}");

            // Se il codice è > 0, il server ha risposto (online)
            // Accettiamo Success (200/201), 409 (già esistente) o 422 (validazione)
            if (request.result == UnityWebRequest.Result.Success ||
                request.responseCode == 409 ||
                request.responseCode == 422 ||
                request.responseCode == 201)
            {
                statusText.text = $"Server Online ({currentIp})";
                statusText.color = Color.green;
                StartCoroutine(FadeOutText());
            }
            else
            {
                // Se è ancora 0, mostra l'errore testuale (es. Timeout o DNS)
                statusText.text = $"Server Offline\n{request.error} (Cod: {request.responseCode})";
                statusText.color = Color.red;
            }
        }
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSeconds(5f);
        if (statusText != null) statusText.enabled = false;
    }
}