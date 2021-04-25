using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using TMPro;

public class RadialSliderValueListener : MonoBehaviour
{
    public RadialSlider rad_slider;
    public TMP_Text tmptextlabel;
    float prev_val;
    public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        prev_val = rad_slider.currentValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator setup_slider()
    {
        yield return null;
        tmptextlabel.text = parent.GetComponent<GraphElementScript>().abstraction_layer;
        string str = parent.GetComponent<GraphElementScript>().abstraction_layer;

        if (str == "graph")
        {
            rad_slider.currentValue = 0f;
            rad_slider.SliderValue = 0f;
        }

        else if (str == "simplicial")
        {
            rad_slider.currentValue = 1f;
            rad_slider.SliderValue = 1f;
        }

        else if (str == "hypergraph")
        {
            rad_slider.currentValue = 2f;
            rad_slider.SliderValue = 2f;
        }
        else
        {
            rad_slider.currentValue = 3f;
            rad_slider.SliderValue = 3f;
        }

        rad_slider.SliderAngle = rad_slider.currentValue * 90f;
        Debug.Log("cur_val: " + rad_slider.currentValue.ToString() + " , " + rad_slider.SliderAngle.ToString());
        rad_slider.UpdateUI();

        /*float normalizedAngle = rad_slider.SliderAngle / 360.0f;
        Debug.Log("manual_normalizedAngle: " + normalizedAngle.ToString());
        rad_slider.indicatorPivot.transform.localEulerAngles = new Vector3(180.0f, 0.0f, normalizedAngle);
        rad_slider.sliderImage.fillAmount = normalizedAngle;*/
    }

    public void setup()
    {
        // UpdateUI is called when radial slider in initialized, hence we use a coroutine to call the UpdateUI in the following frame 
        StartCoroutine("setup_slider");
    }

    public void check()
    {
        if(prev_val != rad_slider.SliderValue)
        {
            Debug.Log(rad_slider.SliderValue);
            if (rad_slider.SliderValue == 0f || rad_slider.SliderValue == 4f)
            {
                //parent.GetComponent<GraphElementScript>().abstraction_layer = "graph";
                parent.GetComponent<GraphElementScript>().StartConversion("graph");
            }
            else if (rad_slider.SliderValue == 1f)
            {
                //parent.GetComponent<GraphElementScript>().abstraction_layer = "simplicial";
                parent.GetComponent<GraphElementScript>().StartConversion("simplicial");
            }
            else if (rad_slider.SliderValue == 2f)
            {
                //parent.GetComponent<GraphElementScript>().abstraction_layer = "hypergraph";
                parent.GetComponent<GraphElementScript>().StartConversion("hypergraph");
            }
            else
            {
                //parent.GetComponent<GraphElementScript>().abstraction_layer = "abstract";
                parent.GetComponent<GraphElementScript>().StartConversion("abstract");
            }

        }

        prev_val = rad_slider.SliderValue;
    }
}
