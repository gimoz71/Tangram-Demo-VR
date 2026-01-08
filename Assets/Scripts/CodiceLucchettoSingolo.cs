using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodiceLucchettoSingolo : MonoBehaviour
{

    public Button addNum;
    public Button removeNum;
    //public Text counter;
    public TextMeshPro counter;
    private int number;

    // Start is called before the first frame update
    void Start()
    {
        counter.text = number.ToString();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddValue()
    {
        if (number < 9)
        {
            number++;
            counter.text = number.ToString();
        }
    }

    public void RemoveValue()
    {
        if (number > 0)
        {
            number--;
            counter.text = number.ToString();
        }
    }
}
