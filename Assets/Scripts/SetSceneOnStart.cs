using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetSceneOnStart : MonoBehaviour
{
    public Text sceneTitle;
    public GameObject scene1;
    public GameObject scene2;
    public GameObject scene3;
    public GameObject scene4;
    public GameObject scene5;
    public GameObject scene6;
    

    void Awake()
    {
        scene1.SetActive(false);
        scene2.SetActive(false);
        scene3.SetActive(false);
        scene4.SetActive(false);
        scene5.SetActive(false);
        scene6.SetActive(false);

        Debug.Log("TEAM: " + SceneChanger.team + "SCENA: " + SceneChanger.scene);

        if (SceneChanger.team == 0)
        {
            SceneChanger.team = 1;
        }

        if (SceneChanger.scene == 0)
        {
            SceneChanger.scene = 1;
        }

        sceneTitle = GameObject.Find("Scene Title").GetComponent<Text>();




        // Scena 1 - Naggaroth
        if (SceneChanger.scene == 1)
        {
            LightDimmer.value = 10f;

            scene1.SetActive(true);
            sceneTitle.text = "1: Naggaroth";

            if (SceneChanger.team == 1)
            {
                GameObject lettereSimboli = GameObject.Find("LettereSimboliB");
                lettereSimboli.SetActive(false);
            } else
            {
                GameObject lettereSimboli = GameObject.Find("LettereSimboliA");
                lettereSimboli.SetActive(false);
            }

            /*GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(true);*/

            /*GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(true);*/

            /*GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(false);*/


        }




        // Scena 2 - Tangramion
        if (SceneChanger.scene == 2)
        {
            LightDimmer.value = 4f;

            scene2.SetActive(true);
            sceneTitle.text = "2: Tangramion";


            if (SceneChanger.team == 1)
            {
                GameObject lettereSimboli = GameObject.Find("TangramA");
                lettereSimboli.SetActive(false);
            }
            else
            {
                GameObject lettereSimboli = GameObject.Find("TangramB");
                lettereSimboli.SetActive(false);
            }


            /*GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(true);*/

            /*GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(true);*/
            

           /* GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(false);*/

        }




        // Scena 3 - Treasure Chest
        if (SceneChanger.scene == 3)
        {
            LightDimmer.value = 3f;

            scene3.SetActive(true);
            sceneTitle.text = "3: Treasure Chest";

            GameObject ghostButton = GameObject.Find("Ghost");
            ghostButton.SetActive(true);

            /*GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(true);*/

            /*GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(true);*/



            /*GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(false);*/


        } else {
            GameObject ghostButton = GameObject.Find("Ghost");
            ghostButton.SetActive(false);
        }




        // Scena 4 - Wise Totem
        if (SceneChanger.scene == 4)
        {
            LightDimmer.value = 2f;

            scene4.SetActive(true);
            sceneTitle.text = "4: Wise Totem";



            GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(false);

            GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(false);


            GameObject questionPanel = GameObject.Find("Question Panel");
            questionPanel.SetActive(true);

            GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(true);
        } else
        {
            GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(true);

            GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(true);

            GameObject questionPanel = GameObject.Find("Question Panel");
            questionPanel.SetActive(false);

            GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(false);
        }




        // Scena 5 - Sei sicuro di quello che vedi?
        if (SceneChanger.scene == 5)
        {
            LightDimmer.value = 1.5f;

            scene5.SetActive(true);
            sceneTitle.text = "5: Sei sicuro di quello che vedi?";

            GameObject shapeSelect = GameObject.Find("Shape");
            shapeSelect.SetActive(true);

            GameObject.Find("Shape").GetComponent<DropDownPopulate>().ActivateShapesDropdown();


            /*GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(true);*/

            /*GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(true);*/
            
            /*GameObject questionTimePanel = GameObject.Find("Question Timer Panel");
            questionTimePanel.SetActive(false);*/

        }
        else
        {
            GameObject shapeSelect = GameObject.Find("Shape");
            shapeSelect.SetActive(false);
        }




        // Scena 6 - Luci o Tenebre?
        if (SceneChanger.scene == 6)
        {
            LightDimmer.value = 0f;

            scene6.SetActive(true);
            sceneTitle.text = "6: Luci o Tenebre?";


            if (SceneChanger.team == 1)
            {
                GameObject lettereSimboli = GameObject.Find("BrickA");
                lettereSimboli.SetActive(false);
            }
            else
            {
                GameObject lettereSimboli = GameObject.Find("BrickB");
                lettereSimboli.SetActive(false);
            }


            

            GameObject finaleTotaleButton = GameObject.Find("Finale Totale");
            finaleTotaleButton.SetActive(true);

            GameObject FinaleTimePanel = GameObject.Find("Finale Timer Panel");
            FinaleTimePanel.SetActive(true);


            GameObject timerPanel = GameObject.Find("Timer Panel");
            timerPanel.SetActive(false);

            

            GameObject countDownMainArea = GameObject.Find("Countdown");
            countDownMainArea.SetActive(false);




        }
        else
        {
            

            GameObject FinaleTimePanel = GameObject.Find("Finale Timer Panel");
            FinaleTimePanel.SetActive(false);

            GameObject finaleTotaleButton = GameObject.Find("Finale Totale");
            finaleTotaleButton.SetActive(false);
        }
    }
}
