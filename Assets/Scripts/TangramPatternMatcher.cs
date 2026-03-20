using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public float posTolerance;
        public float rotTolerance;
        public List<PieceGoal> goals;
    }

    [Header("--- Configurazione Pezzi ---")]
    [Tooltip("Il pezzo 'Capo' (es. il Quadrato). La posizione di tutti gli altri pezzi viene calcolata rispetto a questo.")]
    public XRGrabInteractable anchorPiece;

    [Tooltip("Lista di tutti gli altri pezzi del Tangram che devono essere posizionati.")]
    public List<XRGrabInteractable> otherPieces;

    [Header("--- Tolleranza ---")]
    [Tooltip("Quanto può essere impreciso il giocatore nella POSIZIONE (in metri)?")]
    public float positionTolerance = 0.05f;

    [Tooltip("Quanto può essere impreciso il giocatore nella ROTAZIONE (in gradi)?")]
    public float rotationTolerance = 15f;

    [Header("--- Gestione File JSON ---")]
    [Tooltip("Nome del file da salvare o caricare. Non serve scrivere .json")]
    public string jsonFileName = "nuovo_livello";

    [Header("--- Soluzione (Non toccare a mano) ---")]
    [Tooltip("Questa lista si riempie automaticamente quando premi 'Bake Solution' o carichi un JSON.")]
    [SerializeField] private List<PieceGoal> savedSolution = new List<PieceGoal>();

    [Header("--- Eventi & Setup ---")]
    public UnityEvent OnWin;

    [Tooltip("Nome del livello da scrivere nel file di Log.")]
    public string levelName = "Livello Tangram Cigno";

    private bool hasWon = false;

    void Start()
    {
        TangramLogger logger = FindObjectOfType<TangramLogger>();
        if (logger != null)
        {
            logger.LogData("INFO", "Start_Level: " + levelName, 0f);
        }
    }

    void Update()
    {
        if (hasWon) return;

        if (CheckPattern())
        {
            hasWon = true;
            Debug.Log($"VITTORIA! Livello {levelName} completato.");
            OnWin.Invoke();

            TangramLogger logger = FindObjectOfType<TangramLogger>();
            if (logger != null) logger.LogVictory();
        }
    }

    bool CheckPattern()
    {
        if (anchorPiece.isSelected) return false;

        List<int> availableGoalIndices = new List<int>();
        for (int i = 0; i < savedSolution.Count; i++) availableGoalIndices.Add(i);

        foreach (var piece in otherPieces)
        {
            if (piece.isSelected) return false;

            bool pieceMatched = false;

            Vector3 currentRelPos = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            Quaternion currentRelRot = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;

            for (int j = 0; j < availableGoalIndices.Count; j++)
            {
                int goalIdx = availableGoalIndices[j];
                PieceGoal goal = savedSolution[goalIdx];

                if (piece.CompareTag(goal.pieceTag))
                {
                    float dist = Vector3.Distance(currentRelPos, goal.relPosition);
                    float angle = Quaternion.Angle(currentRelRot, goal.relRotation);

                    if (dist <= positionTolerance && IsRotationValid(piece.transform, angle, rotationTolerance))
                    {
                        availableGoalIndices.RemoveAt(j);
                        pieceMatched = true;
                        break;
                    }
                }
            }

            if (!pieceMatched) return false;
        }

        return availableGoalIndices.Count == 0;
    }

    private bool IsRotationValid(Transform currentPiece, float angleDifference, float tolerance)
    {
        if (currentPiece.CompareTag("square"))
        {
            angleDifference = angleDifference % 90f;
            if (angleDifference > 45f) angleDifference = 90f - angleDifference;
        }
        else if (currentPiece.CompareTag("parallelogram"))
        {
            angleDifference = angleDifference % 180f;
            if (angleDifference > 90f) angleDifference = 180f - angleDifference;
        }

        return angleDifference <= tolerance;
    }

    // =========================================================
    // MENU CONTESTUALI (Tasto Destro sullo script nell'Inspector)
    // =========================================================

    [ContextMenu("1. Bake Solution (Memoria)")]
    public void BakeSolution()
    {
        if (anchorPiece == null)
        {
            Debug.LogError("ERRORE: Devi assegnare l'Anchor Piece prima di fare il Bake!");
            return;
        }

        savedSolution.Clear();

        foreach (var piece in otherPieces)
        {
            PieceGoal newGoal = new PieceGoal();
            newGoal.pieceTag = piece.tag;
            newGoal.relPosition = anchorPiece.transform.InverseTransformPoint(piece.transform.position);
            newGoal.relRotation = Quaternion.Inverse(anchorPiece.transform.rotation) * piece.transform.rotation;
            savedSolution.Add(newGoal);
        }

        Debug.Log($"Bake completato! Salvati {savedSolution.Count} pezzi.");
    }

    [ContextMenu("2. Save Level to JSON")]
    public void SaveLevelToFile()
    {
        if (savedSolution.Count == 0)
        {
            Debug.LogError("La soluzione è vuota! Fai prima 'Bake Solution'.");
            return;
        }

        LevelData data = new LevelData();
        data.levelName = this.levelName;
        data.posTolerance = this.positionTolerance;
        data.rotTolerance = this.rotationTolerance;
        data.goals = this.savedSolution;

        string json = JsonUtility.ToJson(data, true);
        string path = GetFilePath();

        File.WriteAllText(path, json);
        Debug.Log($"Livello salvato in: {path}");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }

    [ContextMenu("3. Load Level from JSON")]
    public void LoadLevelFromFile()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            this.levelName = data.levelName;
            this.positionTolerance = data.posTolerance;
            this.rotationTolerance = data.rotTolerance;
            this.savedSolution = data.goals;

            Debug.Log($"Livello '{this.levelName}' caricato con successo!");
        }
        else
        {
            Debug.LogError($"File non trovato al percorso {path}");
        }
    }

    // --- AGGIUNTA: RESET PEZZI ALLA SOLUZIONE ---
    [ContextMenu("4. Reset Pieces to Solution (Editor Only)")]
    public void ResetPiecesToSolution()
    {
        // Impedisce l'uso accidentale durante il gioco
        if (Application.isPlaying)
        {
            Debug.LogWarning("Il Reset è disponibile solo in modalità Editing.");
            return;
        }

        if (anchorPiece == null || savedSolution == null || savedSolution.Count == 0)
        {
            Debug.LogError("Manca l'Anchor o la soluzione salvata è vuota!");
            return;
        }

        for (int i = 0; i < otherPieces.Count; i++)
        {
            if (i >= savedSolution.Count) break;

            XRGrabInteractable piece = otherPieces[i];
            PieceGoal goal = savedSolution[i];

#if UNITY_EDITOR
            Undo.RecordObject(piece.transform, "Reset Tangram Piece");
#endif
            // Riposiziona il pezzo usando l'Anchor come riferimento (matematica inversa al Bake)
            piece.transform.position = anchorPiece.transform.TransformPoint(goal.relPosition);
            piece.transform.rotation = anchorPiece.transform.rotation * goal.relRotation;
        }

        Debug.Log("Tangram ricomposto correttamente nell'Editor.");
    }

    private string GetFilePath()
    {
#if UNITY_EDITOR
        string folder = Path.Combine(Application.dataPath, "Levels");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return Path.Combine(folder, jsonFileName + ".json");
#else
        return Path.Combine(Application.persistentDataPath, jsonFileName + ".json");
#endif
    }
}