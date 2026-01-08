using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageState : MonoBehaviour
{

    public TextMeshProUGUI message;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendMessage(int value)
    {
        if (value == 1)
        {
            message.text = "Peccato, non hai finito, devi abbandonare la grotta e la sfida passa in mano all’altro clan.";
        }
        if (value == 2)
        {
            message.text = "Bravo, hai superato l’enigma ma non è sufficiente. Passa la sfida ad un altro clan per ottenere un numero di pietre sufficienti.";
        }
        if (value == 3)
        {
            message.text = "Bravi, avete raccolto tutte le pietre! potete passare al prossimo enigma. Passa la sfida a un membro dell’altro clan.";
        }
        if (value == 4)
        {
            message.text = "";
        }
    }
}
