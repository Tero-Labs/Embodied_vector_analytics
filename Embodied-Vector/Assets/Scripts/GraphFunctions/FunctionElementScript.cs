using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using System.IO;

public class FunctionElementScript : MonoBehaviour
{
    // for vector grid creation
    public int gridmaxx, gridmaxy, gridminx, gridminy;

    // enclosing vectors
    public List<GameObject> selected_vectors;
    public Vector[,] spawnGrid;

    // for vector operation and server connection
    public VectorField vectorField;

    // object drawing properties
    public List<Vector3> points = new List<Vector3>();
    public Vector3 centroid;
    public float maxx = -100000f, maxy = -100000f, minx = 100000f, miny = 100000f;
    public bool draggable_now = false;

    public Material icon_elem_material;
    public GameObject paintable_object;    
    public string icon_name;
    // needed for abstraction conversion
    public int icon_number;
    public int edge_offset;

    public GameObject mesh_holder;
    public GameObject function_prefab;

   
    // movement related variables
    public Vector3 previous_position;	// used for checking if the pen object is under a set or function
    public Vector3 joint_centroid;

    // path record
    public List<Vector3> recorded_path = new List<Vector3>();
    public bool record_path_enable = false;
    public Vector3 position_before_record;
    public Vector3 edge_position;

    public Dictionary<string, Transform> nodeMaps;

   
    // other interaction variables
    private Vector3 touchDelta = new Vector3();
    private Vector3 menu_center = new Vector3();

    // pressure based line width
    public List<float> pressureValues = new List<float>();
    public AnimationCurve widthcurve = new AnimationCurve();
    float totalLength = 0f;
    List<float> cumulativeLength = new List<float>();
    int movingWindow = 3;

    // Pen Interaction
    Pen currentPen;

    // double function related variables
    public bool is_this_double_function_operand = false;

    // play log related variables
    public Dictionary<string, Vector2> saved_positions = new Dictionary<string, Vector2>();

    // global stroke details
    //public GameObject details_dropdown;
    public bool global_details_on_path = true;
    public bool fused_function;
    public bool vector_analysis_done;

    public GameObject function_menu;
    public GameObject video_player;

    public InputField mainInputField;
    public GameObject[] grapharray;

    // prefabs
    public GameObject graph_prefab;
    public GameObject topo_label;
    public GameObject scalar_label;

    // server output store
    Graphs returned_graphs;
    Graph returned_graph;
    bool cascaded_lasso = false;
    public string output;

    IEnumerator clear_files()
    {
        //File.Delete("Assets/Resources/" + "output.json");
        File.Delete("Assets/Resources/" + "data.json");
        vector_analysis_done = true;

        if (video_player != null && 
            video_player.transform.parent.GetComponent<VideoPlayerChildrenAccess>().PlayorPause.GetComponent<VideoPlayPause>().playFlag)
            
        {
            video_player.transform.GetComponent<VideoPlayer>().Play();
        }

        
        transform.GetChild(0).GetComponent<FunctionMenuScript>().UIsetafterEval(output);

        // restoring their inactiveness
        for (int i = 0; i < transform.GetComponent<FunctionCaller>().selected_final_graphs.Length; i++)
        {
            if (transform.GetComponent<FunctionCaller>().selected_final_graphs[i].name == "temp_graph")
            {
                // if the object is a temp video graph, restore the argument object for enabling recalculation
                if (transform.GetComponent<FunctionCaller>().selected_final_graphs[i].GetComponent<GraphElementScript>().video_graph)
                    transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects[i] =
                        transform.GetComponent<FunctionCaller>().selected_final_graphs[i].GetComponent<GraphElementScript>().parent_graph;

                if (transform.GetChild(0).GetComponent<FunctionMenuScript>().eval_finished || 
                    transform.GetComponent<FunctionCaller>().selected_final_graphs[i].GetComponent<GraphElementScript>().video_graph)
                    Destroy(transform.GetComponent<FunctionCaller>().selected_final_graphs[i]);
                else
                    transform.GetComponent<FunctionCaller>().selected_final_graphs[i].SetActive(false);
            }                
        }

        if (video_player != null)
            transform.GetChild(0).GetComponent<FunctionMenuScript>().videohide();

        yield return null;

        // update history when execution done
        Debug.Log("history calling");
        StartCoroutine(GameObject.Find("Paintable").GetComponent<Paintable>().HistoryModify(transform.gameObject));
    }

    // https://forum.unity.com/threads/declaring-and-initializing-2d-arrays-in-c.52954/
    public void InstantiateMatrix()
    {
        spawnGrid = new Vector[gridmaxx - gridminx + 1, gridmaxy - gridminy + 1];

        for (int i = 0; i < gridmaxx - gridminx + 1; i++)
        {
            for (int j = 0; j < gridmaxy - gridminy + 1; j++)
            {
                spawnGrid[i, j] = new Vector
                {
                    x = i,
                    y = j,
                    f_x = 0,
                    f_y = 0
                };
            }
        }

        int temp_i, temp_j;
        foreach (Vector vector in vectorField.vectors)
        {
            temp_i = vector.x - gridminx;
            temp_j = vector.y - gridminy;
            spawnGrid[temp_i, temp_j] = vector;
        }
    }

    public void InstantiateNameBox()
    {
        //Instantiate(name_label, new Vector3(maxx, maxy, -5), Quaternion.identity, transform);
        GameObject temp = Instantiate(function_menu, new Vector3(maxx, maxy, -10), Quaternion.identity, transform);
        temp.transform.SetSiblingIndex(0);
        temp.GetComponent<FunctionMenuScript>().paintable = paintable_object;

        paintable_object.GetComponent<Paintable>().no_func_menu_open = false;
    }

    // ToDo: instantiate the graph somewhere else? perform the toggle operation here as well
    public void ServerOutputProcessing(string graph_as_Str)
    {
        output = graph_as_Str;
        // if returned as a graph
        // destroy previous function outputs, if any
        Destroy(transform.GetChild(1).gameObject);
        
        transform.GetChild(0).GetComponent<FunctionMenuScript>().PostProcess(graph_as_Str);
    }

    // https://www.mathworks.com/help/matlab/ref/divergence.html
    public void DivergenceCalculate()
    {
        float div_result_x = 0, div_result_y = 0;

        // interior points
        for (int i = 1; i < gridmaxx - gridminx; i++)
        {
            for (int j = 1; j < gridmaxy - gridminy; j++)
            {
                /*div_result_x = (spawnGrid[i + 1, j].f_x - spawnGrid[i - 1, j].f_x) /(float)(spawnGrid[i + 1, j].x - spawnGrid[i - 1, j].x);
                div_result_y = (spawnGrid[i, j + 1].f_y - spawnGrid[i, j - 1].f_y) /(float)(spawnGrid[i, j + 1].y - spawnGrid[i, j - 1].y);*/
                if ((spawnGrid[i, j + 1].x - spawnGrid[i, j - 1].x) > 0)
                    div_result_x = (spawnGrid[i, j + 1].f_x - spawnGrid[i, j - 1].f_x) / (float)(spawnGrid[i, j + 1].x - spawnGrid[i, j - 1].x);
                if ((spawnGrid[i + 1, j].y - spawnGrid[i - 1, j].y) > 0)
                    div_result_y = (spawnGrid[i + 1, j].f_y - spawnGrid[i - 1, j].f_y) / (float)(spawnGrid[i + 1, j].y - spawnGrid[i - 1, j].y);
            }
        }

        //  left and right edges        
        for (int i = 0; i < gridmaxx - gridminx + 1; i++)
        {                    
            int j = 0;
            //div_result_x = (spawnGrid[i + 1, j].f_x - spawnGrid[i, j].f_x) / (float)(spawnGrid[i + 1, j].x - spawnGrid[i, j].x);
            if ((spawnGrid[i, j + 1].x - spawnGrid[i, j].x) > 0)
                div_result_x = (spawnGrid[i, j + 1].f_x - spawnGrid[i, j].f_x) / (float)(spawnGrid[i, j + 1].x - spawnGrid[i, j].x);

            j = gridmaxy - gridminy;
            if ((spawnGrid[i, j].x - spawnGrid[i, j - 1].x) > 0)
                div_result_x = (spawnGrid[i, j].f_x - spawnGrid[i, j - 1].f_x) / (float)(spawnGrid[i, j].x - spawnGrid[i, j - 1].x);
        }

        //  top and bottom edges        
        for (int j = 0; j < gridmaxy - gridminy + 1; j++)
        {
            int i = 0;
            //div_result_y = (spawnGrid[i, j + 1].f_y - spawnGrid[i, j].f_y) /(float)(spawnGrid[i, j + 1].y - spawnGrid[i, j].y);    
            if ((spawnGrid[i + 1, j].y - spawnGrid[i, j].y) > 0)
                div_result_y = (spawnGrid[i + 1, j].f_y - spawnGrid[i, j].f_y) / (float)(spawnGrid[i + 1, j].y - spawnGrid[i, j].y);

            i = gridmaxx - gridminx;
            if ((spawnGrid[i, j].y - spawnGrid[i - 1, j].y) > 0)
                div_result_y = (spawnGrid[i, j].f_y - spawnGrid[i - 1, j].f_y) / (float)(spawnGrid[i, j].y - spawnGrid[i - 1, j].y);
        }

        InstantiateScalarOutput((div_result_x+div_result_y).ToString());
    }

    public void InstantiateScalarOutput(string output)
    {
        GameObject temp_label = Instantiate(scalar_label, new Vector3(maxx, maxy, -10), Quaternion.identity, transform);
        temp_label.transform.SetSiblingIndex(1);
        temp_label.name = "label";

        temp_label.GetComponent<topoLabelScript>().tmptextlabel.text = output;

        temp_label.GetComponent<topoLabelScript>().functiontextlabel.text = 
            transform.GetChild(0).GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text;

        //temp_label.GetComponent<TextMeshProUGUI>().fontSize = 26;
        StartCoroutine(clear_files());
    }

    public void InstantiateGraph()
    {
        GameObject tempgraph = Instantiate(graph_prefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
        tempgraph.transform.SetSiblingIndex(1);

        Paintable.graph_count++;

        tempgraph.name = "graph_" + Paintable.graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();
        tempgraph.GetComponent<GraphElementScript>().paintable = paintable_object;
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "graph";

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;
        GameObject tempsimplicialparent = tempgraph.transform.GetChild(2).gameObject;
        GameObject temphyperparent = tempgraph.transform.GetChild(3).gameObject;

        tempgraph.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();
        nodeMaps = new Dictionary<string, Transform>();

        
        returned_graph = JsonUtility.FromJson<Graph>(File.ReadAllText("Assets/Resources/" + "output.json"));

        tempgraph.GetComponent<GraphElementScript>().graph = new Graph();
        tempgraph.GetComponent<GraphElementScript>().graph.nodes = new List<int>();

        foreach (int current_node in returned_graph.nodes)
        {
            foreach (GameObject current_graph in transform.GetComponent<FunctionCaller>().selected_final_graphs)
            {
                if (current_graph.tag == "graph")
                {
                    if (current_graph.GetComponent<GraphElementScript>().nodeMaps.ContainsKey(current_node.ToString()))
                    {
                        Transform prev_Trasform = current_graph.GetComponent<GraphElementScript>().nodeMaps[current_node.ToString()];
                        GameObject tempicon = Instantiate(prev_Trasform.gameObject, prev_Trasform.position, Quaternion.identity, tempnodeparent.transform);
                        
                        tempgraph.GetComponent<GraphElementScript>().graph.nodes.Add(current_node);
                        nodeMaps.Add(current_node.ToString(), tempicon.transform);
                        tempgraph.GetComponent<GraphElementScript>().nodeMaps.Add(current_node.ToString(), tempicon.transform);

                        //Debug.Log("creating " + current_node.ToString() + " node");
                        break;
                    }
                }
                // ToDo: else if iconic, direct copy if needed later
            }
        }
        
        foreach (Edge edge in returned_graph.edges)
        {
            string[] nodes_of_edge = new string[2];
            nodes_of_edge[0] = edge.edge_start.ToString();
            nodes_of_edge[1] = edge.edge_end.ToString();

            // simplicial may have subsets of length 1, will skip those
            if (nodes_of_edge.Length != 2)
                continue;

            EdgeCreation("edge", nodes_of_edge, 1);
        }
                
        tempgraph.GetComponent<GraphElementScript>().edges_init();
        //tempgraph.GetComponent<GraphElementScript>().Graph_init();
               

        StartCoroutine(clear_files());
    }

    /*public void InstantiateGraph(string graph_as_Str)
    {
        Debug.Log("graph_as_Str: " + graph_as_Str);
        GameObject tempgraph = Instantiate(graph_prefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
        tempgraph.transform.SetSiblingIndex(1);

        paintable_object.GetComponent<Paintable>().graph_count++;

        tempgraph.name = "graph_" + paintable_object.GetComponent<Paintable>().graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + paintable_object.GetComponent<Paintable>().graph_count.ToString();
        tempgraph.GetComponent<GraphElementScript>().paintable = paintable_object;
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "graph";

        GameObject tempnodeparent = new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);

        GameObject tempedgeparent = new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);

        GameObject tempsimplicialparent = new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);

        GameObject temphyperparent = new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);

        nodeMaps = new Dictionary<string, Transform>();

        //nodes_init
        string nodes_str = (graph_as_Str.Split('+'))[0];
        string[] each_node = nodes_str.Split(',');        

        foreach (string current_node in each_node)
        {
            foreach (GameObject current_graph in transform.GetComponent<FunctionCaller>().selected_graphs)
            {
                if (current_graph.tag == "graph")
                {
                    if (current_graph.GetComponent<GraphElementScript>().nodeMaps.ContainsKey(current_node))
                    {
                        Transform prev_Trasform = current_graph.GetComponent<GraphElementScript>().nodeMaps[current_node];
                        GameObject tempicon = Instantiate(prev_Trasform.gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity, tempnodeparent.transform);
                        paintable_object.GetComponent<Paintable>().totalLines++;
                        tempicon.name = "iconic_" + paintable_object.GetComponent<Paintable>().totalLines.ToString();
                        tempicon.GetComponent<iconicElementScript>().icon_number = paintable_object.GetComponent<Paintable>().totalLines;

                        nodeMaps.Add(current_node, tempicon.transform);

                        //tempicon.GetComponent<MeshFilter>().sharedMesh = tempicon.GetComponent<MeshFilter>().sharedMesh;
                        break;
                    }
                }
                // ToDo: else if iconic, direct copy if needed later
            }
        }

        //edges_init
        string[] newedges = ((graph_as_Str.Split('+'))[1]).Split('-');
        foreach (string edge in newedges)
        {
            string[] nodes_of_edge = edge.Split(',');

            // simplicial may have subsets of length 1, will skip those
            if (nodes_of_edge.Length != 2)
                continue;

            EdgeCreation("edge", nodes_of_edge, 1);
        }

        tempgraph.GetComponent<GraphElementScript>().Graph_init();
        //tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
    }
    */

    public void InstantiateTopoGraph()
    {
        GameObject graph = transform.GetComponent<FunctionCaller>().selected_final_graphs[0];

        GameObject temp_graph = Instantiate(graph);
        temp_graph.transform.parent = transform;
        temp_graph.transform.SetSiblingIndex(1);
        temp_graph.SetActive(true);

        Paintable.graph_count++;
        temp_graph.name = "graph_" + Paintable.graph_count.ToString();
        temp_graph.tag = "graph";
        temp_graph.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();
        
        // remap node dictionary
        temp_graph.GetComponent<GraphElementScript>().graph = new Graph();
        temp_graph.GetComponent<GraphElementScript>().nodes_init();
        nodeMaps = temp_graph.GetComponent<GraphElementScript>().nodeMaps;

        if (transform.GetChild(0).GetComponent<FunctionMenuScript>().cur_arg_Str[1] == "in-place")
        {
            GameObject extra_objects = new GameObject("labels_overlay");
            extra_objects.transform.parent = temp_graph.transform;
            extra_objects.transform.SetSiblingIndex(5);
                        
            returned_graph = JsonUtility.FromJson<Graph>(File.ReadAllText("Assets/Resources/" + "output.json"));
            int index = 0;
            int temp_cnt = returned_graph.nodes.Count;

            foreach (int current_node in returned_graph.nodes)
            {
                if (nodeMaps.ContainsKey(current_node.ToString()))
                {
                    Transform child = nodeMaps[current_node.ToString()];

                    GameObject temp_label = Instantiate(topo_label);
                    //temp_label.transform.SetParent(extra_objects.transform);
                    temp_label.transform.SetParent(child.transform);
                    temp_label.name = "label";

                    temp_label.transform.position = child.GetComponent<iconicElementScript>().edge_position +
                        new Vector3(child.GetComponent<iconicElementScript>().radius, child.GetComponent<iconicElementScript>().radius + 8, 0);

                    index++;
                    temp_label.GetComponent<TextMeshProUGUI>().text = index.ToString();

                    int temp_size = (int)(Mathf.Lerp(26, 12, (index / (float)temp_cnt)));
                    Color temp_color = Color.Lerp(new Color32(102, 0, 102, 255), new Color32(255, 26, 255, 255), (index / (float)temp_cnt));
                    
                    /*Debug.Log("index: " + index.ToString() + "temp_cnt: " + temp_cnt.ToString() + "percent: " + (index / (float)temp_cnt).ToString() +
                        "temp_size: " + temp_size.ToString() + ", temp_color: " + temp_color.ToString());*/

                    temp_label.GetComponent<TextMeshProUGUI>().fontSize = temp_size;
                    temp_label.GetComponent<TextMeshProUGUI>().color = temp_color;
                }
            }                                 
            
        }
        else
        {
            temp_graph.GetComponent<GraphElementScript>().splined_edge_flag = true;
            int iter = 0;
            Vector3 position = Vector3.zero;
            returned_graph = JsonUtility.FromJson<Graph>(File.ReadAllText("Assets/Resources/" + "output.json"));

            foreach (int current_node in returned_graph.nodes)
            {
                if (nodeMaps.ContainsKey(current_node.ToString()))
                {
                    Transform child = nodeMaps[current_node.ToString()];
                    iter++;

                    if (iter == 1)
                    {
                        position = /*child.position - */child.GetComponent<iconicElementScript>().edge_position +
                            new Vector3(child.GetComponent<iconicElementScript>().radius, 0, 0);
                    }
                    else
                    {
                        if(child.GetComponent<iconicElementScript>().image_icon)
                            position += new Vector3(child.GetComponent<iconicElementScript>().radius, 0, 0);
                        else
                            position += new Vector3(2 * child.GetComponent<iconicElementScript>().radius, 0, 0);

                        Vector3 old_pos = child.position;
                        // we want the edge_position to project
                        Vector3 new_pos = child.InverseTransformDirection(position) -
                            child.InverseTransformDirection(child.GetComponent<iconicElementScript>().bounds_center);

                        child.position = new Vector3(new_pos.x, new_pos.y, -40f);

                        child.GetComponent<iconicElementScript>().edge_position = position;
                        Debug.Log(child.name);

                        position += new Vector3(child.GetComponent<iconicElementScript>().radius, 0, 0);

                    }

                }                
            }

            Transform edge_parent = temp_graph.transform.GetChild(1);            
            for (int i = 0; i < edge_parent.childCount; i++)
            {
                if (edge_parent.GetChild(i).tag == "edge")
                    edge_parent.GetChild(i).GetComponent<VectorElementScript>().updateSplineEndPoint();
            }
            
        }

        
        StartCoroutine(clear_files());
    }

    public void InstantiatePathGraph()
    {
        GameObject graph = transform.GetComponent<FunctionCaller>().selected_final_graphs[0];

        GameObject temp_graph = Instantiate(graph);
        temp_graph.transform.parent = transform;
        temp_graph.transform.SetSiblingIndex(1);
        temp_graph.SetActive(true);

        Destroy(temp_graph.transform.GetChild(0).gameObject);
        Destroy(temp_graph.transform.GetChild(1).gameObject);
        Destroy(temp_graph.transform.GetChild(2).gameObject);
        Destroy(temp_graph.transform.GetChild(3).gameObject);

        GameObject tempnodeparent = new GameObject("node_parent");
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = temp_graph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);

        GameObject tempedgeparent = new GameObject("edge_parent");
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = temp_graph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);

        GameObject tempsimplicialparent = new GameObject("simplicial_parent");
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = temp_graph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);

        GameObject temphyperparent = new GameObject("hyper_parent");
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = temp_graph.transform;
        temphyperparent.transform.SetSiblingIndex(3);

        for (int i = 0; i < 4; i++)
            temp_graph.transform.GetChild(i).transform.localPosition = Vector3.zero;

        //StartCoroutine(set_correct_position(temp_graph));

        Paintable.graph_count++;
        temp_graph.name = "graph_" + Paintable.graph_count.ToString();
        temp_graph.tag = "graph";
        temp_graph.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();

        temp_graph.GetComponent<GraphElementScript>().graph = new Graph();
        temp_graph.GetComponent<GraphElementScript>().graph.nodes = new List<int>();

        //temp_graph.GetComponent<GraphElementScript>().nodes_init();
        nodeMaps = new Dictionary<string, Transform>();
        temp_graph.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();
        //graph.GetComponent<GraphElementScript>().nodes_init();

        returned_graph = JsonUtility.FromJson<Graph>(File.ReadAllText("Assets/Resources/" + "output.json"));

        int index = 0;
        foreach (int current_node in returned_graph.nodes)
        {
            Transform child = graph.GetComponent<GraphElementScript>().nodeMaps[current_node.ToString()];
            GameObject temp_node = Instantiate(child.gameObject);
            temp_node.transform.parent = tempnodeparent.transform;
            temp_node.transform.position = child.transform.position;

            temp_graph.GetComponent<GraphElementScript>().graph.nodes.Add(temp_node.transform.GetComponent<iconicElementScript>().icon_number);
            temp_graph.GetComponent<GraphElementScript>().nodeMaps.Add(temp_node.transform.GetComponent<iconicElementScript>().icon_number.ToString(), temp_node.transform);
            nodeMaps.Add(temp_node.transform.GetComponent<iconicElementScript>().icon_number.ToString(), temp_node.transform);

            index++;
            if (index == 1) continue;

            string[] nodes_of_edge = new string[2];
            nodes_of_edge[0] = returned_graph.nodes[index-2].ToString();
            nodes_of_edge[1] = returned_graph.nodes[index-1].ToString();

            // simplicial may have subsets of length 1, will skip those
            if (nodes_of_edge.Length != 2)
                continue;

            GameObject edgeline = EdgeCreation("edge", nodes_of_edge, 1);
            IEnumerator coroutine = material_update(edgeline);
            StartCoroutine(coroutine);

        }
        
        StartCoroutine(clear_files());
    }

    IEnumerator set_correct_position(GameObject temp_graph)
    {       
        for (int i = 0; i < 4; i++)
            temp_graph.transform.GetChild(i).transform.localPosition = Vector3.zero;
        yield return null;
    }
    
    IEnumerator material_update(GameObject edgeline)
    {
        yield return null;
        edgeline.GetComponent<LineRenderer>().startWidth = 10;
        edgeline.GetComponent<LineRenderer>().endWidth = 10;
        edgeline.GetComponent<LineRenderer>().material.SetColor("_Color", Color.red);
    }

    public void InstantiateCommunityGraph()
    {
        GameObject graph = transform.GetComponent<FunctionCaller>().selected_final_graphs[0];

        if (graph.tag != "graph") return;
                
        GameObject temp_graph = Instantiate(graph);
        temp_graph.transform.parent = transform;
        temp_graph.transform.SetSiblingIndex(1);
        temp_graph.SetActive(true);

        Paintable.graph_count++;
        temp_graph.name = "graph_" + Paintable.graph_count.ToString();
        temp_graph.tag = "graph";
        temp_graph.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();

        /*GameObject extra_objects = new GameObject("labels_overlay");
        extra_objects.transform.parent = temp_graph.transform;
        extra_objects.transform.SetSiblingIndex(5);*/

        // remap node dictionary
        temp_graph.GetComponent<GraphElementScript>().graph = new Graph();
        temp_graph.GetComponent<GraphElementScript>().nodes_init();
        nodeMaps = temp_graph.GetComponent<GraphElementScript>().nodeMaps;

        returned_graphs = JsonUtility.FromJson<Graphs>(File.ReadAllText("Assets/Resources/" + "output.json"));
        cascaded_lasso = true;

        StartCoroutine(community_lasso_initiate(/*extra_objects*/temp_graph));

    }

    IEnumerator community_lasso_initiate(GameObject /*extra_objects*/temp_graph)
    {
        int idx = 0;
        GameObject extra_objects = new GameObject("labels_overlay");
        extra_objects.transform.parent = temp_graph.transform;
        extra_objects.transform.SetSiblingIndex(5);

        foreach (Graph returned_graph in returned_graphs.graphs)
        {
            GameObject functionline = Instantiate(transform.gameObject,
                transform.position, Quaternion.identity, extra_objects.transform);

            for (int i = 0; i < (functionline.transform.childCount - 1); i++)
                Destroy(functionline.transform.GetChild(i).gameObject);

            updatechildLassoPoints(returned_graph.nodes, functionline, idx);            

            idx++;
            Debug.Log("a lasso drawn");
            yield return null;
        }

        
        StartCoroutine(clear_files());
    }

    GameObject EdgeCreation(string tag, string[] nodes_of_edge, int idx)
    {
        List<GameObject> temp_nodes = new List<GameObject>();
        foreach (string node in nodes_of_edge)
        {
            if (nodeMaps.ContainsKey(node))
            {
                temp_nodes.Add(nodeMaps[node].gameObject);
            }
        }

        GameObject source = temp_nodes[0];
        GameObject target = temp_nodes[1];

        Vector3 source_vec = source.GetComponent<iconicElementScript>().getclosestpoint(target.GetComponent<iconicElementScript>().edge_position);
        Vector3 target_vec = target.GetComponent<iconicElementScript>().getclosestpoint(source_vec);


        GameObject edgeline = Instantiate(paintable_object.GetComponent<Paintable>().VectorElement, source_vec, 
            Quaternion.identity, transform.GetChild(1).GetChild(idx));

        paintable_object.GetComponent<Paintable>().selected_obj_count++;
        edgeline.name = "edge_" + paintable_object.GetComponent<Paintable>().selected_obj_count.ToString();
        edgeline.tag = tag;

        edgeline.GetComponent<VectorElementScript>().edge_start = temp_nodes[0];
        edgeline.GetComponent<VectorElementScript>().edge_end = temp_nodes[1];

        edgeline.GetComponent<LineRenderer>().SetPosition(0, source_vec);
        edgeline.GetComponent<LineRenderer>().SetPosition(1, target_vec);
        Destroy(edgeline.GetComponent<TrailRenderer>());

        var edgepoints = new List<Vector3>() { edgeline.GetComponent<LineRenderer>().GetPosition(0), edgeline.GetComponent<LineRenderer>().GetPosition(1) };

        edgeline.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
        {
            var pos = edgeline.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        edgeline.GetComponent<EdgeCollider2D>().edgeRadius = 10;
        edgeline.GetComponent<VectorElementScript>().addDot();

        return edgeline;
    }

    public void AddPoint(Vector3 vec)
    {
        points.Add(vec);
                
        if (maxy < vec.y)
        {
            maxy = vec.y;
            /*if (maxx < vec.x)*/ maxx = vec.x;
        }

        if (miny > vec.y)
        {
            miny = vec.y;
            /*if (minx > vec.x)*/ minx = vec.x;
        }

    }

    public void computeCentroid()
    {
        float totalx = 0, totaly = 0, totalz = 0;
        for (int i = 0; i < points.Count; i++)
        {
            totalx += points[i].x;
            totaly += points[i].y;
            totalz += points[i].z;
        }
        centroid = new Vector3(totalx / points.Count, totaly / points.Count, points[0].z); // including z in the average calc. created problem
                                                                                           // so just used a constant value from the points list.
    }

    public void computeBounds()
    {
        menu_center.x = maxx;
        menu_center.y = maxy;

        maxx = -100000f; maxy = -100000f; minx = 100000f; miny = 100000f;
        int interval = Mathf.Max(1, (int)Math.Floor((float)(points.Count / 10)));

        for (int i = 0; i < points.Count; i = i + interval)
        {
            /*if (maxx < points[i].x) maxx = points[i].x;
            if (maxy < points[i].y) maxy = points[i].y;
            if (minx > points[i].x) minx = points[i].x;
            if (miny > points[i].y) miny = points[i].y;*/
            if (maxx < points[i].x)
            {
                maxx = points[i].x;
                maxy = points[i].y;
            }
            /*if ((Mathf.Abs(maxx - points[i].x) < 15f) && (maxy < points[i].y))
            {
                maxx = points[i].x;
                maxy = points[i].y;
            }*/
            if (minx > points[i].x)
            {
                minx = points[i].x;
                miny = points[i].y;
            }
        }        
    }

    public void fromGlobalToLocalPoints()
    {
        // transform all points from world to local coordinate with respect to the transform position of the set game object
        for (int i = 0; i < points.Count; i++)
        {
            points[i] = transform.InverseTransformPoint(points[i]);
        }
    }

    public List<Vector3> fromLocalToGlobalPoints()
    {
        // Assumes fromGlobalToLocalPoints() has already been called on points set
        List<Vector3> gbpoints = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            gbpoints.Add(transform.TransformPoint(points[i]));
        }

        return gbpoints;
    }

    public void updateLengthFromPoints()
    {
        totalLength = 0f;
        cumulativeLength.Clear();
        for (int i = 1; i < points.Count; i++)
        {
            totalLength += Vector3.Distance(points[i - 1], points[i]);
            cumulativeLength.Add(totalLength);
        }
    }

    public void addPressureValue(float val)
    {
        pressureValues.Add(val);
    }

    public void reNormalizeCurveWidth()
    {
        // create a curve with as many points as the current number of pressure values
        int numPts = cumulativeLength.Count;
        widthcurve = new AnimationCurve();

        if (numPts > movingWindow)
        {
            List<float> smoothedPressureValues = smoothenList(pressureValues);

            for (int i = 0; i < numPts - movingWindow; i++)
            {
                widthcurve.AddKey(cumulativeLength[i] / totalLength, Mathf.Clamp(pressureValues[i], 0f, 1f));
            }
        }
    }

    public List<float> smoothenList(List<float> values)
    {
        // take the width curve (float values) and run a moving average operation
        return Enumerable
                .Range(0, values.Count - movingWindow)
                .Select(n => values.Skip(n).Take(movingWindow).Average())
                .ToList();
    }
  
    public void updateLassoPointsIconDrag()
    {
        // we don't want the function to change it's shape if it is a fused function
        if (!fused_function && mesh_holder.activeSelf /*GetComponent<MeshRenderer>().enabled*/)
        {
            List<Vector3> hull_pts = new List<Vector3>();
            int center_count = 0;
            joint_centroid = Vector3.zero;

            foreach (GameObject function_argument in transform.GetComponent<FunctionCaller>().selected_final_graphs)
            {
                if (function_argument == null) continue;

                if (function_argument.tag == "graph")
                {
                    bool video_flag = false;

                    Transform[] allChildrennode = function_argument.transform.GetChild(0).GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildrennode)
                    {
                        if (child.tag == "iconic")
                        {
                            // to optimize we take special measure for videos
                            if (child.GetComponent<iconicElementScript>().video_icon)
                            {
                                hull_pts.AddRange(function_argument.GetComponent<GraphElementScript>().points);
                                video_flag = true;
                                break;
                            }

                            List<Vector3> returned_pts = child.GetComponent<iconicElementScript>().hullPoints();
                            hull_pts.AddRange(returned_pts);
                            center_count++;
                            joint_centroid += child.GetComponent<iconicElementScript>().edge_position;
                        }
                    }

                    if (video_flag) continue;

                    // we want the edges to stay within the lasso as well 
                    Transform[] allChildrenedge = function_argument.transform.GetChild(1).GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildrenedge)
                    {
                        if (child.tag == "edge")
                        {
                            Vector3[] returned_pts_arr = new Vector3[child.GetComponent<LineRenderer>().positionCount];
                            int temp = child.GetComponent<LineRenderer>().GetPositions(returned_pts_arr);

                            // adding a little offset so that it does not look too tight
                            for (int i = 0; i < returned_pts_arr.Length; i++)
                            {
                                hull_pts.Add(returned_pts_arr[i] - new Vector3(0, edge_offset, 0));
                                hull_pts.Add(returned_pts_arr[i] + new Vector3(0, edge_offset, 0));
                            }

                            /*List<Vector3> returned_pts = returned_pts_arr.ToList();
                            hull_pts.AddRange(returned_pts);*/
                        }

                    }
                }
                else if (function_argument.tag == "iconic")
                {
                    List<Vector3> returned_pts = function_argument.GetComponent<iconicElementScript>().hullPoints();
                    hull_pts.AddRange(returned_pts);
                    center_count++;
                    joint_centroid += function_argument.GetComponent<iconicElementScript>().edge_position;
                }
            }

            joint_centroid = joint_centroid / center_count;

            var hullAPI = new HullAPI();
            var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = hull_pts.ToArray(), Concavity = 3000 });

            Vector3[] vertices = hull.vertices;
            //Array.Sort(vertices);
            Debug.Log("hulled: ");
                       

            mesh_holder.GetComponent<MeshFilter>().sharedMesh.Clear();
            transform.GetComponent<LineRenderer>().enabled = true;

            points = vertices.ToList();

            paintable_object.GetComponent<CreatePrimitives>().FinishFunctionLine(transform.gameObject);
            transform.GetChild(0).position = new Vector3(maxx + 15, maxy + 15, -5);
        }

        if (cascaded_lasso /*&& transform.childCount > 1*/)
        {
            Debug.Log("cascaded_try");
            GameObject graph = transform.GetChild(1).gameObject;
            Transform extra_objects = graph.transform.GetChild(5);
            
            int idx = 0;
            foreach (Graph returned_graph in returned_graphs.graphs)
            {
                GameObject functionline = extra_objects.GetChild(idx).gameObject;
                updatechildLassoPoints(returned_graph.nodes, functionline, idx);
                idx++;
            }
        }
    }
        
    public void updatechildLassoPoints(List<int> nodes, GameObject gameObject, int idx)
    {
        List<Vector3> hull_pts = new List<Vector3>();
        int center_count = 0;
        joint_centroid = Vector3.zero;              
                          
        // we need only certain nodes inside the hull, hence tracking it down   
        foreach (int node in nodes)
        {                
            if (nodeMaps.ContainsKey(node.ToString()))
            {
                Transform child = nodeMaps[node.ToString()];
                                        
                if (child.GetComponent<iconicElementScript>().video_icon)
                {
                    hull_pts.AddRange(child.GetComponent<iconicElementScript>().points);
                    continue;
                }

                List<Vector3> returned_pts = child.GetComponent<iconicElementScript>().hullPoints(20f);
                hull_pts.AddRange(returned_pts);
                center_count++;
                joint_centroid += child.GetComponent<iconicElementScript>().edge_position;
                        
            }
        }

        joint_centroid = joint_centroid / center_count;

        var hullAPI = new HullAPI();
        var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = hull_pts.ToArray(), Concavity = 1500 });

        Vector3[] vertices = hull.vertices;
        //Array.Sort(vertices);
        Debug.Log("hulled: ");

        //gameObject.transform.position = vertices[0];
        gameObject.transform.GetComponent<LineRenderer>().enabled = true;

        gameObject.GetComponent<FunctionElementScript>().points = vertices.ToList();

        if (gameObject.transform.parent.parent.GetComponent<GraphElementScript>().video_graph)
        {
            paintable_object.GetComponent<CreatePrimitives>().FinishVideoFunctionLine(gameObject, true, idx);
        }
        else
            paintable_object.GetComponent<CreatePrimitives>().FinishFunctionLine(gameObject, true, idx);
            
    }

    public static float AngleBetweenVectors(Vector2 a, Vector2 b)
    {
        float angle = (float)Mathf.Atan2(a.y - b.y,
                                         a.x - b.x);
        return angle;
    }
       
    
    /*public void menuButtonClick(GameObject radmenu, Button btn, int buttonNumber)
    {
        if (buttonNumber == 0)
        {
            btn.onClick.AddListener(() => setPropertyAsTranslation(radmenu));
        }
        else if (buttonNumber == 1)
        {
            btn.onClick.AddListener(() => setPropertyAsRotation(radmenu));
        }
        else if (buttonNumber == 2)
        {
            btn.onClick.AddListener(() => setPropertyAsScale(radmenu));
        }
        else if (buttonNumber == 3)
        {
            // clicking attribute shouldn't do anything, except maybe submenu opens and closes
            btn.onClick.AddListener(() => interactWithAttributeMenu(radmenu));
        }
        else if (buttonNumber == 4)
        {
            btn.onClick.AddListener(() => deleteObject(radmenu));
        }
        else if (buttonNumber == 5)
        {
            btn.onClick.AddListener(() => createRotationMenu(radmenu));
        }
        else if (buttonNumber == 6)
        {
            btn.onClick.AddListener(() => copyObject(radmenu));
        }
        else if (buttonNumber == 7)
        {
            btn.onClick.AddListener(() => setAreaAsAttribute(radmenu));
        }
        else if (buttonNumber == 8)
        {
            btn.onClick.AddListener(() => setLengthAsAttribute(radmenu));
        }
    }*/

    public void deleteObject(GameObject radmenu)
    {
        // Destroy the radial menu
        Destroy(radmenu);

        // TODO: DELETE ANY EDGELINE ASSOCIATED WITH THIS PEN OBJECT

        // Destroy the object attached to this script
        Destroy(this.gameObject);
    }

  
    private void Awake()
    {
        //details_dropdown = GameObject.Find("Details_Dropdown");
        paintable_object = GameObject.Find("Paintable");
    }


    // Start is called before the first frame update
    void Start()
    {
        transform.gameObject.tag = "function";
        edge_offset = 10;

        List<GameObject> selected_vectors = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        //checkHitAndMove();
        //checkContinuousPathDefinitionInteraction();
        //checkDiscretePathDefinitionInteraction();
        //checkMove();
        //checkIfThisIsPartOfDoubleFunction();
        //onAbstractionLayerChange();

        // should be called after abstraction layer changes.
        //applyGlobalStrokeDetails();
    }

    public bool isInsidePolygon(Vector3 p)
    {
        //Debug.Log("vector_"+p.ToString()+"_own_"+this.transform.position.ToString());
        int j = points.Count - 1;
        int interval = 1; //Mathf.Max(1, (int)Math.Floor((float)(points.Count / 40)));

        bool inside = false;
        for (int i = 0; i < points.Count; i = i + interval)
        {
            //Debug.Log("I:" + i.ToString() + ", J: " + j.ToString());

            if (((points[i].y <= p.y && p.y < points[j].y) || (points[j].y <= p.y && p.y < points[i].y)) &&
               (p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                inside = !inside;

            }

            j = i;

        }
        return inside;
    }

    void OnDestroy()
    {
        Transform node_parent = transform.parent;
        if (node_parent.tag == "node_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().Graph_init();
            //node_parent.parent.GetComponent<GraphElementScript>().Graph_as_Str();
        }
    }
}
