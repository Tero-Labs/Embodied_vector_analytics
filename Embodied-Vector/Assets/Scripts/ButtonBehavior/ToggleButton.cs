using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public GameObject graph_parent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onvaluechanged()
    {
        if (transform.GetComponent<Toggle>().isOn)
            whenSelected();
        else
            whenDeselected();
    }

    public void whenSelected()
    {
        if (this.name == "Graph_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().graph_lock = true;
        }

        else if (this.name == "simplicial_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().simplicial_lock = true;
        }

        else if (this.name == "hypergraph_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().hyper_edges_lock = true;
        }
        else if (this.name == "abstract_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().abstract_lock = true;
        }
    }

    public void whenDeselected()
    {

        if (this.name == "Graph_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().graph_lock = false;
        }

        else if (this.name == "simplicial_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().simplicial_lock = false;
        }

        else if (this.name == "hypergraph_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().hyper_edges_lock = false;
        }
        else if (this.name == "abstract_toggle")
        {
            graph_parent.GetComponent<GraphElementScript>().abstract_lock = false;
        }
    }
}
