using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuAppearSwitch : MonoBehaviour
{
    public CanvasGroup menu; // Assign in inspector
    //private bool isShowing;

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            /*if (isShowing)
            {
                isShowing = !isShowing;
                if (menu.alpha == 1f)
                {
                    menu.alpha = 0f;
                }
                if (menu.alpha == 0f)
                {
                    menu.alpha = 1f;
                }

                Time.timeScale = isShowing ? 0 : 1;
            }*/
            if (menu.alpha == 1f)
            {
                menu.alpha = 0f;
            } else
            {
                menu.alpha = 1f;
            }
        }
    }
}
