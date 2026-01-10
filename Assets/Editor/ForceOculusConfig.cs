using UnityEngine;
using UnityEditor;

public class ForceOculusConfig : MonoBehaviour
{
    // Aggiunge una voce al menu "Tools"
    [MenuItem("Tools/Force Create Oculus Config")]
    public static void CreateConfig()
    {
        // Cerca di creare l'asset. 
        // Nota: Questo funziona se la classe OculusProjectConfig è accessibile.
        // Se ti dà errore di compilazione, significa che il pacchetto Oculus è rotto/non installato.
        
        // Questo comando apre il file di config ufficiale usando le API interne di Oculus
        EditorApplication.ExecuteMenuItem("Oculus/Tools/Oculus Project Config");
        
        Debug.Log("Ho provato ad aprire il Config. Guarda l'Inspector!");
    }
}