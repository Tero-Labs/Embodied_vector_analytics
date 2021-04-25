using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

public class VideoController : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField]
    private VideoPlayer videoplayer;
    public Slider mainSlider;

    frames frames_annotation;
    all_route All_Route;

    public float width, height;    
    public int frequency;    
    public Vector3[] vec;
    long prev_frame;

    public GameObject icon_prefab;
    public GameObject edge_prefab;
    public GameObject graph_prefab;
    public GameObject paintable;
    public GameObject graph_holder;

    public List<GameObject> all_icons;

    public float node_radius;
    public string graph_type;
    public bool auto_track;

    // Start is called before the first frame update
    void Start()
    {
        graph_holder = null;
        node_radius = 20f;
        frequency = 15;
    }

    public void loadAnnotation(string filename)
    {
        Debug.Log("filename:" + filename);
        frames_annotation = JsonUtility.FromJson<frames>(File.ReadAllText(filename));
        
        width = videoplayer.transform.localScale.x;
        height = videoplayer.transform.localScale.y;

        //temp_parent = Instantiate(graph_prefab);
    }

    public void loadRoutes(string filename)
    {
        Debug.Log("filename:" + filename);
        All_Route = JsonUtility.FromJson<all_route>(File.ReadAllText(filename));

    }

    // Update is called once per frame
    void Update()
    {        

        if (videoplayer.frameCount > 0)
        {
            mainSlider.value = (float)videoplayer.frame / (float)videoplayer.frameCount;
        }

        if (prev_frame!= videoplayer.frame && videoplayer.frame > 0)
        {
            /*if (videoplayer.frame % frequency == 0 || videoplayer.frame == 5)
            {*/
                prev_frame = videoplayer.frame;
                //Debug.Log("current_frame: " + videoplayer.frame.ToString());

                if (graph_holder != null)
                    {
                        /*Destroy(temp_parent);
                        temp_parent = null;*/                 
                    

                        if (frames_annotation.node_type != "static")
                        {
                            GameObject nodepar = graph_holder.transform.GetChild(0).gameObject;
                            Destroy(nodepar);
                            GameObject tempnodeparent = new GameObject("node_parent_1");
                            tempnodeparent.tag = "node_parent";
                            tempnodeparent.transform.parent = graph_holder.transform;
                            tempnodeparent.transform.SetSiblingIndex(0);

                            GameObject edgepar = graph_holder.transform.GetChild(1).gameObject;
                            GameObject simplicialpar = graph_holder.transform.GetChild(2).gameObject;
                            GameObject hyperpar = graph_holder.transform.GetChild(3).gameObject;

                            Destroy(edgepar);
                            Destroy(simplicialpar);
                            Destroy(hyperpar);

                            GameObject tempedgeparent = new GameObject("edge_parent_1");
                            tempedgeparent.tag = "edge_parent";
                            tempedgeparent.transform.parent = graph_holder.transform;
                            tempedgeparent.transform.SetSiblingIndex(1);

                            GameObject tempsimplicialparent = new GameObject("simplicial_parent_1");
                            tempsimplicialparent.tag = "simplicial_parent";
                            tempsimplicialparent.transform.parent = graph_holder.transform;
                            tempsimplicialparent.transform.SetSiblingIndex(2);

                            GameObject temphyperparent = new GameObject("hyper_parent_1");
                            temphyperparent.tag = "hyper_parent";
                            temphyperparent.transform.parent = graph_holder.transform;
                            temphyperparent.transform.SetSiblingIndex(3);
                        }                
                                      


                    }
                else
                {
                    graph_holder = Instantiate(graph_prefab);
                    graph_holder.transform.parent = paintable.GetComponent<Paintable>().Objects_parent.transform;
                    graph_holder.GetComponent<GraphElementScript>().video_graph = true;
                    graph_holder.GetComponent<GraphElementScript>().abstraction_layer = "graph";
                    graph_holder.GetComponent<GraphElementScript>().paintable = paintable;
                    Paintable.graph_count++;
                    graph_holder.name = "graph_" + Paintable.graph_count.ToString();
                    graph_holder.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();
                }

                graph_holder.SetActive(true);

            if (graph_holder.transform.GetChild(0).childCount == 0)
            {

                all_icons = new List<GameObject>();
                List<tracked_object> all_objects;

                if (frames_annotation.node_type != "static")
                    all_objects = frames_annotation.all_frame[(int)videoplayer.frame].objects;
                else
                    all_objects = frames_annotation.all_frame[0].objects;


                // because, we do not want to increase total icon numbers in each frame, which will be ambigious
                int num = Paintable.totalLines;
                List<Vector3> graph_points = new List<Vector3>();

                graph_holder.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();
                graph_holder.GetComponent<GraphElementScript>().graph = new Graph();
                graph_holder.GetComponent<GraphElementScript>().graph.nodes = new List<int>();

                foreach (tracked_object cur_obj in all_objects)
                {
                    Vector3 edge_pos = Vector3.zero;
                    GameObject temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

                    num++;
                    temp.tag = "iconic";
                    temp.name = "iconic_" + /*num*/cur_obj.name;
                    temp.GetComponent<iconicElementScript>().icon_number = /*num*/cur_obj.id;
                    temp.GetComponent<iconicElementScript>().video_icon = true;
                    all_icons.Add(temp);

                    graph_holder.GetComponent<GraphElementScript>().graph.nodes.Add(/*num*/cur_obj.id);
                    graph_holder.GetComponent<GraphElementScript>().nodeMaps.Add(/*num*/cur_obj.id.ToString(), temp.transform);

                    temp.GetComponent<TrailRenderer>().enabled = false;
                    temp.GetComponent<MeshRenderer>().enabled = false;

                    List<Vector3> points = new List<Vector3>();

                    foreach (bounds first_obj in cur_obj.bounds)
                    {
                        float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, frames_annotation.width, first_obj.x));
                        float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, frames_annotation.height, first_obj.y));

                        Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -40f);
                        edge_pos += pos_vec;

                        points.Add(pos_vec);
                    }


                    // connect the end and statr position as well
                    /*LineRenderer l = temp.GetComponent<LineRenderer>();
                    l.material.color = Color.black;
                    l.startWidth = 2f;
                    l.endWidth = 2f;
                    l.loop = true;
                    // if we don't manually change the position count, it only takes the first two positions
                    l.positionCount = points.Count;
                    l.SetPositions(points.ToArray());*/

                    edge_pos = edge_pos / points.Count;
                    temp.GetComponent<iconicElementScript>().edge_position = edge_pos;
                    temp.GetComponent<iconicElementScript>().bounds_center = edge_pos;

                    // to save compute resource, we store all points in graph instead
                    temp.GetComponent<iconicElementScript>().points = points;
                    graph_points.AddRange(points);

                    temp.GetComponent<iconicElementScript>().centroid = edge_pos;

                    // again we optimize by approxmating the radius calculation
                    if (points.Count > 1)
                        temp.GetComponent<iconicElementScript>().radius = Vector3.Distance(points[0], points[1]) / 2;
                    else
                        temp.GetComponent<iconicElementScript>().radius = 20f;

                    float rad = temp.GetComponent<iconicElementScript>().radius;
                    Vector3 size = new Vector3(rad, rad, rad);

                    BoxCollider box_cl = temp.AddComponent<BoxCollider>();
                    box_cl.center = edge_pos;//Vector3.zero;
                    box_cl.size = size;
                }

                Debug.Log("all_icons: " + all_icons.Count.ToString());

                graph_holder.GetComponent<GraphElementScript>().points = graph_points;

                graph_holder.GetComponent<GraphElementScript>().graph.edges = new List<Edge>();

            }

            // create graph based on node radius
            if (graph_type == "NodeRadius")
            {
                for (int i = 0; i < all_icons.Count; i++)
                {
                    for (int j = (i + 1); j < all_icons.Count; j++)
                    {
                        if (Vector3.Distance(all_icons[i].GetComponent<iconicElementScript>().edge_position,
                            all_icons[j].GetComponent<iconicElementScript>().edge_position) < node_radius)
                        {
                            GameObject temp = Instantiate(edge_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(1));

                            temp.tag = "edge";
                            temp.GetComponent<VectorElementScript>().edge_start = all_icons[i];
                            temp.GetComponent<VectorElementScript>().edge_end = all_icons[j];
                            temp.GetComponent<VectorElementScript>().video = true;


                            //temp.GetComponent<EdgeElementScript>().addDot();
                            //temp.GetComponent<EdgeElementScript>().updateEndPoint();
                            temp.GetComponent<VectorElementScript>().addEndPoint(true);

                            Edge edge = new Edge();
                            edge.edge_start = all_icons[i].GetComponent<iconicElementScript>().icon_number;
                            edge.edge_end = all_icons[j].GetComponent<iconicElementScript>().icon_number;
                            edge.weight = 1;

                            graph_holder.GetComponent<GraphElementScript>().graph.edges.Add(edge);
                        }
                    }
                }
            }
            // create site specific edges  
            else
            {
                List<Edge> all_edges = All_Route.edge_list[(int)videoplayer.frame].edges;

                foreach (Edge cur_edge in all_edges)
                {
                    GameObject temp = Instantiate(edge_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(1));

                    temp.tag = "edge";
                    temp.GetComponent<VectorElementScript>().edge_start = 
                        graph_holder.GetComponent<GraphElementScript>().nodeMaps[cur_edge.edge_start.ToString()].gameObject;

                    temp.GetComponent<VectorElementScript>().edge_end =
                        graph_holder.GetComponent<GraphElementScript>().nodeMaps[cur_edge.edge_end.ToString()].gameObject;

                    temp.GetComponent<VectorElementScript>().video = true;


                    //temp.GetComponent<EdgeElementScript>().addDot();
                    //temp.GetComponent<EdgeElementScript>().updateEndPoint();
                    temp.GetComponent<VectorElementScript>().addEndPoint(true);
                                        
                    graph_holder.GetComponent<GraphElementScript>().graph.edges.Add(cur_edge);
                }                
            }

            if (videoplayer.frame % VideoPlayerChildrenAccess.time_slice == 0)
                graph_holder.GetComponent<GraphElementScript>().RequestRecalculationonValueChange(videoplayer.transform.gameObject);

            //}
            /*else if (graph_holder != null)
            {
                prev_frame = videoplayer.frame;              

                List<tracked_object> all_objects = frames_annotation.all_frame[(int)videoplayer.frame].objects;
                List<Vector3> graph_points = new List<Vector3>();
                               
                foreach (tracked_object cur_obj in all_objects)
                {
                    GameObject temp;

                    if (graph_holder.GetComponent<GraphElementScript>().nodeMaps.ContainsKey(cur_obj.id.ToString()))
                    {
                        temp = graph_holder.GetComponent<GraphElementScript>().nodeMaps[cur_obj.id.ToString()].gameObject;
                    }
                    else
                    {
                        temp = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity, graph_holder.transform.GetChild(0));

                        temp.tag = "iconic";
                        temp.name = "iconic_" + cur_obj.id.ToString();
                        temp.GetComponent<iconicElementScript>().icon_number = cur_obj.id;
                        temp.GetComponent<iconicElementScript>().video_icon = true;
                        all_icons.Add(temp);

                        graph_holder.GetComponent<GraphElementScript>().graph.nodes.Add(cur_obj.id);
                        graph_holder.GetComponent<GraphElementScript>().nodeMaps.Add(cur_obj.id.ToString(), temp.transform);

                        temp.GetComponent<TrailRenderer>().enabled = false;
                        temp.GetComponent<MeshRenderer>().enabled = false;

                    }

                    Vector3 edge_pos = Vector3.zero;
                    
                    List<Vector3> points = new List<Vector3>();

                    foreach (bounds first_obj in cur_obj.bounds)
                    {
                        float lerped_x = Mathf.Lerp(videoplayer.transform.position.x - (width / 2), videoplayer.transform.position.x + (width / 2), Mathf.InverseLerp(1, 853, first_obj.x));
                        float lerped_y = Mathf.Lerp(videoplayer.transform.position.y + (height / 2), videoplayer.transform.position.y - (height / 2), Mathf.InverseLerp(1, 480, first_obj.y));

                        Vector3 pos_vec = new Vector3(lerped_x, lerped_y, -5f);
                        edge_pos += pos_vec;

                        points.Add(pos_vec);
                    }

                    edge_pos = edge_pos / points.Count;
                    temp.GetComponent<iconicElementScript>().edge_position = edge_pos;
                    temp.GetComponent<iconicElementScript>().bounds_center = edge_pos;

                    // to save compute resource, we store all points in graph instead
                    temp.GetComponent<iconicElementScript>().points = points;
                    graph_points.AddRange(points);

                    temp.GetComponent<iconicElementScript>().centroid = edge_pos;

                    // again we optimize by approxmating the radius calculation
                    temp.GetComponent<iconicElementScript>().radius = Vector3.Distance(points[0], points[1]) / 2;
                    float rad = temp.GetComponent<iconicElementScript>().radius;
                    Vector3 size = new Vector3(rad, rad, rad);

                    BoxCollider box_cl = temp.AddComponent<BoxCollider>();
                    box_cl.center = edge_pos;//Vector3.zero;
                    box_cl.size = size;
                }

                Transform edge_parent = graph_holder.transform.GetChild(1);

                for (int i = 0; i < edge_parent.childCount; i++)
                {
                    Transform child = edge_parent.GetChild(i);
                    if (graph_holder.GetComponent<GraphElementScript>().splined_edge_flag)
                        child.GetComponent<EdgeElementScript>().updateSplineEndPoint();
                    else
                        child.GetComponent<EdgeElementScript>().updateEndPoint();
                }

            }
            */
        }
    }

    
    public void OnSliderValueChanged(PointerEventData eventData)
    {
        Vector2 localpoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle
            (mainSlider.GetComponent<RectTransform>(), eventData.position, null, out localpoint))
        {
            float value = Mathf.InverseLerp(mainSlider.GetComponent<RectTransform>().rect.xMin, mainSlider.GetComponent<RectTransform>().rect.xMax, localpoint.x) ;
            var frame = videoplayer.frameCount * value;
            videoplayer.frame = (long)frame;
            mainSlider.value = (float)value;
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSliderValueChanged(eventData);
        //throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnSliderValueChanged(eventData);
        //throw new System.NotImplementedException();
    }

    //  DumpRenderTexture(videoplayer.targetTexture, "Assets/Resources/" + "dump.png");
    //  https://gist.github.com/AlexanderDzhoganov/d795b897005389071e2a
    public Texture2D DumpRenderTexture(/*RenderTexture rt, string pngOutPath*/)
    {
        RenderTexture rt = videoplayer.targetTexture;

        var oldRT = RenderTexture.active;

        var tex = new Texture2D(rt.width, rt.height);
        //var tex_small = new Texture2D((int)width, (int)height);

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        /*tex_small.SetPixels(0, 0, (int)width, (int)height, tex.GetPixels());
        tex_small.Apply();*/

        //File.WriteAllBytes(pngOutPath, tex.EncodeToPNG());
        RenderTexture.active = oldRT;

        //Destroy(tex);
        return tex;
    }
}
