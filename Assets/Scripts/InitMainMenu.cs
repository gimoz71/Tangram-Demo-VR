using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitMainMenu : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        GameObject mainMenu = GameObject.Find("MainMenu");
        GameObject subMenuA = GameObject.Find("Squadra A menu");
        GameObject subMenuB = GameObject.Find("Squadra B menu");

        mainMenu.SetActive(true);
        subMenuA.SetActive(false);
        subMenuB.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
