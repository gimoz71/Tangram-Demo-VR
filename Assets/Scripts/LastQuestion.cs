using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using models;
using UnityEngine.SceneManagement;

public class LastQuestion : MonoBehaviour
{
    public TextMeshProUGUI lastQuestion;



    //public Timer timer;


    private void Start()
    {
        var configManager = JSonConfigManager.Instance;
        if (SceneChanger.team == 1)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathA);
        }
        if (SceneChanger.team == 2)
        {
            configManager.OpenConfigFile(JSonConfigManager.ConfigFilePathB);
        }

        lastQuestion.text = configManager.getDomandaFinale();
    }

    
}
