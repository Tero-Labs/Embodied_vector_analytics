using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements.
using TMPro;

public class EdgeMenuScript : MonoBehaviour
{
    public InputField mainInputField;

    public GameObject menu_parent;
    public float weight;
    public Toggle toggle;
    public Button delete;
    public TMP_Dropdown dropdown;
    public TMP_Text tmptextlabel;
    public bool mapppedweight;


    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        toggle.onValueChanged.AddListener(delegate { DirEdge(toggle); });
        delete.onClick.AddListener(delegate { Delete(delete); });
        dropdown.onValueChanged.AddListener(delegate { ChangeWeightType(dropdown); });

        StartCoroutine(SetupParent());
    }

    void ChangeWeightType(TMP_Dropdown dropdown)
    {
        string target_weight = dropdown.captionText.text;

        if (target_weight == "auto")
        {
            tmptextlabel.text = "X unit";
            mapppedweight = true;
        }
        else
        {
            tmptextlabel.text = "weight";
            mapppedweight = false;
        }
    }


    IEnumerator SetupParent()
    {        
        toggle.isOn = menu_parent.GetComponent<VectorElementScript>().directed_edge;

        if (menu_parent.GetComponent<VectorElementScript>().free_hand)
        {
            dropdown.value = Paintable.weight_dict["auto"];
            tmptextlabel.text = "X unit";
            mapppedweight = true;
            mainInputField.text = menu_parent.GetComponent<VectorElementScript>().edge_weight_multiplier.ToString();
        }
        else
        {
            dropdown.value = Paintable.weight_dict["custom"];
            tmptextlabel.text = "weight";
            mapppedweight = false;
            mainInputField.text = menu_parent.GetComponent<VectorElementScript>().edge_weight.ToString();
        }
        
        yield return null;
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        
        if (input.text.Length > 0)
        {
            weight = (float)menu_parent.GetComponent<VectorElementScript>().edge_weight;
            float.TryParse(input.text, out weight);

            if (!mapppedweight)
                menu_parent.GetComponent<VectorElementScript>().edge_weight = (int)weight;
            else if (mapppedweight && menu_parent.GetComponent<VectorElementScript>().free_hand)
            {
                menu_parent.GetComponent<VectorElementScript>().edge_weight = Mathf.RoundToInt(weight * menu_parent.GetComponent<VectorElementScript>().totalLength);
                menu_parent.GetComponent<VectorElementScript>().edge_weight_multiplier = weight;
            }                 
            else
                menu_parent.GetComponent<VectorElementScript>().edge_weight = (int)weight;

            menu_parent.transform.parent.parent.GetComponent<GraphElementScript>().edges_init();
            //Debug.Log("weight" + input.text + "has been updated for"+ menu_parent.name);
        }
    }

    void DirEdge(Toggle toggle)
    {
        for (int i = 0; i < menu_parent.transform.childCount; i++)
            Destroy(menu_parent.transform.GetChild(i).gameObject);

        menu_parent.GetComponent<VectorElementScript>().directed_edge = toggle.isOn;
        menu_parent.GetComponent<VectorElementScript>().addDot();
    }

    void Delete(Button delete)
    {
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        GameObject graph = menu_parent.transform.parent.parent.gameObject;
        Destroy(menu_parent);

        yield return null;

        graph.GetComponent<GraphElementScript>().edges_init();

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
            Paintable.click_on_inputfield = true;
    }

}
