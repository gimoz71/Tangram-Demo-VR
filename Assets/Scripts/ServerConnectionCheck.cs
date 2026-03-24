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
    public string defaultIp = "192.168.8.100";
    public float timeout = 5.0f; // Alzato a 5 per maggiore stabilità

    private string currentIp;
    private string finalUrl;

    void Start()
    {
        currentIp = LoadIpFromConfig();
        finalUrl = $"http://{currentIp}/health";

        StartCoroutine(HealthCheckRoutine());
    }

    private string LoadIpFromConfig()
    {
        string path;
#if UNITY_EDITOR
        path = Path.Combine(Application.dataPath, "server_config.txt");
#else
        path = Path.Combine(Application.persistentDataPath, "server_config.txt");
#endif

        string content = defaultIp;
        if (File.Exists(path))
        {
            try
            {
                // --- MODIFICA: Legge le righe singolarmente e prende solo la prima ---
                string[] lines = File.ReadAllLines(path);
                if (lines.Length > 0 && !string.IsNullOrEmpty(lines[0].Trim()))
                {
                    content = lines[0].Trim();
                }
            }
            catch { }
        }

        return content;
    }

    private IEnumerator HealthCheckRoutine()
    {
        // 1. Ritardo iniziale per stabilizzazione Wi-Fi Android
        yield return new WaitForSeconds(3.5f);

        if (statusText != null)
        {
            statusText.text = $"Verifica: {currentIp}...";
            statusText.color = Color.white;
            statusText.enabled = true;
        }

        int attempts = 2; // Proviamo 2 volte prima di arrenderci
        for (int i = 0; i < attempts; i++)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(finalUrl))
            {
                request.SetRequestHeader("User-Agent", "Mozilla/5.0 (Unity/Quest)");
                request.SetRequestHeader("Accept", "*/*");
                request.useHttpContinue = false;
                request.timeout = Mathf.CeilToInt(timeout);

                yield return request.SendWebRequest();

                Debug.Log($"[Check Attempt {i + 1}] Code: {request.responseCode} | Error: {request.error}");

                if (request.result == UnityWebRequest.Result.Success)
                {
                    statusText.text = $"Server Online ({currentIp})";
                    statusText.color = Color.green;
                    StartCoroutine(FadeOutText());
                    yield break; // Successo, usciamo
                }
                else if (i < attempts - 1)
                {
                    // Fallito il primo colpo? Aspettiamo 1.5s e riproviamo
                    if (statusText != null) statusText.text = "Riconnessione...";
                    yield return new WaitForSeconds(1.5f);
                }
                else
                {
                    // Fallimento definitivo
                    string detail = request.error;
                    if (string.IsNullOrEmpty(detail)) detail = "Unknown Error";

                    statusText.text = $"Server Offline\n{detail} (0)";
                    statusText.color = Color.red;
                }
            }
        }
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSeconds(5f);
        if (statusText != null) statusText.enabled = false;
    }
}