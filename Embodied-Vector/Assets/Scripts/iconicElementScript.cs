using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.InputSystem;

public class iconicElementScript : MonoBehaviour
{

    public bool video_icon;
    public bool image_icon;

    // previous first child, instead of creating a separate child, we now want to keep it in the script
    //public Mesh _mesh;

    // visual representation
    // public List<Vector2> points;

    // object drawing properties
    public List<Vector3> points = new List<Vector3>();
    public Vector3 centroid;
    // actual radius
    public float radius;
    // offset from radius
    public float radius_margin;

    public float maxx = -100000f, maxy = -100000f, minx = 100000f, miny = 100000f;
    public bool draggable_now = false;
    public bool menu_open = false;

    public Material icon_elem_material;
    public GameObject paintable_object;
    public string icon_name;
    // needed for abstraction conversion
    public int icon_number;

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

    // path record
    public List<Vector3> recorded_path = new List<Vector3>();
    public List<Vector3> hull_pts = new List<Vector3>();
    public bool record_path_enable = false;
    public Vector3 position_before_record;
    public Vector3 edge_position;
    public Vector3 bounds_center;

    // path translate
    public Vector3 position_before_translation;
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
    public bool apply_scale = false;

    // other interaction variables
    private Vector3 touchDelta = new Vector3();

    // pressure based line width
    public List<float> pressureValues = new List<float>();
    public AnimationCurve widthcurve = new AnimationCurve();
    float totalLength = 0f;
    List<float> cumulativeLength = new List<float>();
    int movingWindow = 5;

    // Pen Interaction
    Pen currentPen;

    // double function related variables
    public bool is_this_double_function_operand = false;

    // play log related variables
    public Dictionary<string, Vector2> saved_positions = new Dictionary<string, Vector2>();

    // global stroke details
    public GameObject details_dropdown;
    public bool global_details_on_path = true;

    public Sprite recognized_sprite;

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

        computeRadius();
    }

    public void computeRadius()
    {
        int Count = 0;
        int interval = Mathf.Max(1,(int)Math.Floor((float)(points.Count / 10)));

        float totalx = 0, totaly = 0, totalz = 0;
        float distance = 0;

        for (int i = 0; i < points.Count; i = i + interval)
        {
            totalx = points[i].x - centroid.x;
            totaly = points[i].y - centroid.y;
            totalz = points[i].z - centroid.z;
            distance += (float)Math.Sqrt((totalx * totalx) + (totaly * totaly) + (totalz * totalz));
            Count++;
        }
        radius = distance / Count;
    }

    public List<Vector3> hullPoints(float margin = 0f)
    {
        if (margin == 0f)
            margin = radius_margin;

        hull_pts = new List<Vector3>();

        for (int i = 0; i < 361; i = i + 36)
        {
            hull_pts.Add(new Vector3(edge_position.x + ((radius + margin) * (float)Math.Cos(i)),
                edge_position.y + ((radius + margin) * (float)Math.Sin(i)),
                -5f));
        }

        Debug.Log("hull_pts_check:" + hull_pts.Count.ToString());

        return hull_pts;
    }

    public void computeBounds()
    {
        maxx = -100000f; maxy = -100000f; minx = 100000f; miny = 100000f;

        for (int i = 0; i < points.Count; i++)
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

    
    public void deleteObject(GameObject radmenu)
    {
        // Destroy the radial menu
        Destroy(radmenu);

        // TODO: DELETE ANY EDGELINE ASSOCIATED WITH THIS PEN OBJECT

        // Destroy the object attached to this script
        Destroy(this.gameObject);
    }

    
    // load a new image and convert to sprite 
    // https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/ (the solution by Freznosis#5)        
    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();
        return NewSprite;
    }

    public Sprite LoadSprite(Texture2D SpriteTexture, /*float x, float y, float width, float height,*/ float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),//(x, y, width, height), 
            new Vector2(0, 0), PixelsPerUnit, 0, spriteType);
        recognized_sprite = NewSprite;
        SpriteToRender();
        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public void SpriteToRender()
    {
        
        // rescaling, otherwise the projected mesh is too small
        transform.localScale = new Vector3(30f, 30f, 1f);

        transform.gameObject.AddComponent<BoxCollider>();
        transform.gameObject.AddComponent<SpriteRenderer>();
        transform.GetComponent<SpriteRenderer>().sprite = recognized_sprite;

        //Debug.Log("box collider before: " + templine.GetComponent<BoxCollider>().center.ToString());
        transform.GetComponent<BoxCollider>().size = transform.GetComponent<SpriteRenderer>().sprite.bounds.size;
        transform.GetComponent<BoxCollider>().center = new Vector3(transform.GetComponent<SpriteRenderer>().sprite.bounds.center.x,
                            transform.GetComponent<SpriteRenderer>().sprite.bounds.center.y,
                            -5f);        

        radius = transform.GetComponent<SpriteRenderer>().bounds.extents.magnitude;
        
        // set collider trigger
        transform.GetComponent<BoxCollider>().isTrigger = true;
        transform.GetComponent<BoxCollider>().enabled = true;

        edge_position = transform.GetComponent<SpriteRenderer>().bounds.extents;
        edge_position.z = -5f;

        bounds_center = edge_position;

    }

    public Mesh createQuad(float width, float height)
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
    }

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
    

    private void Awake()
    {
        // setting these at the declaration section does not work for some reason, so declaring here
        min_rotation_val = 0f;
        max_rotation_val = 5f;
        min_scale_val = 1f;
        max_scale_val = 5f;

        //details_dropdown = GameObject.Find("Details_Dropdown");
        paintable_object = GameObject.Find("Paintable");
    }


    // Start is called before the first frame update
    void Start()
    {
        radius_margin = 40;
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
        bool inside = false;
        for (int i = 0; i < points.Count; j = i++)
        {
            if (((points[i].y <= p.y && p.y < points[j].y) || (points[j].y <= p.y && p.y < points[i].y)) &&
               (p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                inside = !inside;

            }

        }
        return inside;
    }

    public Vector3 getclosestpoint(Vector3 target)
    {
        if (video_icon) return edge_position;
        
        if (image_icon)
        {
            points.Clear();
            points.Add(edge_position + new Vector3(-bounds_center.x, 0, 0));
            points.Add(edge_position + new Vector3(0, bounds_center.y, 0));
            points.Add(edge_position + new Vector3(bounds_center.x, 0, 0));
            points.Add(edge_position + new Vector3(0, -bounds_center.y, 0));

            Vector3 desired_point = points[0];
            float distance = Vector3.Distance(desired_point, target);

            for (int i = 1; i < points.Count; i = i + 1)
            {
                // recalculate repositioned points
                Vector3 calibrated_pt = points[i];
                float temp_dist = Vector3.Distance(calibrated_pt, target);

                if (temp_dist < distance)
                {
                    desired_point = points[i];
                    distance = temp_dist;
                }

            }

            return desired_point;
        }

        return edge_position;
    }

    public void getImagepts()
    {
        points.Clear();
        points.Add(edge_position + new Vector3(-bounds_center.x, 0, 0));
        points.Add(edge_position + new Vector3(0, bounds_center.y, 0));
        points.Add(edge_position + new Vector3(bounds_center.x, 0, 0));
        points.Add(edge_position + new Vector3(0, -bounds_center.y, 0));

        maxx = edge_position.x + bounds_center.x;
        maxy = edge_position.y + bounds_center.y; 
        minx = edge_position.x - bounds_center.x;
        miny = edge_position.y - bounds_center.y;
    }

    void OnDestroy()
    {
        Transform node_parent = transform.parent;
        if (paintable_object != null && node_parent != null && node_parent.tag == "node_parent")
        {
            paintable_object.GetComponent<Paintable>().searchNodeAndDeleteEdge(transform.gameObject);
        }
    }

    public void searchNodeAndUpdateEdge()
    {
        if (video_icon) return;
        if (transform.parent.tag != "node_parent") return;

        Transform Prev_node_parent = transform.parent;
        Transform Prev_graph_parent = Prev_node_parent.transform.parent;
        Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
        Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
        Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
        Transform[] simplicials = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
        Transform[] hyper_edges = Prev_hyper_parent.GetComponentsInChildren<Transform>();

        bool splined_edge_flag = Prev_graph_parent.GetComponent<GraphElementScript>().splined_edge_flag;

        foreach (Transform child in allChildrenedge)
        {            
            if (child.tag == "edge")
            {
                if (child.GetComponent<VectorElementScript>().free_hand
                    && (child.GetComponent<VectorElementScript>().edge_start == transform.gameObject ||
                    child.GetComponent<VectorElementScript>().edge_end == transform.gameObject))
                {
                    Destroy(child.GetComponent<MeshRenderer>());
                    Destroy(child.GetComponent<MeshFilter>());
                    var lineRenderer = child.GetComponent<LineRenderer>();
                    lineRenderer.enabled = true;
                    lineRenderer.positionCount = 2;
                    lineRenderer.widthMultiplier = 1f;

                    child.GetComponent<VectorElementScript>().free_hand = false;
                    child.GetComponent<VectorElementScript>().edge_weight = 1;
                    Prev_graph_parent.GetComponent<GraphElementScript>().edges_init();
                }

                if (splined_edge_flag)
                    child.GetComponent<VectorElementScript>().updateSplineEndPoint();
                else
                    child.GetComponent<VectorElementScript>().updateEndPoint();
            }
        }

        
    }

    // called only when dragging is done, to save computation cost
    public void searchFunctionAndUpdateLasso()
    {
        if (video_icon) return;
        GameObject[] all_functions = GameObject.FindGameObjectsWithTag("function");

        foreach (GameObject cur_function in all_functions)
        {
            //if (cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().instant_eval)
            if ((cur_function.transform.childCount > 2) /*&& cur_function.transform.GetChild(0).gameObject.activeSelf*/)
            {
                // if current icon is under the result graph
                
                if (transform.parent.tag == "node_parent" && cur_function.transform.GetChild(1).tag == "graph" &&
                        transform.parent.parent.gameObject == cur_function.transform.GetChild(1).gameObject)
                {
                    cur_function.GetComponent<FunctionElementScript>().updateLassoPointsIconDrag();
                    continue;
                }

                if (cur_function.GetComponent<FunctionElementScript>().mesh_holder.activeSelf == false) continue;

                // check if any function argument has been assigned
                if (cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects == null) continue;

                foreach (GameObject function_argument in cur_function.transform.GetChild(0).GetComponent<FunctionMenuScript>().argument_objects)
                {
                    if (function_argument == null) continue;

                    // if the argument is a graph which contains this object, then call update lasso
                    if (function_argument.tag == "graph" &&
                        transform.parent.tag == "node_parent" &&
                        transform.parent.parent.gameObject == function_argument)
                    {
                        cur_function.GetComponent<FunctionElementScript>().updateLassoPointsIconDrag();
                        break;
                    }
                    else if (function_argument.tag == "function" && function_argument.transform.GetChild(1).tag == "graph" &&
                        transform.parent.tag == "node_parent" &&
                        transform.parent.parent.gameObject == function_argument.transform.GetChild(1).gameObject)
                    {
                        cur_function.GetComponent<FunctionElementScript>().updateLassoPointsIconDrag();
                        break;
                    }
                    // else if the argument is an icon and matches with this object, call update lasso
                    else if (function_argument.tag == "iconic" && function_argument == transform.gameObject)
                    {
                        cur_function.GetComponent<FunctionElementScript>().updateLassoPointsIconDrag();
                        break;
                    }
                }
            }
        }

    }
}