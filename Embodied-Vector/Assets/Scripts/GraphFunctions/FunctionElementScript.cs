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
    
    // visual representation
    // public List<Vector2> points;

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

    // get-able attributes
    public struct Attributes
    {
        public float Length;
        public float Area;
    };

    public Attributes attribute;
    public float current_attribute;

    // movement related variables
    public Vector3 previous_position;	// used for checking if the pen object is under a set or function
    public Vector3 joint_centroid;

    // path record
    public List<Vector3> recorded_path = new List<Vector3>();
    public bool record_path_enable = false;
    public Vector3 position_before_record;
    public Vector3 edge_position;

    public Dictionary<string, Transform> nodeMaps;

    // path translate
    /*public Vector3 position_before_translation;
    public List<Vector3> translation_path = new List<Vector3>();
    public SortedList<float, int> translation_sections_indices = new SortedList<float, int>();
    public int path_index = 0;
    public float eps_path_radius = 10f;
    public bool has_continuous_path = true;
    public float heightScale = 20, xScale = 100;

    // causal transformation parameters
    public float max_possible_scale = 2.0f;
    public float max_possible_rotation = 360.0f;
    public float rotation_angle, scale_value;
    public float min_rotation_val = 0f;
    public float max_rotation_val = 5f;
    public float min_scale_val = 1f;
    public float max_scale_val = 5f;
    public bool apply_rotation = false;
    public bool apply_scale = false;*/

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
    public bool graph_generation_done;

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
        graph_generation_done = true;

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
    
    /*public void updateLassoPoints(GameObject graph)
    {
        if (fused_function) return;

        if (!transform.GetChild(0).GetComponent<FunctionMenuScript>().instant_eval)
        {
            List<Vector3> hull_pts = new List<Vector3>();
            int center_count = 0;
            joint_centroid = Vector3.zero;

            if (graph.tag == "graph")
            {
                bool video_flag = false;
                Transform[] allChildrennode = graph.transform.GetChild(0).GetComponentsInChildren<Transform>();

                foreach (Transform child in allChildrennode)
                {
                    if (child.tag == "iconic")
                    {
                        // to optimize we take special measure for videos
                        if (child.GetComponent<iconicElementScript>().video_icon)
                        {
                            hull_pts.AddRange(graph.GetComponent<GraphElementScript>().points);
                            video_flag = true;
                            break;
                        }

                        List<Vector3> returned_pts = child.GetComponent<iconicElementScript>().hullPoints();
                        hull_pts.AddRange(returned_pts);
                        center_count++;
                        joint_centroid += child.GetComponent<iconicElementScript>().edge_position;
                    }
                }

                if (!video_flag)
                {
                    // we want the edges to stay within the lasso as well 
                    Transform[] allChildrenedge = graph.transform.GetChild(1).GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildrenedge)
                    {
                        if (child.tag == "edge")
                        {
                            Vector3[] returned_pts_arr = new Vector3[child.GetComponent<LineRenderer>().positionCount];
                            int temp = child.GetComponent<LineRenderer>().GetPositions(returned_pts_arr);

                            for (int i = 0; i < returned_pts_arr.Length; i++)
                            {
                                hull_pts.Add(returned_pts_arr[i] - new Vector3(0, edge_offset, 0));
                                hull_pts.Add(returned_pts_arr[i] + new Vector3(0, edge_offset, 0));
                            }

                            
                        }

                    }

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
        
    }*/

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

    /*public void checkContinuousPathDefinitionInteraction()
    {

        if (Paintable.pathdefinition_button.GetComponent<AllButtonsBehavior>().selected &&
            Paintable.pathdefinition_button.GetComponent<AllButtonsBehavior>().isContinuousPathDefinition)
        {
            //currentPen = Pen.current;

            //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {

                    current_property = Properties.translation;
                    record_path_enable = true;
                    position_before_record = transform.position;
                    // empty the list
                    recorded_path.Clear();

                    draggable_now = true;

                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -40f;

                    touchDelta = transform.position - vec;

                    GetComponent<TrailRenderer>().Clear();

                }
            }

            //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            if (PenTouchInfo.PressedNow)//currentPen.tip.isPressed)
            {
                //Debug.Log("penline touched");

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    //Debug.Log(transform.name);

                    draggable_now = true;

                    Vector3 vec = Hit.point; //+ new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                             //Debug.Log(vec);

                    // enforce the same z coordinate as the rest of the points in the parent set object
                    vec.z = -40f;

                    Vector3 diff = vec - transform.position + touchDelta;
                    diff.z = 0;

                    // update the drawn object position.
                    transform.position += diff;

                    // update the previous_position variable: since user moved it by hand, not driven by a set or function
                    previous_position = transform.position;

                    // record path UI
                    if (record_path_enable)
                    {
                        recordPath();

                        // enable trail renderer to show the drawn path
                        GetComponent<TrailRenderer>().enabled = true;
                        GetComponent<TrailRenderer>().material.color = new Color(110, 0, 0);
                    }
                }
            }

            //else if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled))
            else if (PenTouchInfo.ReleasedThisFrame)//currentPen.tip.wasReleasedThisFrame)
            {
                // important: touch can start and end when interacting with other UI elements like a set's slider.
                // so, double check that this touch was still on top of the penline object, not, say, on a slider.
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == this.gameObject)
                {
                    // if record path UI was on, then take care of the recorded path and return pen object to original position.
                    if (record_path_enable)
                    {
                        recordPath();

                        transform.position = position_before_record;

                        draggable_now = false;

                        // Add a line renderer component and re-distribute the points across the line
                        this.gameObject.AddComponent<LineRenderer>();

                        // UNIFORMLY DISTRIBUTE THE POINTS ALONG THE PATH
                        GameObject spline = new GameObject("spline");
                        spline.AddComponent<BezierSpline>();
                        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
                        bs.Initialize(recorded_path.Count);
                        for (int ss = 0; ss < recorded_path.Count; ss++)
                        {
                            bs[ss].position = recorded_path[ss];
                        }

                        // Now sample 50 points across the spline with a [0, 1] parameter sweep
                        recorded_path = new List<Vector3>(50);
                        for (int i = 0; i < 50; i++)
                        {
                            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 49, i)));
                        }

                        Destroy(spline);


                        record_path_enable = false;

                        GetComponent<TrailRenderer>().enabled = false;

                        // convert global recorded coordinates to local, wrt pen object
                        for (int k = 0; k < recorded_path.Count; k++)
                        {
                            recorded_path[k] = transform.InverseTransformPoint(recorded_path[k]);
                        }

                        // calculate the actual (global) translation path
                        calculateTranslationPath();

                        // indicate what kind of path this penline has
                        has_continuous_path = true;

                        // set the upper_val manually, doesn't seem to initialize with default value;

                        // (re)create param text and text box
                        createParamGUIText();

                        return;
                    }
                }
            }
        }
    }*/

    /*public void checkDiscretePathDefinitionInteraction()
    {
        if (Paintable_Script.pathdefinition_button.GetComponent<AllButtonsBehavior>().selected &&
            Paintable_Script.pathdefinition_button.GetComponent<AllButtonsBehavior>().isContinuousPathDefinition == false)
        {

            // if a key press is detected, leave the record path mode. 
            // Do it for all other pen objects too, as any pen object could be suspending in that state while this pen object is clicked
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.Return) || Input.anyKeyDown)
            {
                record_path_enable = false;

                GameObject[] allpens = GameObject.FindGameObjectsWithTag("penline");
                for (int i = 0; i < allpens.Length; i++)
                {
                    allpens[i].GetComponent<penLine_script>().record_path_enable = false;
                }
            }

            //currentPen = Pen.current;

            // let the user create some keypoints
            if (PenTouchInfo.PressedThisFrame)
            {
                //Vector2 penpos = Camera.main.ScreenToWorldPoint(currentPen.position.ReadValue());

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                // check for a hit on the anchor object
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == transform.gameObject)
                {
                    current_property = Properties.translation;
                    record_path_enable = true;
                    position_before_record = transform.position;
                    // empty the list
                    recorded_path.Clear();

                    // indicate what kind of path this penline has
                    has_continuous_path = false;

                    // 
                }

            }
            if (PenTouchInfo.ReleasedThisFrame && record_path_enable)
            {
                Vector3 penpos = Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);

                penpos.z = -40.5f;

                recorded_path.Add(penpos);

                // if it's the first point, skip (it's meant for param_text_start at 0), then the next point to param_text_end, and then to new ones.
                if (recorded_path.Count == 2)
                {
                    transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().upper_val = 5.0f;

                    transform.GetChild(4).GetChild(0).GetComponent<TextMeshPro>().text = 5.0f.ToString();

                    max_parameter_value = 5;

                    // update translation path
                    transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(recorded_path[0]);

                    float length = (recorded_path[1] - recorded_path[0]).magnitude;
                    Vector3 unit = (recorded_path[1] - recorded_path[0]) / length;


                    // sample 50 points along the unit vector direction
                    for (int k = 1; k < 50; k++)
                    {
                        transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(
                            recorded_path[0] + unit * (length / 50) * k);
                    }

                    // update full translation path
                    updateFullTranslationPathFromAllChildren();

                    // add a line renderer in case it doesn't exist
                    gameObject.AddComponent<LineRenderer>();

                    setupLineRendererFromTranslationPath();

                    // show the line renderer
                    showTranslationPath();

                    // Update properties of text display
                    createParamGUIText();

                }
                else if (recorded_path.Count > 2)
                {
                    // create a parameter text box at this location, assign appropriate value according to path index
                    GameObject newparamtext = Instantiate(transform.GetChild(4).GetChild(0).gameObject, transform.GetChild(4));

                    // clear the translation_path in the script, every property gets copied it seems.
                    newparamtext.GetComponent<param_text_Script>().translation_path.Clear();

                    newparamtext.name = transform.GetChild(4).childCount.ToString();
                    newparamtext.transform.position = recorded_path[recorded_path.Count - 1];
                    newparamtext.GetComponent<BoxCollider2D>().size = newparamtext.GetComponent<RectTransform>().sizeDelta;

                    int newparamindex = transform.GetChild(4).childCount - 1;

                    transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val =
                         transform.GetChild(4).GetChild(newparamindex - 1).GetComponent<param_text_Script>().upper_val + 5.0f;

                    max_parameter_value = (int)transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val;

                    transform.GetChild(4).GetChild(newparamindex).GetComponent<TextMeshPro>().text =
                        transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().upper_val.ToString();

                    // update translation path
                    transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().translation_path.Add(recorded_path[recorded_path.Count - 1 - 1]);

                    float length = (recorded_path[recorded_path.Count - 1] - recorded_path[recorded_path.Count - 2]).magnitude;
                    Vector3 unit = (recorded_path[recorded_path.Count - 1] - recorded_path[recorded_path.Count - 2]) / length;


                    // sample 50 points along the unit vector direction
                    for (int k = 1; k < 50; k++)
                    {
                        transform.GetChild(4).GetChild(newparamindex).GetComponent<param_text_Script>().translation_path.Add(
                            recorded_path[recorded_path.Count - 2] + unit * (length / 50) * k);
                    }

                    // update full translation path
                    updateFullTranslationPathFromAllChildren();

                    setupLineRendererFromTranslationPath();

                    // show the line renderer
                    showTranslationPath();

                    // Update properties of text display
                    createParamGUIText();
                }

                /*
				// is this position close to one of the points of translation_path?
				for (int i = 0; i < translation_path.Count; i++)
				{
					if (Vector2.Distance(penpos, (Vector2)translation_path[i]) < eps_path_radius * 1.5f)
					{
						// Snap? What's the closest whole value index? Snap to there and create a text box
						//float param = (float)(i * 1.0f / translation_path.Count);

						// create a parameter text box at this location, assign appropriate value according to path index
						GameObject newparamtext = Instantiate(transform.GetChild(1).gameObject, transform.GetChild(5));
						newparamtext.name = transform.GetChild(5).childCount.ToString();
						newparamtext.transform.position = translation_path[i];
						newparamtext.GetComponent<BoxCollider2D>().size = newparamtext.GetComponent<RectTransform>().sizeDelta;

						float param = (float)(i * 1.0f / translation_path.Count);
						newparamtext.GetComponent<TextMeshPro>().text = (param * max_parameter_value).ToString("F2");

						//translation_sections_indices.Add((float)(decimal.Round((decimal)param * max_parameter_value), i));

						break;
					}
				}
				*/
    //}

    /*
    // if this is already a warped path, then can't add new keypoints. start fresh. Delete all current keypoints, and add one point inbetween the two ends
    else if (currentPen.tip.wasReleasedThisFrame && translation_path.Count > 1 && translation_sections_indices.Count > 2)
    {
        Vector2 penpos = Camera.main.ScreenToWorldPoint(currentPen.position.ReadValue());

        // is this position close to one of the points of translation_path?
        for (int i = 0; i < translation_path.Count; i++)
        {
            if (Vector2.Distance(penpos, (Vector2)translation_path[i]) < eps_path_radius * 1.5f)
            {
                // Delete all current keypoints, and add one point inbetween the two ends

                break;
            }
        }
    }
    */
    /*}
}*/

    /*public void recalibrateWarpedTranslationPath()
    {
        // UNIFORMLY DISTRIBUTE THE POINTS ALONG THE PATH
        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
        bs.Initialize(recorded_path.Count);
        bs[0].position = translation_path[0];
        for (int ss = 1; ss < recorded_path.Count; ss++)
        {
            bs[ss].position = translation_path[0] + recorded_path[ss];
        }

        // Now sample 50 points, but decide how many to sample in each section
        // ...



        // Now sample 50 points across the spline with a [0, 1] parameter sweep
        recorded_path = new List<Vector3>(50);
        for (int i = 0; i < 50; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 49, i)));
        }

        Destroy(spline);


        record_path_enable = false;

        GetComponent<TrailRenderer>().enabled = false;

        // convert global recorded coordinates to local, wrt pen object
        for (int k = 0; k < recorded_path.Count; k++)
        {
            recorded_path[k] = transform.InverseTransformPoint(recorded_path[k]);
        }
    }
    */

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

   /* public void updateFeatureAction(float val)
    {
        // if the object is being dragged by pen/touch, don't update at that time
        if (draggable_now) return;

        // apply rotation, if there's a positive rotation value
        if (apply_rotation)
        {
            float rot_param = val / max_rotation_val;
            Quaternion rot_target = Quaternion.Euler(0, 0, -max_possible_rotation * rot_param);
            // Dampen towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rot_target, Time.deltaTime * 2.0f);
        }

        // apply scale, if any positive scale value
        if (apply_scale)
        {
            float scale_param = val / max_scale_val;
            Vector3 scale_target = new Vector3(scale_param * max_possible_scale, scale_param * max_possible_scale, 1);
            // Dampen towards the target scale
            transform.localScale = Vector3.Lerp(transform.localScale, scale_target, Time.deltaTime * 2.0f);
        }

        if (Paintable.allowOpacity)
        {
            float opacity_param = val / 110;
            Color col = transform.GetComponent<MeshRenderer>().materials[0].color;
            transform.GetComponent<MeshRenderer>().materials[0].color = new Color(col.r, col.g, col.b, opacity_param);
        }

        // =============
        // Take care of translation parameter

        
        if (translation_path.Count > 0)
        {
            // determine which section val belongs to
            int section = -1;
            float remaining_section_length = 0;
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                if (val <= transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val)
                {
                    section = i;

                    remaining_section_length = transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val - val;

                    break;
                }
                else
                {
                    if ((i == transform.GetChild(4).childCount - 1) &&
                        val >= transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val)
                    // are we at the last section and value is over the section? do things manually for this edge case. (for continuous, this'll always be the case)
                    {
                        section = i;
                        remaining_section_length = 0f;
                        break;
                    }
                }
            }

            // if not found in range, return. should not happen.
            if (section == -1) { Debug.Log("section = - 1"); return; }

            path_index = section * 49 + (int)(49 * (1 - (remaining_section_length * 1f / transform.GetChild(4).GetChild(section).GetComponent<param_text_Script>().section_length)));

            // param is in [0, 1]. Not needed for translation.
            float param = val / max_parameter_value;

            //Debug.Log(param);
            //path_index = (int)((float)translation_path.Count * param);

            // clamp path_index to prevent access errors.
            if (path_index > translation_path.Count - 1) path_index = translation_path.Count - 1;
            if (path_index < 0) path_index = 0;

            Vector3 target;
            if (path_index == 0 && translation_path.Count == 0) target = transform.position;    // prevent accessing translation_path as it has no elements in this case (no path defined).
            else target = translation_path[path_index];

            // experimental: add perlin noise. Needs more exploration.
            float height = heightScale * Mathf.PerlinNoise(Time.time * (transform.position.x - translation_path[0].x), 0.0f) - heightScale / 2; //heightScale * Mathf.PerlinNoise(Time.time * xScale, 0.0f);
            target.y += height;
            //target = target + target * Mathf.PerlinNoise(target.x, target.y) * 0.5f;

            //Debug.Log(this.name + " " + target.ToString());

            // Dampen towards the target translation
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 2.0f);
            //transform.position = target;

            //(new Vector3(0, 0, 1), param);

            // update the max_parameter_value of this script
            //max_parameter_value = (int)capacity;

            // update param text label and fix the begin and end position, otherwise they move with transform
            transform.GetChild(2).GetComponent<TextMeshPro>().text = val.ToString(); // (param * max_parameter_value).ToString();
                                                                                     // float text position
            transform.GetChild(2).transform.position = transform.GetComponent<BoxCollider>().bounds.max;
            //transform.GetChild(1).GetComponent<TextMeshPro>().text = min_parameter_value.ToString();
            //transform.GetChild(4).GetChild(transform.GetChild(4).childCount - 1).GetComponent<TextMeshPro>().text = max_parameter_value.ToString();

            transform.GetChild(1).transform.position = translation_path[0];

            // update all keypoint labels, fix their position, otherwise they move with transform.
            for (int s = 0; s < transform.GetChild(4).childCount; s++)
            {
                transform.GetChild(4).GetChild(s).transform.position =
                    transform.GetChild(4).GetChild(s).GetComponent<param_text_Script>().translation_path[49];   // place text at the end of the path, as these are upper vals.
            }
                        
        }
        
    }

    public void recordPath()
    {
        if (recorded_path.Count > 1)
        {
            // if the dragged distance is not too low, or to prevent adding points when object is stationary
            if (Vector3.Distance(recorded_path[recorded_path.Count - 1], transform.position) > 0.1)
            {
                recorded_path.Add(transform.position);
            }
        }
        else
        {
            recorded_path.Add(transform.position);
        }
    }

    public void calculateTranslationPath()
    {
        // if it's a continuous path
        
        if (transform.GetChild(4).childCount == 1 && has_continuous_path)
        {
            // clear the current translation path
            transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Clear();
            // create a new list of positions, based on current transform.position and the local coord. recorded path
            transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(transform.position);
            for (int i = 0; i < recorded_path.Count; i++)
            {
                transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path.Add(transform.position + recorded_path[i]);
            }

            // UPDATE THE FULL TRANSLATION PATH
            translation_path.Clear();
            translation_path.AddRange(transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path);
        }
        

        setupLineRendererFromTranslationPath();

        // show the translation path
        //showTranslationPath();
    }

    public void calculateTranslationPathIfAlreadyDefined()
    {
        
        if (translation_path.Count > 0 && path_index > -1)
        {
            // translate the translation path, according to the current path_index (where the object is along the path)
            //Vector3 delta_path = transform.position - transform.GetChild(4).GetChild(0).GetComponent<param_text_Script>().translation_path[path_index];
            Vector3 delta_path = transform.position - translation_path[path_index]; // use global translation path. assume it's calculated already.

            // update all section's translation paths in case it is a discrete keypoint path, or the loop stops after first iteration if it's a continuous path.
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                for (int j = 0; j < transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().translation_path.Count; j++)
                {
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().translation_path[j] += delta_path;
                }
            }
        }

        // update the full translation path after updating the children paths
        // updateFullTranslationPathFromAllChildren();

        // set up line renderer
        setupLineRendererFromTranslationPath();

        // show the translation path
        // showTranslationPath();
    }

    public void setupLineRendererFromTranslationPath()
    {
        // set up line renderer
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.positionCount = translation_path.Count;
            lr.SetPositions(translation_path.ToArray());
            lr.material.color = new Color(255, 0, 0);
            lr.startWidth = 0.5f;
            lr.endWidth = 0.4f;
        }
    }*/

    /*public void showTranslationPath()
    {
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = true;

            // update limit texts
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                transform.GetChild(4).GetChild(i).gameObject.SetActive(true);
                transform.GetChild(4).GetChild(i).GetComponent<TextMeshPro>().text =
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val.ToString();
            }

            // update the beginning and float text
            transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(1).GetComponent<TextMeshPro>().text = min_parameter_value.ToString();
            transform.GetChild(2).gameObject.SetActive(true); // this will get updated in updateFeatureAction() if there's an edge
        }
    }
    */

    /*public void hideTranslationPath()
    {
        LineRenderer lr = transform.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.enabled = false;

            // update limit texts
            for (int i = 0; i < transform.GetChild(4).childCount; i++)
            {
                transform.GetChild(4).GetChild(i).gameObject.SetActive(false);
                transform.GetChild(4).GetChild(i).GetComponent<TextMeshPro>().text =
                    transform.GetChild(4).GetChild(i).GetComponent<param_text_Script>().upper_val.ToString();
            }

            // limit texts: min and max
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
        }
    }
    */

    /*public void checkMove()
    {
        // NOTE: WHAT THRESHOLD DISTANCE WOULD BE SUITABLE IF THE USER WANTS FINE-GRAINED PARENT CHANGING BEHAVIOR?
        // NOTE: CHECK IF PAINTABLE OBJECT IS ASSIGNED YET, OTHERWISE UNITY THROWS AN ERROR (WHEN TEMPLINE SETUP IS NOT COMPLETE)

        if (Vector3.Distance(transform.position, previous_position) > 10f && paintable_object != null)
        {
            //Debug.Log(">5");
            previous_position = transform.position;

            //draggable_now = true;

            //checkAndUpdateMembership();

            if (draggable_now)
            {
                calculateTranslationPathIfAlreadyDefined();
            }
        }
        /*else
        {
            //draggable_now = false;
        }*/
    
    /*public void checkAndUpdateMembership()
    {
        // check for type of parent membership -- the pen line is moving.

        // if this penline is already part of a set or function, and it's in abs. layer = 3, then don't check for further membership,
        // the pen line should move with the set or function. period. 
        if (transform.parent.tag == "set")
        {
            if ((transform.parent.GetComponent<setLine_script>().abstraction_layer == 3 ||
                transform.parent.GetComponent<setLine_script>().abstraction_layer == 0))
            {
                return;
            }
        }
        if (transform.parent.tag == "function")
        {
            if ((transform.parent.GetComponent<functionLine_script>().abstraction_layer == 3 ||
                transform.parent.GetComponent<functionLine_script>().abstraction_layer == 0))
            {
                return;
            }
        }

        
        // check if the drawn object came out of any parent set's area, and went into another's
        GameObject[] sets = GameObject.FindGameObjectsWithTag("set");
        bool in_canvas = false;
        for (int i = 0; i < sets.Length; i++)
        {
            // if the set is in an abstraction layer that still shows the set polygon area, then check for point in polygon
            if ((sets[i].GetComponent<setLine_script>().abstraction_layer == 1 ||
                sets[i].GetComponent<setLine_script>().abstraction_layer == 2)
                &&
                sets[i].GetComponent<setLine_script>().isInsidePolygon(
                sets[i].GetComponent<setLine_script>().transform.InverseTransformPoint(
                transform.position))
                )
            {
                // if already under a set or function, don't change membership
                if ((transform.parent.tag == "set" || transform.parent.tag == "function") && transform.parent.gameObject != sets[i]) return;

                // else, change parent to new set
                transform.parent = sets[i].transform;
                //sets[i].GetComponent<setLine_script>().updateLegibleLayer();
                in_canvas = true;

                // save log
                // save coordinates wrt parent center
                if (abstraction_layer == 1) // don't need to log when it's hidden under a parent, otherwise, many extra log lines are added for each frame
                {
                    string tkey = sets[i].name + "|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                    if (!saved_positions.ContainsKey(tkey))
                        saved_positions.Add(tkey, (Vector2)(transform.position - sets[i].transform.position));
                }

                break;
            }
        }

        // check if it went inside a function
        GameObject[] functions = GameObject.FindGameObjectsWithTag("function");
        for (int i = 0; i < functions.Length; i++)
        {
            // if the set is in an abstraction layer that still shows the set polygon area, then check for point in polygon
            if ((functions[i].GetComponent<functionLine_script>().abstraction_layer == 1 ||
                functions[i].GetComponent<functionLine_script>().abstraction_layer == 2)
                &&
                !in_canvas
                &&
                functions[i].GetComponent<functionLine_script>().CheckCategoryFilterCriteria(this.gameObject)
                &&
                functions[i].GetComponent<functionLine_script>().isInsidePolygon(
                functions[i].GetComponent<functionLine_script>().transform.InverseTransformPoint(
                transform.position))
                )
            {
                // if already under a set or function, don't change membership
                if ((transform.parent.tag == "set" || transform.parent.tag == "function") && transform.parent.gameObject != functions[i]) return;

                transform.parent = functions[i].transform;
                in_canvas = true;

                // save log
                if (abstraction_layer == 1) // don't need to log when it's hidden under a parent, otherwise, many extra log lines are added for each frame
                {
                    string tkey = functions[i].name + "|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                    if (!saved_positions.ContainsKey(tkey))
                        saved_positions.Add(tkey, (Vector2)(transform.position - functions[i].transform.position));
                }

                break;
            }
        }

        // if none of the sets or functions contain it, then it should be a child of the paintable canvas
        if (!in_canvas)
        {
            transform.parent = paintable_object.transform;

            // change the abstraction layer in case they were part of a hidden layer before
            parent_asked_to_change_layer = false;

            // save log
            // FIXED: seconds may not be the lowest resolution. There could be dragging happening twice within a second and
            // an exception is then thrown.
            string tkey = "paintable|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
            if (!saved_positions.ContainsKey(tkey))
                saved_positions.Add(tkey, (Vector2)transform.position);
        }
    }*/

    /*public Mesh SpriteToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Array.ConvertAll(recognized_sprite.vertices, i => (Vector3)i);
        mesh.uv = recognized_sprite.uv;
        mesh.triangles = Array.ConvertAll(recognized_sprite.triangles, i => (int)i);

        return mesh;
    }*/

    /*public Mesh createQuad(float width, float height)
    {
        var mesh = new Mesh();

        var vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        return mesh;
    }*/

    public void calculateLengthAttributeFromPoints()
    {
        attribute.Length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            attribute.Length += Vector3.Distance(points[i - 1], points[i]);
        }
        // scale to global unit scale
        attribute.Length *= Paintable.unitScale;
    }

    // keep checking every frame if it came out from a double function lasso or if it's still a child of it.
    /*public void checkIfThisIsPartOfDoubleFunction()
    {
        if (is_this_double_function_operand)
        {
            if (transform.parent.tag == "function" && transform.parent.GetComponent<functionLine_script>().is_this_a_double_function)
            {
                // keep it true
                is_this_double_function_operand = true;
            }
            else
            {
                // turn off the operand argument_label
                is_this_double_function_operand = false;
                transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }*/

    /*public void applyGlobalStrokeDetails()
    {
        // act only if visible, and if pen line is finished drawing and there's a translation path available
        if (abstraction_layer == 1 && translation_path.Count > 1)
        {
            //if (details_dropdown.GetComponent<DropdownMultiSelect>().transform.Find("List") != null)
            //Debug.Log("not null.");

            bool state = details_dropdown.GetComponent<DropdownMultiSelect>().transform.GetChild(1).GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<Toggle>().isOn;

            if (state != global_details_on_path)
            {
                // change needed
                if (!state)
                {
                    hideTranslationPath();
                    global_details_on_path = false;
                }
                else if (state)
                {
                    showTranslationPath();
                    global_details_on_path = true;
                }
            }
        }
    }*/


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
