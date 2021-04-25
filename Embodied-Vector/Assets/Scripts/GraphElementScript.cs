using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.Video;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;


public class GraphElementScript : MonoBehaviour
{
    public Vector3 edge_position;
    public Vector3 center_position;

    public string graph_name;
    public string nodes_str;
    public string edges_str;
    public string hyper_edges_str;
    public string simplicial_str;
    public string abstraction_layer;

    public string layout_layer;

    public bool splined_edge_flag;
    public bool conversion_done;

    public GameObject EdgeElement;
    public GameObject SimplicialEdgeElement;
    public GameObject hyperEdgeElement;
    public GameObject LabelElement;
    public GameObject graph_radial_menu;
    public GameObject canvas_radial;
    public GameObject paintable;

    // to track the video parent which was the full graph 
    public GameObject parent_graph;

    public bool graph_lock;
    public bool simplicial_lock;
    public bool hyper_edges_lock;
    public bool abstract_lock;

    public bool graph_drawn;
    public bool simplicial_drawn;
    public bool hyper_edges_drawn;
    public bool abstract_drawn;

    public bool video_graph;

    public Graph graph;

    // for layout
    public GraphCordinate graphCordinate;
    public GraphCordinate manual_graphCordinate;
    float rad_x, rad_y;

    public Dictionary<string, Transform> nodeMaps;

    public List<Vector3> points = new List<Vector3>();

    // for shwing abstraction layer name
    TMP_Text tmptextlabel;

    // for graph details showing
    public GameObject graph_Details;
    public GameObject topo_label;


    //MENU
    InputField mainInputField;

    // Start is called before the first frame update
    void Start()
    {
        //abstraction_layer = "graph";
        //canvas_radial = GameObject.Find("canvas_radial");

        graph_lock = true;
        simplicial_lock = true;
        hyper_edges_lock = true;
        abstract_lock = true;

        graph_drawn = false;
        simplicial_drawn = false;
        hyper_edges_drawn = false;

        layout_layer = "manual";
              

        //splined_edge_flag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
            Paintable.click_on_inputfield = true;
    }

    void DeleteChildren(Transform trans)
    {
        for (int i = trans.childCount - 1; i >= 0; --i)
        {
            var child = trans.GetChild(i).gameObject;
            Debug.Log("Deleting " + child.name);
            Destroy(child);
        }

    }

    /*
    public void edges_as_Str()
    {
        edges_str = "";

        Transform Prev_edge_parent = transform.GetChild(1);
        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                edges_str += "{" + child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number.ToString() + "," + child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number.ToString() + "}";
            }
        }

        //Debug.Log("edges_str" + " : " + edges_str);
        edges_init();
    }

    public void simplicial_as_Str()
    {
        simplicial_str = "";

        Transform Prev_simp_parent = transform.GetChild(2);
        Transform[] allChildrensimpedge = Prev_simp_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrensimpedge)
        {
            if (child.tag == "simplicial")
            {
                if (child.GetComponent<SimplicialElementScript>() != null)
                {
                    simplicial_str += "{";
                    foreach (GameObject node in child.GetComponent<SimplicialElementScript>().thenodes)
                        simplicial_str += node.GetComponent<iconicElementScript>().icon_number.ToString() + ",";

                    simplicial_str = simplicial_str.Remove(simplicial_str.Length - 1) + "}";
                }
                else
                {
                    simplicial_str += "{" + child.GetComponent<EdgeElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number.ToString() + "," + child.GetComponent<EdgeElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number.ToString() + "}";
                }
            }
        }

        //Debug.Log("simplicial_str" + " : " + simplicial_str);
        simplicial_init();
    }

    public void hyperedges_as_Str()
    {
        hyper_edges_str = "";

        Transform Prev_hyper_parent = transform.GetChild(3);
        Transform[] allChildrenhyperedge = Prev_hyper_parent.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildrenhyperedge)
        {
            if (child.tag == "hyper")
            {
                hyper_edges_str += "{";
                foreach (GameObject node in child.GetComponent<HyperElementScript>().thenodes)
                    hyper_edges_str += node.GetComponent<iconicElementScript>().icon_number.ToString() + ",";

                hyper_edges_str = hyper_edges_str.Remove(hyper_edges_str.Length - 1) + "}";                
            }
        }

        //Debug.Log("hyper_edges_str" + " : " + hyper_edges_str);
        hyperedges_init();
    }

    // express the graph as string, so that we can pass to the python server
    public void Graph_as_Str()
    {
        nodes_str = "{";

        Transform Prev_node_parent = transform.GetChild(0);
        Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();

        nodeMaps = new Dictionary<string, Transform>();
        int icon_count = 0;
        foreach (Transform child in allChildrennode)
        {
            if (child.tag == "iconic")
            {
                icon_count++;
                nodes_str += child.GetComponent<iconicElementScript>().icon_number.ToString() + ",";
                nodeMaps.Add(child.GetComponent<iconicElementScript>().icon_number.ToString(), child);
            }            
        }

        if (icon_count == 0)
            Destroy(transform.gameObject);
                
        nodes_str = nodes_str.Remove(nodes_str.Length - 1) + "}";
        //Debug.Log("nodes_str" + " : " + nodes_str);

        edges_as_Str();
        simplicial_as_Str();
        hyperedges_as_Str();

        Graph_init();
    }
    */

    public int nodes_init()
    {
        edge_position = Vector3.zero;
        center_position = Vector3.zero;
        
        graph.nodes = new List<int>();

        Transform node_parent = transform.GetChild(0);        

        nodeMaps = new Dictionary<string, Transform>();
        int icon_count = 0;

        //Transform[] allChildrennode = node_parent.GetComponentsInChildren<Transform>();
        //foreach (Transform child in allChildrennode)

        for (int i = 0; i < node_parent.childCount; i++)
        {
            Transform child = node_parent.GetChild(i);
            if (child != null && child.tag == "iconic")
            {
                icon_count++;
                graph.nodes.Add(child.GetComponent<iconicElementScript>().icon_number);
                nodeMaps.Add(child.GetComponent<iconicElementScript>().icon_number.ToString(), child);

                if (edge_position == Vector3.zero)
                    edge_position = child.GetComponent<iconicElementScript>().edge_position;

                if (child.GetComponent<iconicElementScript>().edge_position.x > edge_position.x)
                {
                    edge_position = new Vector3(
                        child.GetComponent<iconicElementScript>().edge_position.x + child.GetComponent<iconicElementScript>().radius,
                        child.GetComponent<iconicElementScript>().edge_position.y,
                        child.GetComponent<iconicElementScript>().edge_position.z
                        );
                }

                center_position += child.GetComponent<iconicElementScript>().edge_position;
            }
        }

        center_position = center_position / icon_count;
        return icon_count;
    }

    public void edges_init()
    {
        graph.edges = new List<Edge>();

        Transform edge_parent = transform.GetChild(1);

        /*Transform[] allChildrenedge = edge_parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildrenedge)*/

        for (int i = 0; i < edge_parent.childCount; i++)
        {
            Transform child = edge_parent.GetChild(i);
            if (child != null &&
                child.tag == "edge" &&
                child.GetComponent<VectorElementScript>().edge_start != null &&
                child.GetComponent<VectorElementScript>().edge_end != null)
            {
                Edge edge = new Edge();
                edge.edge_start = child.GetComponent<VectorElementScript>().edge_start.GetComponent<iconicElementScript>().icon_number;
                edge.edge_end = child.GetComponent<VectorElementScript>().edge_end.GetComponent<iconicElementScript>().icon_number;
                edge.weight = child.GetComponent<VectorElementScript>().edge_weight;

                graph.edges.Add(edge);
            }
        }

    }
   
    // express the graph as string, so that we can pass to the python server
    public void Graph_init()
    {
        graph = new Graph();

        int icon_count = nodes_init();

        if (icon_count == 0)
            Destroy(transform.gameObject);

        edges_init();
    }

    public void GetGraphJson()
    {
        Graphs graphs = new Graphs();
        graphs.graphs = new List<Graph>();

        graphs.graphs.Add(graph);

        Debug.Log(JsonUtility.ToJson(graphs));
        File.WriteAllText("Assets/Resources/" + "data.json", JsonUtility.ToJson(graphs));
    }

    public void MenuClickSetup(GameObject Radmenu)
    {
        Radmenu.transform.GetChild(2).GetComponent<GraphSliderMenu>().parent = transform.gameObject;
        Radmenu.transform.GetChild(2).GetComponent<GraphSliderMenu>().UpdateLayer(abstraction_layer);
        /*Radmenu.transform.GetChild(0).GetComponent<RadialSliderValueListener>().parent = transform.gameObject;
        Radmenu.transform.GetChild(0).GetComponent<RadialSliderValueListener>().setup();*/

        Transform rad_menu = Radmenu.transform.GetChild(1);
        for (int i = 0; i < rad_menu.childCount; i++)
        {
            Transform child = rad_menu.GetChild(i);

            if (child.name == "delete")
            {
                Button button = child.GetComponent<Button>();
                button.onClick.AddListener(delegate { DestroyMenu(Radmenu); });                
            }
            else if (child.name == "show_node")
            {
                Toggle toggle = child.GetChild(0).GetComponent<Toggle>();
                toggle.isOn = transform.GetChild(0).gameObject.activeSelf;
                toggle.onValueChanged.AddListener(delegate { ShowNodes(toggle); });
            }
            else if (child.name == "name")
            {
                tmptextlabel = child.GetComponent<TMP_Text>();
                tmptextlabel.text = graph_name;
            }
            else if (child.name == "show_edges")
            {
                Toggle toggle = child.GetChild(0).GetComponent<Toggle>();
                toggle.isOn = (transform.GetChild(1).gameObject.activeSelf ||
                    transform.GetChild(2).gameObject.activeSelf ||
                    transform.GetChild(3).gameObject.activeSelf);
                toggle.onValueChanged.AddListener(delegate { ShowEdges(toggle); });
            }
            else if (child.name == "layer_lock")
            {
                Toggle toggle = child.GetChild(0).GetComponent<Toggle>();
                toggle.isOn = graph_lock;
                toggle.onValueChanged.AddListener(delegate { GraphLock(toggle); });
            }
            else if (child.name == "change_name")
            {
                /*InputField*/ mainInputField = child.GetComponent<InputField>();
                mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
            }
            else if (child.name == "Dropdown")
            {
                TMP_Dropdown dropdown = child.GetComponent<TMP_Dropdown>();
                dropdown.value = Paintable.layout_dict[layout_layer];
                dropdown.onValueChanged.AddListener(delegate { ChangeLayout(dropdown); });
            }

        }
    }

    void ChangeLayout(TMP_Dropdown dropdown)
    {        
        string target_layout = dropdown.captionText.text;
        ChangeLayout(target_layout);
    }

    public void ChangeLayout(string target_layout)
    {
                
        if (target_layout == "manual")
        {
            layout_layer = target_layout;

            // show the last saved arrangements
            rad_x = 200;
            rad_y = 200;

            foreach (single_node_cord cur_cord in manual_graphCordinate.node_cord)
            {
                if (nodeMaps.ContainsKey(cur_cord.node_id.ToString()))
                {
                    Transform child = nodeMaps[cur_cord.node_id.ToString()];

                    float x = 0.5f;
                    float.TryParse(cur_cord.x, out x);
                    float y = 0.5f;
                    float.TryParse(cur_cord.y, out y);

                    Vector3 position = new Vector3(center_position.x + (x * rad_x),
                        center_position.y + (y * rad_y), center_position.z);

                    Vector3 new_pos = child.InverseTransformDirection(position) -
                                child.InverseTransformDirection(child.GetComponent<iconicElementScript>().bounds_center);

                    child.position = new Vector3(new_pos.x, new_pos.y, -40f);
                    child.GetComponent<iconicElementScript>().edge_position = position;

                    //Debug.Log("now modify positions: " + new_pos.ToString());                
                }
            }

            Transform[] allChildrenedge = transform.GetChild(1).GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildrenedge)
            {
                if (child.tag == "edge")
                {
                    if (splined_edge_flag)
                        child.GetComponent<VectorElementScript>().updateSplineEndPoint();
                    else
                        child.GetComponent<VectorElementScript>().updateEndPoint();
                }

            }

           
            return;
        }

        // save the last manual arrangement before changing it 
        if (layout_layer == "manual")
        {
            rad_x = 200;
            rad_y = 200;

            manual_graphCordinate = new GraphCordinate();
            manual_graphCordinate.node_cord = new List<single_node_cord>();

            Transform node_parent = transform.GetChild(0);
            for (int i = 0; i < node_parent.childCount; i++)
            {
                Transform child = node_parent.GetChild(i);
                if (child != null && child.tag == "iconic")
                {
                    single_node_cord single_Node = new single_node_cord();
                    single_Node.node_id = child.GetComponent<iconicElementScript>().icon_number;
                    Vector3 current_position = child.GetComponent<iconicElementScript>().edge_position;

                    // reverting the formula to get normalize values within [-1,1]
                    single_Node.x = ((current_position.x - center_position.x) / (float)rad_x).ToString();
                    single_Node.y = ((current_position.y - center_position.y) / (float)rad_y).ToString();

                    manual_graphCordinate.node_cord.Add(single_Node);                    
                }
            }
        }

        //Graph_as_Str();
        Graph_init();
        GetGraphJson();

        bool flag = false;

        flag = transform.GetComponent<HelloClient>().Call_Server("layout_" + target_layout, "layout_" + target_layout);

        if (flag)
        {
            layout_layer = target_layout;
            conversion_done = false;
        }
    }

    public void showLayout(string serverUpdate, string command)
    {
        Debug.Log("triggered for " + command);
        graphCordinate = JsonUtility.FromJson<GraphCordinate>(File.ReadAllText("Assets/Resources/" + "output.json"));
        Debug.Log(JsonUtility.ToJson(graphCordinate));
        
        /*if (Vector3.Distance(center_position, edge_position) == 0)
        {
            rad_x = 200;
            rad_y = 200;
        }
        else
        {
            rad_x = Mathf.Abs(edge_position.x - center_position.x);
            rad_y = Mathf.Abs(edge_position.y - center_position.y);
        }*/

        rad_x = 200;
        rad_y = 200;

        foreach (single_node_cord cur_cord in graphCordinate.node_cord)
        {
            if (nodeMaps.ContainsKey(cur_cord.node_id.ToString()))
            {
                Transform child = nodeMaps[cur_cord.node_id.ToString()];

                float x = 0.5f;                
                float.TryParse(cur_cord.x, out x);
                float y = 0.5f;
                float.TryParse(cur_cord.y, out y);

                Vector3 position = new Vector3(center_position.x + (x * rad_x),
                    center_position.y + (y * rad_y), center_position.z);

                Vector3 new_pos = child.InverseTransformDirection(position) -
                            child.InverseTransformDirection(child.GetComponent<iconicElementScript>().bounds_center);

                child.position = new Vector3(new_pos.x, new_pos.y, -40f);
                child.GetComponent<iconicElementScript>().edge_position = position;
                
                //Debug.Log("now modify positions: " + new_pos.ToString());                
            }
        }

        Transform[] allChildrenedge = transform.GetChild(1).GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                if (splined_edge_flag)
                    child.GetComponent<VectorElementScript>().updateSplineEndPoint();
                else
                    child.GetComponent<VectorElementScript>().updateEndPoint();
            }

        }       

        
        conversion_done = true;   
        StartCoroutine(clear_files());
    }
    
    void DestroyMenu(GameObject Radmenu)
    {
        Destroy(Radmenu);
        Destroy(transform.gameObject);
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {            
            graph_name = input.text;
            tmptextlabel.text = graph_name;
            if (transform.childCount > 4)
            {
                transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = graph_name;
            }
        }
    }

    IEnumerator clear_files()
    {
        //File.Delete("Assets/Resources/" + "output.json");
        File.Delete("Assets/Resources/" + "data.json");
        yield return null;
    }
    
    public void ShowNodes(Toggle toggle)
    {
        transform.GetChild(0).gameObject.SetActive(toggle.isOn);
        if (transform.childCount > 5)
            transform.GetChild(5).gameObject.SetActive(toggle.isOn);

        if (abstraction_layer == "abstract")
        {
            transform.GetChild(4).gameObject.SetActive(!toggle.isOn);            
        }
    }

    public void ShowEdges(Toggle toggle)
    {
        if (abstraction_layer == "graph")
        {
            transform.GetChild(1).gameObject.SetActive(toggle.isOn);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).gameObject.SetActive(false);
        }
        if (abstraction_layer == "simplicial")
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(toggle.isOn);
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).gameObject.SetActive(false);
        }
        if (abstraction_layer == "hypergraph")
        {
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(toggle.isOn);
            transform.GetChild(4).gameObject.SetActive(false);
        }
        if (abstraction_layer == "abstract")
        {
            transform.GetChild(0).gameObject.SetActive(toggle.isOn);
            if (transform.childCount > 5)
                transform.GetChild(5).gameObject.SetActive(toggle.isOn);

            transform.GetChild(1).gameObject.SetActive(toggle.isOn);
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).gameObject.SetActive(!toggle.isOn);
        }

    }

    void GraphLock(Toggle toggle)
    {
        graph_lock = toggle.isOn;
        simplicial_lock = graph_lock;
        hyper_edges_lock = graph_lock;
        abstract_lock = graph_lock;
    }

    public void StartConversion(string target_layer, GameObject menu_parent = null)
    {
        
        // already at the target abstraction layer, so no change is needed
        if (abstraction_layer == target_layer)
            return;

        if (target_layer == "abstract")
        {            
            transform.GetChild(4).position = new Vector3(edge_position.x, edge_position.y, -5f);
            transform.GetChild(4).gameObject.SetActive(true);

            if (menu_parent == null)
                transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text = graph_name;
            else
            {
                transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().tmptextlabel.GetComponent<TextMeshProUGUI>().text;

                transform.GetChild(4).GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 8;
                transform.GetChild(4).localScale = new Vector3(2f, 2f, 2f);
            }


            abstraction_layer = target_layer;
            for (int i = 0; i < 4; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            if (transform.childCount > 5)
                transform.GetChild(5).gameObject.SetActive(false);

            return;
        }

        if (abstraction_layer == "abstract")
        {            
            transform.GetChild(4).gameObject.SetActive(false); 

            transform.GetChild(0).gameObject.SetActive(true);
            if (target_layer == "graph")
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
            else if(target_layer == "simplicial")
            {
                transform.GetChild(2).gameObject.SetActive(true);
            }
            else if(target_layer == "hypergraph")
            {
                transform.GetChild(3).gameObject.SetActive(true);
            }

            if (transform.childCount > 5)
                transform.GetChild(5).gameObject.SetActive(true);

            abstraction_layer = target_layer;
            return;
        }

        Debug.Log("abstraction_layer: " + abstraction_layer + ", target_layer:" + target_layer);

        //Graph_as_Str();
        Graph_init();
        GetGraphJson();

        bool flag = false;

        if (abstraction_layer == "graph")
        {
            flag = transform.GetComponent<HelloClient>().Call_Server(nodes_str + "-" + edges_str, abstraction_layer + "_to_" + target_layer);
        }
        else if (abstraction_layer == "simplicial")
        {
            flag = transform.GetComponent<HelloClient>().Call_Server(nodes_str + "-" + simplicial_str, abstraction_layer + "_to_" + target_layer);
        }
        else
        {
            flag = transform.GetComponent<HelloClient>().Call_Server(nodes_str + "-" + hyper_edges_str, abstraction_layer + "_to_" + target_layer);
        }

        // set the current layer
        if (flag)
        {
            abstraction_layer = target_layer;
            conversion_done = false;
        }            

    }

    public void showconversion(string serverUpdate, string command)
    {
        // TodO: if an edge is already present, just SetActive instead of creating new, also update edge positions when icons are moving
        // as a work around, i have deleted when i am switching to another layer, will change when it is locked
        Debug.Log("triggered for " + command + " while current layer is: " + abstraction_layer);

        // check if the current command matches with the last conversion operation
        if ((command.Split('_'))[2] == abstraction_layer)
        {
            // string parsing, edges are separated by hyphen and each node of an edge is separated by a comma 
            string[] newedges = serverUpdate.Split('-');

            /*if ((command.Split('_'))[0] == "simplicial")
            {
                //DeleteChildren(transform.GetChild(2));
                transform.GetChild(2).gameObject.SetActive(false);

            }
            else if ((command.Split('_'))[0] == "hypergraph")
            {
                //DeleteChildren(transform.GetChild(3));
                transform.GetChild(3).gameObject.SetActive(false);
            }
            else if ((command.Split('_'))[0] == "graph")
            {
                //DeleteChildren(transform.GetChild(1));
                transform.GetChild(1).gameObject.SetActive(false);
            }*/
            

            // the follwings are basically copy of graph creation in paintable script
            if (abstraction_layer == "graph")
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);

                // delete all previous children, draw again
                if (graph_lock == false)
                {
                    DeleteChildren(transform.GetChild(1));
                }
                // else keep previous children, no need to redraw
                // however, we force a mandatory initial conversion if child count is zero           
                else 
                {                    
                    if (transform.GetChild(1).childCount > 0) return;
                }

                foreach (string edge in newedges)
                {
                    string[] nodes_of_edge = edge.Split(',');

                    // simplicial may have subsets of length 1, will skip those
                    if (nodes_of_edge.Length != 2)
                        continue;                                       

                    EdgeCreation("edge", nodes_of_edge, 1);
                }
            }

            else if (abstraction_layer == "simplicial")
            {
                transform.GetChild(2).gameObject.SetActive(true);
                transform.GetChild(3).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);

                if (simplicial_lock == false)
                {
                    DeleteChildren(transform.GetChild(2));
                }
                // force initial conversion                
                else
                {
                    if (transform.GetChild(2).childCount > 0) return;
                }

                foreach (string edge in newedges)
                {
                    string[] nodes_of_edge = edge.Split(',');
                    Debug.Log("simplicial length: " + nodes_of_edge.Length.ToString());
                                        
                    // if length 2, that is same as a normal graph edge 
                    if (nodes_of_edge.Length == 2)
                    {
                        EdgeCreation("simplicial", nodes_of_edge, 2); 
                    }                    
                }
            }

            else if (abstraction_layer == "hypergraph")
            {
                transform.GetChild(3).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(false);

                if (hyper_edges_lock == false)
                {
                    DeleteChildren(transform.GetChild(3));
                }
                else
                {
                    if (transform.GetChild(3).childCount > 0) return;
                }
                                
            }

            conversion_done = true;
            Graph_init();
        }

        StartCoroutine(clear_files());
    }

    public void EdgeCreation(string tag, string[] nodes_of_edge, int idx)
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

        GameObject edgeline = Instantiate(EdgeElement, source_vec, Quaternion.identity, transform.GetChild(idx));
        edgeline.name = "edge_1";
        edgeline.tag = tag;
        edgeline.GetComponent<VectorElementScript>().paintable_object = paintable;

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
    }

   
    public void RequestRecalculationonValueChange(GameObject video_player)
    {
        if (video_graph)
        {            
            StartCoroutine(RequestFunctionCall(video_player));
        }
    }

    IEnumerator RequestFunctionCall(GameObject video_player)
    {
        GameObject[] all_functions = GameObject.FindGameObjectsWithTag("function");

        // check if the current function is part of any function, if so initiate function call again
        foreach (GameObject cur_function in all_functions)
        {
            if (cur_function == null) continue;

            if ((cur_function.transform.childCount > 2))
            {
                // check if any function argument has been assigned
                if (cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects == null) continue;

                foreach (GameObject function_argument in cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects)
                {
                    if (function_argument == null) continue;

                    // if the argument is a graph which contains this object, then call again
                    if (function_argument.tag == "graph" &&
                        transform.gameObject == function_argument)
                    {
                        video_player.transform.GetComponent<VideoPlayer>().Pause();
                        cur_function.transform.GetChild(0).gameObject.SetActive(true);
                        cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().InitiateFunctionCallHelper(video_player);
                        yield return null;
                        break;
                    }
                    else if (function_argument.tag == "function" && function_argument.transform.GetChild(1).tag == "graph" &&
                        transform.gameObject == function_argument.transform.GetChild(1).gameObject)
                    {
                        video_player.transform.GetComponent<VideoPlayer>().Pause();
                        cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().InitiateFunctionCallHelper(video_player);
                        yield return null;
                        break;
                    }
                }
            }
        }
    }

    public void checkHitAndMove(Vector3 diff)
    {
        //if (video_graph) return;
        edge_position = Vector3.zero;

        /*if (transform.GetChild(0).gameObject.activeSelf)
        {*/
            Transform[] allChildrennode = transform.GetChild(0).GetComponentsInChildren<Transform>();

            foreach (Transform child in allChildrennode)
            {
                if (child.tag == "iconic")
                {
                    child.GetComponent<iconicElementScript>().edge_position += diff;
                    //child.transform.position += diff;

                    if (edge_position == Vector3.zero)
                        edge_position = child.GetComponent<iconicElementScript>().edge_position;

                if (child.GetComponent<iconicElementScript>().edge_position.x > edge_position.x)
                {
                    edge_position = new Vector3(
                        child.GetComponent<iconicElementScript>().edge_position.x + child.GetComponent<iconicElementScript>().radius,
                        child.GetComponent<iconicElementScript>().edge_position.y,
                        child.GetComponent<iconicElementScript>().edge_position.z 
                        );
                }
            }                    
            }
        //}

        /*if (transform.GetChild(1).gameObject.activeSelf)
        {*/
            Transform[] allChildrenedge = transform.GetChild(1).GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildrenedge)
            {
                if (child.tag == "edge")
                {
                    if (child.GetComponent<VectorElementScript>().free_hand)
                    {
                        continue;
                        //child.transform.position += diff;
                    }
                    else if (splined_edge_flag)
                        child.GetComponent<VectorElementScript>().updateSplineEndPoint();
                    else
                        child.GetComponent<VectorElementScript>().updateEndPoint();
                }

            }
        //}       

       

        /*if (transform.childCount > 4)
        {
            transform.GetChild(4).transform.position += diff;
        }

        if (transform.childCount > 5)
        {
            transform.GetChild(5).transform.position += diff;
        }*/

    }
       
    public void createMenu(GameObject arg_canvas_radial = null)
    {
        if (arg_canvas_radial != null)
            canvas_radial = arg_canvas_radial;

        GameObject radmenu = Instantiate(graph_radial_menu,
                            Vector3.zero
                            /*canvas_radial.transform.TransformPoint(edge_position + new Vector3(10f, 0f, 0f))*/,
                            Quaternion.identity,
                            canvas_radial.transform);

        Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, edge_position);

        Vector2 anchored_pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_radial.transform.GetComponent<RectTransform>(), screen_temp_pos,
                                    null, out anchored_pos);

        radmenu.GetComponent<RectTransform>().anchoredPosition = anchored_pos;

        MenuClickSetup(radmenu);
    }

    public void createMenu(Vector3 pos)
    {
        GameObject radmenu = Instantiate(graph_radial_menu,
                            canvas_radial.transform.TransformPoint(pos + new Vector3(10f, 0f, 0f)),
                            Quaternion.identity,
                            canvas_radial.transform);

        MenuClickSetup(radmenu);
    }

    public void GraphDetails(bool lassocolor = false, int rank = 0)
    {
        List<Vector3> hull_pts = new List<Vector3>();

        //GameObject graph_Details = transform.Find("graph_Details").gameObject;
        //If the child was found.
        if (graph_Details != null)
        {
            Destroy(graph_Details);
        }

        graph_Details = new GameObject("graph_Details");
        graph_Details.transform.parent = transform;

        //var l = graph_Details.AddComponent<LineRenderer>();
        var mr = graph_Details.AddComponent<MeshRenderer>();
        var mf = graph_Details.AddComponent<MeshFilter>();
        //mr.material.SetColor("_Color", Color.red); 
        mr.material = paintable.GetComponent<CreatePrimitives>().FindGraphMaterial(lassocolor, rank);

        Transform node_parent = transform.GetChild(0);
        
        for (int i = 0; i < node_parent.childCount; i++)
        {
            Transform child = node_parent.GetChild(i);
            if (child.tag == "iconic")
            {
                // to optimize we take special measure for videos
                if (video_graph)
                {
                    hull_pts.AddRange(points);
                }
                else
                {
                    List<Vector3> returned_pts = child.GetComponent<iconicElementScript>().hullPoints();
                    hull_pts.AddRange(returned_pts);
                }                

                GameObject temp_label = Instantiate(topo_label);
                temp_label.transform.SetParent(graph_Details.transform);
                temp_label.GetComponent<topoLabelScript>().parent = child.gameObject;

                temp_label.transform.position = child.GetComponent<iconicElementScript>().edge_position +
                    new Vector3(child.GetComponent<iconicElementScript>().radius, child.GetComponent<iconicElementScript>().radius + 8, 0);

                var icon_name = child.GetComponent<iconicElementScript>().icon_name;
                if (string.IsNullOrWhiteSpace(icon_name))
                    temp_label.GetComponent<TextMeshProUGUI>().text = child.GetComponent<iconicElementScript>().icon_number.ToString();
                else
                    temp_label.GetComponent<TextMeshProUGUI>().text = icon_name;
            }
        }

        Transform edge_parent = transform.GetChild(1);

        for (int i = 0; i < edge_parent.childCount; i++)
        {
            Transform child = edge_parent.GetChild(i);
            if (child.tag == "edge")
            {
               
                GameObject temp_label = Instantiate(topo_label);
                temp_label.transform.SetParent(graph_Details.transform);

                Vector3 temp_vec = Vector3.Lerp(child.GetComponent<VectorElementScript>().edge_start.GetComponent<iconicElementScript>().edge_position,
                    child.GetComponent<VectorElementScript>().edge_end.GetComponent<iconicElementScript>().edge_position,
                    0.5f);

                temp_label.transform.position = temp_vec;

                temp_label.GetComponent<TextMeshProUGUI>().text = child.GetComponent<VectorElementScript>().edge_weight.ToString();
            }
        }
        

        var hullAPI = new HullAPI();
        var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = hull_pts.ToArray(), Concavity = 30000 });

        Vector3[] vertices = hull.vertices;
        points = vertices.ToList();

        updatePolygon();

        //paintable.GetComponent<CreatePrimitives>().FinishGraphLine(transform.gameObject);
        
    }

    public void updatePolygon()
    {        
        //New mesh and game object
        GameObject myObject = graph_Details;

        //Components
        var MF = myObject.GetComponent<MeshFilter>();
        //Create mesh
        var mesh = CreateMesh();
        //Assign mesh to game object
        MF.mesh = mesh;

    }

    Mesh CreateMesh()
    {
        int x; //Counter

        //Create a new mesh
        Mesh mesh = new Mesh();

        //Vertices
        Vector3[] vertex = new Vector3[points.Count];

        //[SOLVED]Mesh vertex position in world space - Unity Forum
        //https://forum.unity.com/threads/solved-mesh-vertex-position-in-world-space.30108/
        // we need local points with respect to the graph parent, because the current points are in global space
        for (x = 0; x < points.Count; x++)
        {
            //vertex[x] = transform.InverseTransformPoint(points[x]);
            vertex[x] = points[x];
        }

        //UVs
        var uvs = new Vector2[vertex.Length];

        for (x = 0; x < vertex.Length; x++)
        {
            if ((x % 2) == 0)
            {
                uvs[x] = new Vector2(0, 0);
            }
            else
            {
                uvs[x] = new Vector2(1, 1);
            }
        }

        //Triangles
        var tris = new int[3 * (vertex.Length - 2)];    //3 verts per triangle * num triangles
        int C1, C2, C3;
        C1 = 0;
        C2 = 1;
        C3 = 2;

        for (x = 0; x < tris.Length; x += 3)
        {
            tris[x] = C1;
            tris[x + 1] = C2;
            tris[x + 2] = C3;

            C2++;
            C3++;
        }

        //Assign data to mesh
        mesh.vertices = vertex;
        mesh.uv = uvs;
        mesh.triangles = tris;

        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        //Name the mesh
        mesh.name = "MyMesh";

        //Return the mesh
        return mesh;
    }


}
