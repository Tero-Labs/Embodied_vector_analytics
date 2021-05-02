using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphSliderMenu : MonoBehaviour
{
    public string cur_layer;
    public GameObject parent;
    public GameObject menu_object;
    public Slider mainSlider;

    public TMP_Text tmptextlabel;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void setparent(GameObject obj)
    {
        this.parent = obj;
    }

    public void UpdateLayer(string layer)
    {
        if (layer == "graph")
        {
            mainSlider.value = 1;
            cur_layer = "graph";
        }
        else if (layer == "simplicial")
        {
            mainSlider.value = 2;
            cur_layer = "simplicial";
        }
        else if (layer == "hypergraph")
        {
            mainSlider.value = 3;
            cur_layer = "hypergraph";
        }
        else
        {
            mainSlider.value = 4;
            cur_layer = "abstract";
        }
        tmptextlabel.text = cur_layer;
    }

    public void OnSliderValueChanged(float value)
    {
        if (value == 1)
        {
            cur_layer = "graph";
        }            
        else if (value == 2)
        {
            cur_layer = "simplicial";
        }            
        else if (value == 3)
        {
            cur_layer = "hypergraph";
        }
        else
        {
            cur_layer = "abstract";
        }

        tmptextlabel.text = cur_layer;
        if (transform.parent.name.Contains("Graph_menu"))
        {
            parent.GetComponent<GraphElementScript>().StartConversion(cur_layer);
        }
        else
        {
            Debug.Log("function_menu:"+ menu_object.transform.parent.name);
            if (cur_layer == "graph")
            {
                //menu_object.transform.parent.GetChild(1).gameObject.SetActive(false);
                menu_object.GetComponent<FunctionMenuScript>().Show();
            }
            else if (cur_layer == "simplicial" && menu_object.transform.parent.GetChild(1).tag != "graph")
                StartCoroutine(functionSliderInitiate(cur_layer));
            else
                StartCoroutine(RunConversion(cur_layer));
                //parent.GetComponent<GraphElementScript>().StartConversion(cur_layer);
            
        }
        
    }

    IEnumerator functionSliderInitiate(string cur_layer)
    {
        Transform cur_function_line = menu_object.transform.parent;
        menu_object.GetComponent<FunctionMenuScript>().InitiateFunctionCallHelper();

        while (cur_function_line.GetComponent<FunctionElementScript>().vector_analysis_done == false)
        {
            //"waiting_until_i_am_finished_executing"
            yield return null;
        }

        yield return null;
        this.parent = cur_function_line.GetChild(1).gameObject;
        parent.GetComponent<GraphElementScript>().StartConversion(cur_layer);
    }

    IEnumerator RunConversion(string cur_layer)
    {
        // wait until current conversion is done
        while (true)
        {
            Debug.Log("Conversion Waiting");
            yield return null;
            if (this.parent.GetComponent<GraphElementScript>().conversion_done)
            {
                Debug.Log("Conversion Done");
                break;
            }
        }

        this.parent.GetComponent<GraphElementScript>().StartConversion(cur_layer, transform.parent.gameObject);
    }
}
