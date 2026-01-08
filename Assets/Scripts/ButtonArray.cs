using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using models;
using UnityEngine.SceneManagement;

public class ButtonArray : MonoBehaviour
{
    public GameObject AnswerParent;
    public GameObject buttonPrefab;
    public TextMeshProUGUI question;
    public Text questionIndex;

    public ParticleSystem rightParticle;
    public ParticleSystem wrongParticle;

    AudioSource answerConfirm;

    public AudioClip rightAnswerAudioClip;
    public AudioClip errorAnswerAudioClip;

    public Timer timer;

    public List<Domanda> domande = new List<Domanda>();
    public int indiceDomanda = 0;
    public int numeroRisposte = 0;

    public int indiceDomandaTesto;

    public int credit;

    private void Start()
    {
        Debug.Log("START!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        var configManager = JSonConfigManager.Instance;
        if (SceneChanger.team == 1)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathA);
        }
        if (SceneChanger.team == 2)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathB);
        }
        domande = configManager.getDomandeSessione();


        rightParticle.GetComponent<ParticleSystem>().Stop();
        wrongParticle.GetComponent<ParticleSystem>().Stop();

        answerConfirm = GetComponent<AudioSource>();

        

        indiceDomanda = PlayerPrefs.GetInt("Domanda");

        indiceDomandaTesto = indiceDomanda + 1;

        questionIndex.text = "Domanda: " + indiceDomandaTesto;
    }
    
    private void popolaDomanda(Domanda domanda) {
        resetAnswers();

        question.text = domanda.testo;
        var answerNumber = 1;
        foreach (var risposta in domanda.risposte) {
            GameObject answer = Instantiate(buttonPrefab);
            answer.name = "answer-" + answerNumber; 
            answer.transform.SetParent(AnswerParent.transform, false);
            Button tempButton = answer.GetComponent<Button>();
            answer.GetComponentInChildren<Text>().text = risposta.testo;
            tempButton.onClick.AddListener(() => ButtonClicked(risposta.id, domanda.idDomanda));
            answerNumber++;
        }

        numeroRisposte = answerNumber;

        PlayerPrefs.SetInt("Domanda", indiceDomanda);

        indiceDomandaTesto = indiceDomanda + 1;
        questionIndex.text = "Domanda: " + indiceDomandaTesto;
        indiceDomanda++;
       

    }

    private void resetAnswers() {
        for (var i = 1; i <= numeroRisposte; i++) {
            var obj = GameObject.Find("answer-" + i);
            Destroy(obj);
        }
        numeroRisposte = 0;
    }

    private void popolaFineOK() {
        timer.StopTimer();
        resetAnswers();
        //question.text = "Complimenti, hai risposto a tutte le domande!<br>Puoi passare alla fase successiva" + "<br>Risposte giuste: " + credit + " su 18";
        question.text = "Complimenti, hai risposto a tutte le domande!<br>Puoi passare alla fase successiva" + "<br><br>Hai guadagnato " + credit + " punti";
    }

    public void InitQuestion()
    {
        //timer.StartTimer();
        if (domande.Count > 0) {
            popolaDomanda(domande[indiceDomanda]);
        }

    }

    void ButtonClicked(string idRisposta, string idDomanda )
    {
        if (indiceDomanda == domande.Count)
        {
            //sono arrivato alla fine delle domande
            popolaFineOK();
        }
        else
        {
            popolaDomanda(domande[indiceDomanda]);
        }

        //verifica risposta
        if (idRisposta.Equals(getRispostaValidaId(idDomanda)))
        {
           

            rightParticle.GetComponent<ParticleSystem>().Play();
            answerConfirm.PlayOneShot(rightAnswerAudioClip, 1f);

            credit += 2;
        }
        else {

            //gestione grafica errore risposta
            //resetAnswers();
            //timer.StopTimer();
            //question.text = "";
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            

            wrongParticle.GetComponent<ParticleSystem>().Play();
            answerConfirm.PlayOneShot(errorAnswerAudioClip, 1f);

            if (credit != 0  || credit > 0)
            {
                credit -= 1;
            }
        }
    }

    private string getRispostaValidaId(string idDomanda) {
        var rispostaId = "0";

        foreach (var domanda in domande) {
            if (domanda.idDomanda.Equals(idDomanda)) {
                return domanda.rispostaValida;
            }
        }

        return rispostaId;
    }

    public void resetQuestion()
    {

        PlayerPrefs.SetInt("Domanda", 0);
        indiceDomanda = PlayerPrefs.GetInt("Domanda");
        indiceDomandaTesto = indiceDomanda + 1;
        questionIndex.text = "Domanda: " + indiceDomandaTesto;

    }
}
