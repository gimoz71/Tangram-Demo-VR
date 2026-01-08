using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace models {

    [System.Serializable]
    public class ConfigModel
    {
        public List<Domanda> domande;
        public string lucchetto;
        public string secondiTimer;
        public List<string> setDomandeSessione;
        public string domandaFinale;
    }

    [System.Serializable]
    public class Domanda {
        public string idDomanda;
        public string testo;
        public List<Risposta> risposte;
        public string rispostaValida;
    }

    [System.Serializable]
    public class Risposta {
        public string id;
        public string testo;
    }
}
