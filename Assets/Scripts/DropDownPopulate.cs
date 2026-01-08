using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropDownPopulate : MonoBehaviour
{

    GameObject[] dropdownObjects;
    private Dropdown shapesDropdown;
    int dropdownValue = 11;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    

    public void ActivateShapesDropdown()
    {
        shapesDropdown = GameObject.Find("Shape").GetComponent<Dropdown>();
        dropdownObjects = GameObject.FindGameObjectsWithTag("shapes");
        PopulateDropdown(shapesDropdown, dropdownObjects);
    }

    void PopulateDropdown(Dropdown dropdown, GameObject[] optionsArray)
    {
        List<string> options = new List<string>();
        foreach (var option in optionsArray)
        {
            options.Add(option.name); 
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = dropdownValue;
    }

    public void ShowElement() {
        foreach ( var i in dropdownObjects)      {
            i.transform.localPosition = new Vector3(0, 0, 0);
            i.transform.localRotation = Quaternion.identity;
        }
        var selected = shapesDropdown.captionText.text;
        Debug.Log(selected);
        GameObject selectedGameObject = GameObject.Find(selected);
        selectedGameObject.transform.position = new Vector3(0, 1.2f, 0);
    }
}
