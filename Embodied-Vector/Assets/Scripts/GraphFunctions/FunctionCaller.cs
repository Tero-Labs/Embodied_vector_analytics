using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FunctionCaller : MonoBehaviour
{
    private HelloRequester _helloRequester;
    private string graphs_as_string;

    public GameObject[] selected_final_graphs;

    private void Start()
    {

    }

    /*public void GetGraphStrings(GameObject[] selected_graphs)
    {
        graphs_as_string = "";
        for (int i = 0; i < selected_graphs.Length; i++)
        {
            if (selected_graphs[i].tag != "graph") continue;

            //selected_graphs[i].GetComponent<GraphElementScript>().Graph_as_Str();
            graphs_as_string = graphs_as_string + selected_graphs[i].GetComponent<GraphElementScript>().nodes_str + "-" + 
                selected_graphs[i].GetComponent<GraphElementScript>().edges_str + "-" +
                selected_graphs[i].GetComponent<GraphElementScript>().simplicial_str + "-" +
                selected_graphs[i].GetComponent<GraphElementScript>().hyper_edges_str + "+" ;
        }
        graphs_as_string = graphs_as_string.Remove(graphs_as_string.Length - 1);

        Debug.Log("combined_string: " + graphs_as_string);
        GetGraphJson(selected_graphs);
    }*/

    public void GetGraphJson(GameObject[] selected_graphs, string function_name)
    {
        this.selected_final_graphs = (GameObject[]) selected_graphs.Clone();

        for (int i = 0; i < selected_final_graphs.Length; i++)
        {
            if (selected_final_graphs[i].tag == "function")
                this.selected_final_graphs[i] = selected_final_graphs[i].transform.GetChild(1).gameObject;
        }

        bool coercion = false;

        string abs_layer = "";
        for (int i = 0; i < selected_final_graphs.Length; i++)
        {
            if (selected_final_graphs[i].tag != "graph") continue;
            if (abs_layer == "")
                abs_layer = selected_final_graphs[i].GetComponent<GraphElementScript>().abstraction_layer;
            else
            {
                if (abs_layer != selected_final_graphs[i].GetComponent<GraphElementScript>().abstraction_layer)
                {
                    Debug.Log("coercion needed");
                    coercion = true;
                    break;
                }
            }
        }

        if (coercion)
        {
            StartCoroutine(RunCoercionModel(selected_final_graphs, function_name));
            return;
        }

        Debug.Log("no coercion needed");
        Graphs graphs = new Graphs();
        graphs.graphs = new List<Graph>();

        for (int i = 0; i < selected_final_graphs.Length; i++)
        {
            if (selected_final_graphs[i].tag != "graph") continue;

            graphs.graphs.Add(selected_final_graphs[i].GetComponent<GraphElementScript>().graph);
        }

        Debug.Log(JsonUtility.ToJson(graphs));
        File.WriteAllText("Assets/Resources/" + "data.json", JsonUtility.ToJson(graphs));
        Function_Caller(function_name);
    }

    IEnumerator RunCoercionModel(GameObject[] selected_graphs, string function_name)
    {
        Graphs graphs = new Graphs();
        graphs.graphs = new List<Graph>();

        for (int i = 0; i < selected_graphs.Length; i++)
        {
            if (selected_graphs[i].tag != "graph") continue;

            // if already graph, no more change needed
            if (selected_graphs[i].GetComponent<GraphElementScript>().abstraction_layer == "graph")
            {
                graphs.graphs.Add(selected_graphs[i].GetComponent<GraphElementScript>().graph);
                continue;
            }                

            selected_graphs[i].GetComponent<GraphElementScript>().StartConversion("graph");

            // wait until current conversion is done
            while(true)
            {
                yield return null;
                if (selected_graphs[i].GetComponent<GraphElementScript>().conversion_done)
                {
                    Debug.Log("Conversion Done");
                    break;
                }                    
            }

            graphs.graphs.Add(selected_graphs[i].GetComponent<GraphElementScript>().graph);
        }

        Debug.Log(JsonUtility.ToJson(graphs));
        File.WriteAllText("Assets/Resources/" + "data.json", JsonUtility.ToJson(graphs));
        Function_Caller(function_name);
    }

    private void OnDestroy()
    {
        //_helloRequester.Stop();
    }

    public bool Function_Caller(string function_name)
    {
        if (function_name == "shortestpath" || function_name == "shortestpathlength")
        {
            function_name += "_" + selected_final_graphs[1].GetComponent<iconicElementScript>().icon_number.ToString();
            function_name += "_" + selected_final_graphs[2].GetComponent<iconicElementScript>().icon_number.ToString();
        }
        else if (function_name == "egograph" || function_name == "neighborgraph")
        {
            function_name += "_" + selected_final_graphs[1].GetComponent<iconicElementScript>().icon_number.ToString();
            function_name += "_" + transform.GetChild(0).GetComponent<FunctionMenuScript>().cur_arg_Str[2].ToString();
        }

        if (_helloRequester != null)
        {
            //bool flag = true;
            // housekeeping so that an already running thread does not throw netmq exception 
            if (_helloRequester.isalive())
            {
                return false;
            }
            //Debug.Log("checking is alive: " + _helloRequester.isalive().ToString() + " flag: " + flag.ToString());
        }

        //transform.GetComponent<StartServer>().ExecuteCommand(function_name);
        _helloRequester = new HelloRequester();
        _helloRequester.graph_as_str = function_name; //graphs_as_string; //"{8,9,7,10}-{8,9}{9,7}{8,7}+{8,9,7,10}-{8,9}{9,7}{8,7}";
        _helloRequester.command = function_name;
        _helloRequester.Start();
        return true;
    }

    void Update()
    {
        if (_helloRequester != null)
        {
            if (_helloRequester.serverUpdateCame)
            {
                _helloRequester.serverUpdateCame = false;
                //show_your_output_as_you_want
                transform.GetComponent<FunctionElementScript>().ServerOutputProcessing(_helloRequester.serverUpdate);
            }
        }

    }
}
