using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTexture : MonoBehaviour
{
    public string textureName;
    

    Texture2D main_texture;
    Texture2D emission_texture;
    

    // Start is called before the first frame update
    void Start()
    {
        main_texture = (Texture2D)Resources.Load(textureName);
        emission_texture = (Texture2D)Resources.Load(textureName + "-emission");

        if (main_texture)
        {
            //GetComponent<Renderer>().material.mainTexture = main_texture;
            GetComponent<Renderer>().material.SetTexture("_MainTex", main_texture);
            GetComponent<Renderer>().material.SetTexture("_EmissionMap", emission_texture);

        } else
        {
            Debug.Log("ERRORE CARCAMENTO TEXTURE");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
