using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using UnityEngine.EventSystems;

public class DropDownMenu : MonoBehaviour
{
    public GameObject toggle_parent;
    public GameObject details_dropdown;
    public bool nodes_visibility;
    public static bool isPointerOverDropDown = false;

    //  prefabs
    public GameObject ImageIconicElement, Objects_parent;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("ToggleSetup");
        nodes_visibility = true;
    }

    IEnumerator ToggleSetup()
    {
        yield return null;

        Toggle toggle_1 = toggle_parent.transform.GetChild(0).GetComponent<Toggle>();
        toggle_1.onValueChanged.AddListener(delegate { ShowNodes(toggle_1); });

        Toggle toggle_2 = toggle_parent.transform.GetChild(1).GetComponent<Toggle>();
        toggle_2.onValueChanged.AddListener(delegate { ShowEdges(toggle_2); });

        Toggle toggle_3 = toggle_parent.transform.GetChild(2).GetComponent<Toggle>();
        toggle_3.onValueChanged.AddListener(delegate { ShowFunctionLasso(toggle_3); });

        Toggle toggle_4 = toggle_parent.transform.GetChild(3).GetComponent<Toggle>();
        toggle_4.onValueChanged.AddListener(delegate { ShowFunctionAnchor(toggle_4); });

        Toggle toggle_5 = toggle_parent.transform.GetChild(4).GetComponent<Toggle>();
        toggle_5.onValueChanged.AddListener(delegate { ShowVideoPlayer(toggle_5); });

        Toggle toggle_6 = toggle_parent.transform.GetChild(5).GetComponent<Toggle>();
        toggle_6.onValueChanged.AddListener(delegate { ShowGraphDetails(toggle_6); });


    }

    public void applyDetails()
    {        
        bool state = details_dropdown.GetComponent<DropdownMultiSelect>().transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetComponent<Toggle>().isOn;

        if (state != nodes_visibility)
        {
            nodes_visibility = state;
            GameObject[] node_par = GameObject.FindGameObjectsWithTag("graph");

            foreach (GameObject vp in node_par)
            {
                vp.transform.GetChild(0).gameObject.SetActive(nodes_visibility);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        // applyDetails();
        /*if (EventSystem.current.IsPointerOverGameObject(0))
        {
            isPointerOverDropDown = true;
        }
        else
        {
            isPointerOverDropDown = false;
        }*/
    }

    public void ShowNodes(Toggle toggle)
    {
        //StartCoroutine(ShowNodesResult(toggle));
        bool visibility = toggle.isOn;
        
        GameObject[] graphs = GameObject.FindGameObjectsWithTag("graph");

        foreach (GameObject graph in graphs)
        {
            //graph.transform.GetChild(0).gameObject.SetActive(visibility);
            graph.GetComponent<GraphElementScript>().ShowNodes(toggle);
        }
    }
    
    public void ShowEdges(Toggle toggle)
    {
        GameObject[] graphs = GameObject.FindGameObjectsWithTag("graph");

        foreach (GameObject graph in graphs)
        {
            graph.GetComponent<GraphElementScript>().ShowEdges(toggle);
            /*for (int i = 1; i < 4; i++)
            {
                graph.transform.GetChild(i).gameObject.SetActive(toggle.isOn);
            }*/
        }
    }

    public void ShowFunctionLasso(Toggle toggle)
    {
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in functions)
        {
            cur_function.GetComponent<MeshRenderer>().enabled = toggle.isOn;
        }
    }

    public void ShowGraphDetails(Toggle toggle)
    {
        GameObject[] graphs = GameObject.FindGameObjectsWithTag("graph");

        if (toggle.isOn)
        {
            StartCoroutine(GraphDetailsShow(graphs));
            /*int i = 0; 
            foreach (GameObject graph in graphs)
            {
                graph.GetComponent<GraphElementScript>().GraphDetails(true, i);
                i++;
            }*/
        }
        else
        {
            foreach (GameObject graph in graphs)
            {
                var graph_details = graph.GetComponent<GraphElementScript>().graph_Details;
                if (graph_details != null)
                    Destroy(graph_details);
            }
        }

    }

    IEnumerator GraphDetailsShow(GameObject[] graphs)
    {
        int i = 0;
        foreach (GameObject graph in graphs)
        {
            if (graph == null) continue;

            graph.GetComponent<GraphElementScript>().GraphDetails(true, i);
            i++;
            yield return null;
        }
    }

    public void ShowFunctionAnchor(Toggle toggle)
    {
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in functions)
        {
            if (cur_function.transform.childCount == 0) continue; 
            
            cur_function.transform.GetChild(0).GetChild(0).gameObject.SetActive(toggle.isOn);
        }
    }

    public void ShowVideoPlayer(Toggle toggle)
    {
        GameObject[] vps = GameObject.FindGameObjectsWithTag("video_player");
        int idx = 0;

        foreach (GameObject cur_vp in vps)
        {
            cur_vp.transform.parent.GetChild(1).gameObject.SetActive(toggle.isOn);
            cur_vp.GetComponent<MeshRenderer>().enabled = toggle.isOn;            

            /*GameObject slider = cur_vp.transform.parent.GetComponent<VideoPlayerChildrenAccess>().slider;

            if (slider.GetComponent<VideoController>().graph_holder != null)
            {
                slider.GetComponent<VideoController>().graph_holder.SetActive(toggle.isOn);
            }*/

            idx++;

            /*Texture2D tex = cur_vp.transform.parent.GetComponent<VideoPlayerChildrenAccess>().slider.
                GetComponent<VideoController>().DumpRenderTexture();

            GameObject temp = Instantiate(ImageIconicElement, new Vector3(0, 0, -40f), Quaternion.identity, Objects_parent.transform);
            temp.tag = "iconic";

            temp.GetComponent<iconicElementScript>().image_icon = true;
            temp.GetComponent<iconicElementScript>().LoadSprite(tex);*/
        }
    }

}
