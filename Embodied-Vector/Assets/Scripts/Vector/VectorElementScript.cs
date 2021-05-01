using BezierSolution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class VectorElementScript : MonoBehaviour
{
    public GameObject dot_prefab;
    // to store vector information
    public int x, y;
    public float f_x, f_y;

    // Length, area, distance units
    public static float unitScale = 0.025f;
    // ToDo: delete_later
    public int edge_weight = 1;
    public float edge_weight_multiplier = 0.5f;   

    public List<GameObject> node_obj = new List<GameObject>();
    public GameObject edge_start, edge_end;
    // visual representation
    // public List<Vector2> points;

    // object drawing properties
    public List<Vector3> points = new List<Vector3>();
    public bool draggable_now = false;

    // for directed edge
    public bool directed_edge = false;
    public Sprite directed_edge_sprite;

    // for free-hand edge
    public bool free_hand = false;

    public Material icon_elem_material;
    public GameObject paintable_object;       

    // movement related variables
    public Vector3 previous_position;	// used for checking if the pen object is under a set or function

    // path record
    public List<Vector3> recorded_path = new List<Vector3>();    
    
    // pressure based line width
    public float totalLength = 0f;
    List<float> cumulativeLength = new List<float>();
    int movingWindow = 3;
    
    // Pen Interaction
    Pen currentPen;

    // double function related variables
    public bool is_this_double_function_operand = false;

    // play log related variables
    public Dictionary<string, Vector2> saved_positions = new Dictionary<string, Vector2>();

    // global stroke details
    public GameObject details_dropdown;
    public bool video;

    // spline variables
    public int spline_dist;
    
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
        spline_dist = UnityEngine.Random.Range(2, 4);

        if (paintable_object.GetComponent<Paintable>().color_picker.activeSelf)
            transform.GetComponent<LineRenderer>().material.SetColor("_Color", paintable_object.GetComponent<Paintable>().color_picker_script.color);
        else if (video)
            transform.GetComponent<LineRenderer>().material.SetColor("_Color", Color.red);
        else
            transform.GetComponent<LineRenderer>().material.SetColor("_Color", Color.gray);

        edge_weight = 1;
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

    public void addDot()
    {
        for (int x = 0; x < 2; x++)
        {
            GameObject temp = Instantiate(dot_prefab, transform.GetComponent<LineRenderer>().GetPosition(x), Quaternion.identity, transform);
            temp.name = "dot_child";
            temp.transform.parent = transform;
            temp.transform.SetSiblingIndex(x);

            if (directed_edge && x == 1)
            {
                temp.GetComponent<SpriteRenderer>().sprite = directed_edge_sprite;
                Vector3 direction = transform.GetComponent<LineRenderer>().GetPosition(1) - transform.GetComponent<LineRenderer>().GetPosition(0);
                temp.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI);
            }
            else if (directed_edge && x == 0)
            {
                temp.GetComponent<SpriteRenderer>().sprite = null;
            }
        }
    }

    public void updateDot()
    {
        LineRenderer l = transform.GetComponent<LineRenderer>();

        for (int x = 0; x < 2; x++)
        {
            Transform temp = transform.GetChild(x);
            temp.position = l.GetPosition(x);

            if (directed_edge && x == 1)
            {
                Vector3 direction = l.GetPosition(1) - l.GetPosition(0);
                temp.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI);
            }
        }
    }

    public void updateEndPoint(GameObject node_name)
    {        
        var l = transform.GetComponent<LineRenderer>();

        GameObject source = transform.GetComponent<VectorElementScript>().edge_start;
        GameObject target = transform.GetComponent<VectorElementScript>().edge_end;

        if (source == node_name || target == node_name)
        {
            // set line renderer end point
            //l.SetPosition(0, source.GetComponent<iconicElementScript>().edge_position);
            //l.SetPosition(1, target.GetComponent<iconicElementScript>().edge_position);

            l.SetPosition(0, source.GetComponent<iconicElementScript>().getclosestpoint(target.GetComponent<iconicElementScript>().edge_position));
            l.SetPosition(1, target.GetComponent<iconicElementScript>().getclosestpoint(l.GetPosition(0)));

            // assuming edge_start is always an anchor
            var edgepoints = new List<Vector3>() { l.GetPosition(0), l.GetPosition(1) };

            transform.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
            {
                var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
                return new Vector2(pos.x, pos.y);
            }).ToArray();

            //transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;            
            updateDot();
        }
    }

    // edge creation
    public void addEndPoint(bool video = false)
    {
        if (transform.gameObject.GetComponent<TrailRenderer>() != null) 
            Destroy(transform.gameObject.GetComponent<TrailRenderer>());

        GameObject source = edge_start;
        GameObject target = edge_end;

        LineRenderer l = transform.GetComponent<LineRenderer>();
        if(video)
        l.material.SetColor("_Color", Color.red);
        
        // assuming edge_start is always an anchor
        var edgepoints = new List<Vector3>() { l.GetPosition(0), l.GetPosition(1) };

        transform.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
        {
            var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;

        GameObject temp = Instantiate(dot_prefab, l.GetPosition(1), Quaternion.identity, transform);
        temp.name = "dot_child";
        temp.transform.parent = transform;
        temp.transform.SetSiblingIndex(0);
        if (video)
        {
            temp.transform.localScale = new Vector3(5f, 5f, 5f);
        }
        else
        {
            temp.GetComponent<SpriteRenderer>().color = l.material.color;
        }
           
        temp.GetComponent<SpriteRenderer>().sprite = directed_edge_sprite;
        Vector3 direction = l.GetPosition(1) - l.GetPosition(0);
        // the angle this vector creates corresponding to the x axis
        float angle = Mathf.Atan2(direction.y, direction.x);
        
        //radian to degree conversion, because rotation takes degree angle as input
        temp.transform.localRotation = Quaternion.Euler(0f, 0f, angle * 180 / Mathf.PI);

        if (transform.parent.tag == "vector_parent")
        {
            int magnitude = transform.parent.GetComponent<VectorFieldElement>().magnitude;
            f_x = magnitude * Mathf.Cos(angle);
            f_y = magnitude * Mathf.Sin(angle);
        }
            
    }

    public void addFreeHandPoint()
    {
        GameObject source = edge_start;
        GameObject target = edge_end;

        LineRenderer l = transform.GetComponent<LineRenderer>();
        if (video)
            l.material.SetColor("_Color", Color.red);

        // assuming edge_start is always an anchor        
        //var edgepoints = new List<Vector3>() { l.GetPosition(0), l.GetPosition(l.positionCount-1) };
        Vector3[] points_arr = new Vector3[l.positionCount];
        l.GetPositions(points_arr);
        var edgepoints = points_arr.ToList();

        transform.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
        {
            var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;


        for (int x = 0; x < 2; x++)
        {
            int idx = 0;
            if (x == 1)
                idx = l.positionCount - 1;

            GameObject temp = Instantiate(dot_prefab, l.GetPosition(idx), Quaternion.identity, transform);
            temp.name = "dot_child";
            temp.transform.parent = transform;
            temp.transform.SetSiblingIndex(x);
            if (video)
            {
                temp.transform.localScale = new Vector3(5f, 5f, 5f);
            }
            else
            {
                temp.GetComponent<SpriteRenderer>().color = l.material.color;
            }

            if (directed_edge && x == 1)
            {
                temp.GetComponent<SpriteRenderer>().sprite = directed_edge_sprite;
                Vector3 direction = /*l.GetPosition(idx)*/points_arr[points_arr.Length-1] - /*l.GetPosition(idx-2)*/points_arr[(int)(3f*points_arr.Length/4f)];
                temp.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI);
            }
            else if (directed_edge && x == 0)
            {
                temp.GetComponent<SpriteRenderer>().sprite = null;
            }

        }
    }


    public void updateEndPoint()
    {
        GameObject source = edge_start;
        GameObject target = edge_end;

        LineRenderer l = transform.GetComponent<LineRenderer>();

        // set line renderer end point
        l.SetPosition(0, source.GetComponent<iconicElementScript>().getclosestpoint(target.GetComponent<iconicElementScript>().edge_position));
        l.SetPosition(1, target.GetComponent<iconicElementScript>().getclosestpoint(l.GetPosition(0)));

        /*l.SetPosition(0, source.GetComponent<iconicElementScript>().edge_position);
        l.SetPosition(1, target.GetComponent<iconicElementScript>().edge_position);*/

        // assuming edge_start is always an anchor
        var edgepoints = new List<Vector3>() { l.GetPosition(0), l.GetPosition(1) };

        transform.GetComponent<EdgeCollider2D>().points = edgepoints.Select(x =>
        {
            var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        //transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;

        // set line renderer texture scale
        /*var linedist = Vector3.Distance(transform.GetComponent<LineRenderer>().GetPosition(0),
            transform.GetComponent<LineRenderer>().GetPosition(1));
        transform.GetComponent<LineRenderer>().materials[0].mainTextureScale = new Vector2(linedist, 1);*/

        updateDot();        
    }

    public List<Vector3> myEllipseSpline()
    {
        List<Vector3> spline_pts = new List<Vector3>();
        Vector3 center = (edge_start.GetComponent<iconicElementScript>().edge_position + edge_end.GetComponent<iconicElementScript>().edge_position) / 2;
        float a = edge_end.GetComponent<iconicElementScript>().edge_position.x - center.x;
        float b = edge_start.GetComponent<iconicElementScript>().radius + edge_end.GetComponent<iconicElementScript>().radius +
            Math.Abs(edge_start.GetComponent<iconicElementScript>().edge_position.x - edge_end.GetComponent<iconicElementScript>().edge_position.x);

        //spline_pts.Add(edge_start.GetComponent<iconicElementScript>().edge_position);
        for (int i = 0; i < 90; i = i + 6)
        {
            Vector3 temp = new Vector3(center.x + (a * (float)Math.Cos(i)),
                center.y + (b * (float)Math.Sin(i)),
                -5f);

            // as we are approximating the center, some float fluctuation can hamper the result
            // and cause the generated pont cross edge_start. To prevent this, we use this extra check.
            if (temp.y > edge_start.GetComponent<iconicElementScript>().edge_position.y)
                continue;
            spline_pts.Add(temp);
        }

        //spline_pts.RemoveAt(spline_pts.Count - 1);
        spline_pts.Add(edge_start.GetComponent<iconicElementScript>().edge_position);

        return spline_pts;
    }

    public void updateSplineEndPointOld()
    {
        //recorded_path = myEllipseSpline();

        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();
        /*bs.Initialize(recorded_path.Count);

        for (int ss = 0; ss < recorded_path.Count; ss++)
        {
            bs[ss].position = recorded_path[ss];
        }*/

        // free handle mode and set the control point so that we get a eclipse like shape even with only two points! 
        bs.Initialize(2);
        bs[0].position = edge_start.GetComponent<iconicElementScript>().edge_position;
        bs[0].handleMode = BezierPoint.HandleMode.Free;

        float temp_rad = Math.Max(edge_start.GetComponent<iconicElementScript>().radius, edge_end.GetComponent<iconicElementScript>().radius);
        float temp_x = (edge_start.GetComponent<iconicElementScript>().edge_position.x + edge_end.GetComponent<iconicElementScript>().edge_position.x) / 2;
        float temp_y = edge_start.GetComponent<iconicElementScript>().edge_position.y - temp_rad -
            Math.Abs(edge_start.GetComponent<iconicElementScript>().edge_position.x - edge_end.GetComponent<iconicElementScript>().edge_position.x)/3;
        
        bs[0].followingControlPointPosition = new Vector3(temp_x, temp_y, edge_start.GetComponent<iconicElementScript>().edge_position.z);

        bs[1].position = edge_end.GetComponent<iconicElementScript>().edge_position;
        bs[1].handleMode = BezierPoint.HandleMode.Free;
        bs[1].precedingControlPointPosition = new Vector3(temp_x, temp_y, edge_start.GetComponent<iconicElementScript>().edge_position.z);

        // Now sample 50 points, but decide how many to sample in each section
        // ...

        // Now sample 50 points across the spline with a [0, 1] parameter sweep
        recorded_path = new List<Vector3>(10);
        for (int i = 0; i < 10; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 9, i)));
        }

        Destroy(spline);

        Debug.Log("my_spline:" + recorded_path.Count.ToString());

        transform.GetComponent<LineRenderer>().positionCount = recorded_path.Count;
        transform.GetComponent<LineRenderer>().SetPositions(recorded_path.ToArray());
        
        transform.GetComponent<EdgeCollider2D>().points = recorded_path.Select(x =>
        {
            var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;

        // set line renderer texture scale
        /*var linedist = Vector3.Distance(transform.GetComponent<LineRenderer>().GetPosition(0),
            transform.GetComponent<LineRenderer>().GetPosition(1));
        transform.GetComponent<LineRenderer>().materials[0].mainTextureScale = new Vector2(linedist, 1);*/

        transform.GetChild(0).position = edge_start.GetComponent<iconicElementScript>().edge_position;
        transform.GetChild(1).position = edge_end.GetComponent<iconicElementScript>().edge_position;
    }

    public void updateSplineEndPoint()
    {
        //recorded_path = myEllipseSpline();

        Vector3 start = edge_start.GetComponent<iconicElementScript>().getclosestpoint(edge_end.GetComponent<iconicElementScript>().edge_position);
        Vector3 end = edge_end.GetComponent<iconicElementScript>().getclosestpoint(start);

        /*Vector3 start = edge_start.GetComponent<iconicElementScript>().edge_position;
        Vector3 end = edge_end.GetComponent<iconicElementScript>().edge_position;*/

        Vector3 dir_vec = start - end;
        Vector2 unit_vec = new Vector2(-dir_vec.y, dir_vec.x);
        Debug.Log("before normalized:" + unit_vec.ToString());
        unit_vec.Normalize();
        Debug.Log("after normalized:" + unit_vec.ToString());

        float approx_dist;
        approx_dist = Vector3.Distance(start, end) / spline_dist;

        Vector3 first_cpt = Vector3.Lerp(start, end, 0.5f);
        Vector2 temp_vec = new Vector2(first_cpt.x, first_cpt.y) - (approx_dist * unit_vec);
        first_cpt = new Vector3(temp_vec.x, temp_vec.y, first_cpt.z);

        GameObject spline = new GameObject("spline");
        spline.AddComponent<BezierSpline>();
        BezierSpline bs = spline.transform.GetComponent<BezierSpline>();

        // free handle mode and set the control point so that we get a eclipse like shape even with only two points! 
        bs.Initialize(2);
        bs[0].position = start;
        bs[0].handleMode = BezierPoint.HandleMode.Free;
        bs[0].followingControlPointPosition = first_cpt;

        bs[1].position = end;
        bs[1].handleMode = BezierPoint.HandleMode.Free;
        bs[1].precedingControlPointPosition = first_cpt;

        // Now sample 50 points, but decide how many to sample in each section
        // ...

        // Now sample 50 points across the spline with a [0, 1] parameter sweep
        recorded_path = new List<Vector3>(10);
        for (int i = 0; i < 10; i++)
        {
            recorded_path.Add(bs.GetPoint(Mathf.InverseLerp(0, 9, i)));
        }

        Destroy(spline);

        Debug.Log("my_spline:" + recorded_path.Count.ToString());

        transform.GetComponent<LineRenderer>().positionCount = recorded_path.Count;
        transform.GetComponent<LineRenderer>().SetPositions(recorded_path.ToArray());

        transform.GetComponent<EdgeCollider2D>().points = recorded_path.Select(x =>
        {
            var pos = transform.GetComponent<EdgeCollider2D>().transform.InverseTransformPoint(x);
            return new Vector2(pos.x, pos.y);
        }).ToArray();

        //transform.GetComponent<EdgeCollider2D>().edgeRadius = 10;

        transform.GetChild(0).position = start;
        transform.GetChild(1).position = end;

        if (directed_edge)
        {
            Vector3 direction = end - recorded_path[recorded_path.Count - 2];
            transform.GetChild(1).localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI);
        }
    }

    void OnDestroy()
    {
        /*Transform node_parent = transform.parent;
        if (node_parent.tag == "edge_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().edges_init();
        }
        else if (node_parent.tag == "simplicial_parent")
        {
            node_parent.parent.GetComponent<GraphElementScript>().simplicial_init();
        }*/
    }
}