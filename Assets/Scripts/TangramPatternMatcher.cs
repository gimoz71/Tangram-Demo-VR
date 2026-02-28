using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO; // <--- Necessario per leggere e scrivere file

public class TangramPatternMatcher : MonoBehaviour
{
    // Struttura dati per salvare la posizione di un pezzo
    [System.Serializable]
    public struct PieceGoal
    {
        [Tooltip("Il Tag del pezzo (es. 'TriangoloGrande'). Serve per permettere lo scambio tra pezzi uguali.")]
        public string pieceTag;

        [Tooltip("Posizione relativa rispetto all'Anchor.")]
        public Vector3 relPosition;

        [Tooltip("Rotazione relativa rispetto all'Anchor.")]
        public Quaternion relRotation;
    }

    // --- NUOVA CLASSE PER IL SALVATAGGIO JSON ---
    // Serve a impacchettare tutti i dati del livello in un unico oggetto scrivibile
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public float posTolerance;
        public float rotTolerance;
        public List<PieceGoal> goals;
    }

    [Header("--- Configurazione Pezzi ---")]
    [Tooltip("Il pezzo 'Capo' (es. il Quadrato). La posizione di tutti gli altri pezzi viene calcolata rispetto a questo. Se muovi l'Anchor, la soluzione si sposta con lui.")]
    public XRGrabInteractable anchorPiece;

    [Tooltip("Lista di tutti gli altri pezzi del Tangram che devono essere posizionati.")]
    public List<XRGrabInteractable> otherPieces;

    [Header("--- Tolleranza ---")]
    [Tooltip("Quanto può essere impreciso il giocatore nella POSIZIONE (in metri)? Es. 0.05 = 5cm.")]
    public float positionTolerance = 0.05f;

    [Tooltip("Quanto può essere impreciso il giocatore nella ROTAZIONE (in gradi)?")]
    public float rotationTolerance = 15f;

    [Header("--- Gestione File JSON (Nuovo) ---")]
    [Tooltip("Nome del file da salvare o caricare (es. 'livello_cigno'). Non serve scrivere .json")]
    public string jsonFileName = "nuovo_livello";

    [Header("--- Soluzione (Non toccare a mano) ---")]
    [Tooltip("Questa lista si riempie automaticamente quando premi 'Bake Solution' o carichi un JSON.")]
    [SerializeField] private List<PieceGoal> savedSolution = new List<PieceGoal>();

    [Header("--- Eventi & Setup ---")]
    [Tooltip("Cosa succede quando vinci? (Suoni, effetti particellari, attivazione bottoni, ecc.)")]
    public UnityEvent OnWin;

    [Tooltip("Nome del livello da scrivere nel file di Log (es. 'Cigno', 'Gatto').")]
    public string levelName = "Livello Tangram Cigno";

    // Variabile interna per bloccare il controllo dopo la vittoria
    private bool hasWon = false;

    void Start()
    {
        // All'avvio, cerchiamo il Logger e scriviamo che il livello è iniziato
        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null)
        {
            // Nota: LogData gestisce automaticamente la creazione del file se non esiste
            logger.LogData("INFO", "Start_Level: " + levelName, 0f);
        }
    }

    void Update()
    {
        // Se abbiamo già vinto, non facciamo più controlli (risparmia risorse)
        if (hasWon) return;

        // Controlliamo se il pattern è corretto
        if (CheckPattern())
        {
            hasWon = true;
            Debug.Log($"VITTORIA! Livello {levelName} completato.");

            // Scatena gli eventi Unity (es. Audio, FX)
            OnWin.Invoke();

            // Scrive la vittoria definitiva nel CSV e ferma il logging
            TangramLogger logger = FindObjectOfType<TangramLogger>();
            if (logger != null) logger.LogVictory();
        }
    }

    // --- LOGICA CORE: SMART MATCHING ---
    // Questa funzione permette di scambiare pezzi uguali (es. due triangoli grandi)
    // purché abbiano lo stesso TAG.
    bool CheckPattern()
    {
        // 1. REGOLA FERREA: Non puoi vincere se stai ancora tenendo in mano il pezzo Anchor
        if (anchorPiece.isSelected) return false;

        // Creiamo una lista di indici dei "bersagli" (slot) disponibili.
        List<int> availableGoalIndices = new List<int>();
        for (int i = 0; i < savedSolution.Count; i++) availableGoalIndices.Add(i);

        // Controlliamo ogni pezzo fisico presente sulla scena
        foreach (var piece in otherPieces)
        {
            // 2. REGOLA FERREA: Se hai un pezzo in mano, non puoi vincere
            if (piece.isSelected) return false;

            bool pieceMatched = false;

            // Calcoliamo dove si trova questo pezzo RISPETTO all'Anchor
            Vector3 currentRelPos = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            Quaternion currentRelRot = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;

            // Cerchiamo tra gli slot rimasti vuoti se ce n'è uno compatibile
            for (int j = 0; j < availableGoalIndices.Count; j++)
            {
                int goalIdx = availableGoalIndices[j];
                PieceGoal goal = savedSolution[goalIdx];

                // A. Il Tag deve corrispondere (es. Triangolo == Triangolo)
                if (piece.CompareTag(goal.pieceTag))
                {
                    // B. Calcoliamo la distanza e l'errore angolare
                    float dist = Vector3.Distance(currentRelPos, goal.relPosition);
                    float angle = Quaternion.Angle(currentRelRot, goal.relRotation);

                    // C. Verifichiamo se siamo entro la tolleranza (con controllo simmetrie)
                    if (dist <= positionTolerance && IsRotationValid(piece.transform, angle, rotationTolerance))
                    {
                        // TROVATO! Questo pezzo occupa questo slot.
                        availableGoalIndices.RemoveAt(j); // Rimuoviamo lo slot perché è occupato
                        pieceMatched = true;
                        break; // Passiamo al prossimo pezzo fisico
                    }
                }
            }

            // Se il pezzo corrente non ha trovato NESSUNO slot valido, abbiamo fallito.
            if (!pieceMatched) return false;
        }

        // Se tutti gli slot sono stati riempiti (count == 0), abbiamo vinto!
        return availableGoalIndices.Count == 0;
    }

    /// <summary>
    /// Controlla se la rotazione è valida, tenendo conto delle simmetrie (Quadrato e Parallelogramma).
    /// </summary>
    private bool IsRotationValid(Transform currentPiece, float angleDifference, float tolerance)
    {
        if (currentPiece.CompareTag("square"))
        {
            // Il quadrato è identico ogni 90 gradi
            angleDifference = angleDifference % 90f;
            if (angleDifference > 45f)
            {
                angleDifference = 90f - angleDifference;
            }
        }
        else if (currentPiece.CompareTag("parallelogram"))
        {
            // Il parallelogramma è identico ogni 180 gradi
            angleDifference = angleDifference % 180f;
            if (angleDifference > 90f)
            {
                angleDifference = 180f - angleDifference;
            }
        }

        return angleDifference <= tolerance;
    }

    // =========================================================
    // MENU CONTESTUALI (Tasto Destro sullo script nell'Inspector)
    // =========================================================

    // 1. BAKE: Salva la posizione attuale dei pezzi nella memoria RAM (lista Inspector)
    [ContextMenu("1. Bake Solution (Memoria)")]
    public void BakeSolution()
    {
        if (anchorPiece == null)
        {
            Debug.LogError("ERRORE: Devi assegnare l'Anchor Piece prima di fare il Bake!");
            return;
        }

        // Pulisce la vecchia soluzione
        savedSolution.Clear();

        // Salva la posizione relativa di ogni pezzo
        foreach (var piece in otherPieces)
        {
            PieceGoal newGoal = new PieceGoal();

            // IMPORTANTE: Salva il Tag per permettere lo scambio di pezzi uguali
            newGoal.pieceTag = piece.tag;

            // Calcoli relativi all'Anchor
            newGoal.relPosition = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            newGoal.relRotation = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;

            savedSolution.Add(newGoal);
        }

        Debug.Log($"Bake completato! Salvati {savedSolution.Count} pezzi per la soluzione '{levelName}'. Ora puoi salvare su File.");
    }

    // 2. SAVE: Prende la memoria RAM e la scrive su un file JSON
    [ContextMenu("2. Save Level to JSON")]
    public void SaveLevelToFile()
    {
        if (savedSolution.Count == 0)
        {
            Debug.LogError("La soluzione è vuota! Fai prima 'Bake Solution'.");
            return;
        }

        // Creiamo l'oggetto wrapper
        LevelData data = new LevelData();
        data.levelName = this.levelName;
        data.posTolerance = this.positionTolerance;
        data.rotTolerance = this.rotationTolerance;
        data.goals = this.savedSolution;

        // Convertiamo in testo JSON
        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath();

        // Scriviamo su disco
        File.WriteAllText(path, json);
        Debug.Log($"Livello salvato correttamente in: {path}");

#if UNITY_EDITOR
        // Aggiorna la finestra Project di Unity per far apparire il file subito
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    // 3. LOAD: Legge il file JSON e sovrascrive le impostazioni dello script
    [ContextMenu("3. Load Level from JSON")]
    public void LoadLevelFromFile()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);

            // Convertiamo da testo a Oggetto
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            // Sovrascriviamo le variabili dello script
            this.levelName = data.levelName;
            this.positionTolerance = data.posTolerance;
            this.rotationTolerance = data.rotTolerance;
            this.savedSolution = data.goals;

            Debug.Log($"Livello '{this.levelName}' caricato con successo dal file JSON!");
        }
        else
        {
            Debug.LogError($"Impossibile caricare: File non trovato al percorso {path}");
        }
    }

    // Funzione helper per decidere dove salvare i file
    private string GetFilePath()
    {
#if UNITY_EDITOR
        // In Editor salviamo in una cartella "Levels" dentro Assets, così li vedi
        string folder = Path.Combine(Application.dataPath, "Levels");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return Path.Combine(folder, jsonFileName + ".json");
#else
        // Nel gioco finale (Android/PC Build) usiamo la cartella dei dati persistenti
        return Path.Combine(Application.persistentDataPath, jsonFileName + ".json");
#endif
    }
}