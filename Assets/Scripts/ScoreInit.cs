using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreInit : MonoBehaviour
{

    
    private int _score;
    public InputField scoreTextInput;
    public Text ScoreText;

    // Start is called before the first frame update
    void Start()
    {
        GetScore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetScore()
    {
        _score = _score + int.Parse(scoreTextInput.text);
        PlayerPrefs.SetInt("Score", _score);
        GetScore();
    }

    public void GetScore()
    {

        _score = PlayerPrefs.GetInt("Score", 0);
        ScoreText.text = "Punteggio: " + _score.ToString();
        
    }

    public void ResetScore()
    {
        PlayerPrefs.SetInt("Score", 0);
        GetScore();
    }
}
