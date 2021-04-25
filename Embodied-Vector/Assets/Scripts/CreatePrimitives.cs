using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
using System.IO;
/*using uPIe;
using PDollarGestureRecognizer;

using Symbolism;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;*/

public class CreatePrimitives : MonoBehaviour
{

    // Length, area, distance units
    public static float unitScale = 0.025f;
    public Material solid_mat;
    public Material transparent_mat;

    public GameObject PenLine;
    public GameObject EdgeLinePrefab;
    public GameObject FunctionLine;

    public Color[] colors;

    public static GameObject iconicElementButton;
    public static GameObject pan_button;
    /*
    public static GameObject select_button;
    public static GameObject edge_button;
    public static GameObject function_button;
    public static GameObject eraser_button;
    public static GameObject staticpen_button;*/

    // Prefabs
    public GameObject IconicElement;

    public GameObject paintable_object;

    // background materials
    public Material default_background_material, grid_material;

    // line, set, function materials
    public Material set_line_material, function_line_material;

    //private VectorLine tempLine;
    //public List<VectorLine> all_lines = new List<VectorLine>();

    // predictive stroke
    // private List<Gesture> trainingSet = new List<Gesture>();
    // private List<Point> predictive_points = new List<Point>();
    private float predictive_stroke_up_time = 0f;
    private int strokeID = 0;
    public Sprite[] commonShapes;

    //public int totalLines = 0;

    void Start()
    {
        
    }

    // Start is called before the first frame update
    void Awake()
    {
        iconicElementButton = GameObject.Find("IconicPen");
        pan_button = GameObject.Find("Pan");

        paintable_object = GameObject.FindGameObjectWithTag("paintable_canvas_object");

        colors = new Color[8];
        //https://flatuicolors.com/palette/defo
        colors[0] = new Color32(13, 134, 34, 1); //green_sea;
        colors[1] = new Color32(255, 0, 0, 1); //pure_Red;
        colors[2] = new Color32(155, 89, 182, 1);//amyesth;
        colors[3] = new Color32(230, 126, 34, 1);//carrot;
        colors[4] = new Color32(237, 76, 103, 1);//bara_red;
        colors[5] = new Color32(217, 128, 250, 1);// lavender_tea;
        colors[6] = new Color32(77, 77, 77, 1);// ash
        colors[7] = new Color32(255, 140, 0, 1);// dark_orange

        // Predictive stroke
        // Load pre-made gestures
        /*TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/BasicShapeTemplates/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        commonShapes = Resources.LoadAll<Sprite>("Shapes/CommonShapes");*/
    }

    public GameObject FinishStaticLine(GameObject templine, Mesh combinedMesh = null)
    {

        // compute centroid and bounds
        templine.GetComponent<iconicElementScript>().computeCentroid();
        templine.GetComponent<iconicElementScript>().computeBounds();

        // set line renderer, width
        templine.GetComponent<LineRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;

        templine.GetComponent<LineRenderer>().numCapVertices = 15;
        templine.GetComponent<LineRenderer>().numCornerVertices = 15;

        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<iconicElementScript>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<iconicElementScript>().points.ToArray());
       
        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.GetComponent<MeshFilter>();


        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
        }

        //meshObj.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<iconicElementScript>().icon_elem_material;
        templine.GetComponent<MeshRenderer>().sharedMaterial = lineRenderer.material;

        // get rid of the line renderer?
        Destroy(templine.GetComponent<LineRenderer>());
        Destroy(templine.GetComponent<TrailRenderer>());
        Destroy(templine.GetComponent<iconicElementScript>());

        // add a collider        
        templine.AddComponent<BoxCollider>();
        templine.GetComponent<BoxCollider>().size = templine.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        templine.GetComponent<BoxCollider>().center = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;;

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // disable the collider because we are in the pen mode right now. Pan mode enables back all colliders.
        templine.GetComponent<BoxCollider>().enabled = true;

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0);

        return templine;
    }


    // ======================== PenLine Free-form ============================

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishPenLine(GameObject templine, Mesh combinedMesh = null)
    {
        
        // compute centroid and bounds
        templine.GetComponent<iconicElementScript>().computeCentroid();
        templine.GetComponent<iconicElementScript>().computeBounds();

        // set line renderer, width
        templine.GetComponent<LineRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;

        templine.GetComponent<LineRenderer>().numCapVertices = 15;
        templine.GetComponent<LineRenderer>().numCornerVertices = 15;

        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<iconicElementScript>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<iconicElementScript>().points.ToArray());

        // attached with the parent, so no need of initialization any further
        /*GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<MeshFilter>();
        meshObj.AddComponent<MeshRenderer>();*/

        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.GetComponent<MeshFilter>();
               

        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh, true);
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
        }        

        //meshObj.GetComponent<MeshRenderer>().sharedMaterial = templine.GetComponent<iconicElementScript>().icon_elem_material;
        templine.GetComponent<MeshRenderer>().sharedMaterial = lineRenderer.material;

        // get rid of the line renderer?
        Destroy(templine.GetComponent<LineRenderer>());

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;

        // add a collider        
        templine.AddComponent<BoxCollider>();
        templine.GetComponent<BoxCollider>().size = templine.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        templine.GetComponent<BoxCollider>().center = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;

        // for proper positioning
        templine.GetComponent<iconicElementScript>().edge_position = templine.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        templine.GetComponent<iconicElementScript>().bounds_center = templine.GetComponent<iconicElementScript>().edge_position;

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // disable the collider because we are in the pen mode right now. Pan mode enables back all colliders.
        templine.GetComponent<BoxCollider>().enabled = false;

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0);

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<iconicElementScript>().previous_position = templine.transform.position;   

        // Save the area of the bounding box 
        templine.GetComponent<iconicElementScript>().attribute.Area =
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.x *
            templine.GetComponent<MeshFilter>().sharedMesh.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<iconicElementScript>().current_attribute =
            templine.GetComponent<iconicElementScript>().attribute.Length;
        
        return templine;
    }

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishGraphLine(GameObject templine, bool lassocolor = false, int rank = 0)
    {
        var lineRenderer = templine.GetComponent<GraphElementScript>().graph_Details.GetComponent<LineRenderer>();
        var MeshRenderer = templine.GetComponent<GraphElementScript>().graph_Details.GetComponent<MeshRenderer>();
        var meshFilter = templine.GetComponent<GraphElementScript>().graph_Details.GetComponent<MeshFilter>();

        lineRenderer.numCapVertices = 15;
        lineRenderer.numCornerVertices = 15;
        lineRenderer.widthMultiplier = 2;
        lineRenderer.positionCount = templine.GetComponent<GraphElementScript>().points.Count;
        lineRenderer.SetPositions(templine.GetComponent<GraphElementScript>().points.ToArray());

        
        if (lassocolor)
        {
            Color color = colors[rank % (colors.Length)];

            //color = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)); 
            /*How to change color of material -Unity Forum
            https://forum.unity.com/threads/how-to-change-color-of-material.874921/ */

            Material new_material = new Material(solid_mat);
            new_material.SetColor("_Color", color);
            MeshRenderer.sharedMaterial = new_material;
            lineRenderer.sharedMaterial = new_material;
        }
        else
        {
            Material new_material = new Material(solid_mat);
            new_material.SetColor("_Color", Color.red);
            MeshRenderer.sharedMaterial = new_material;
            lineRenderer.sharedMaterial = new_material;
        }


        // If a combined mesh is not passed, use the line renderer mesh (default case)        
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshFilter.sharedMesh = mesh;
        Destroy(lineRenderer);

        templine.GetComponent<GraphElementScript>().graph_Details.transform.position = new Vector3(0, 0, 0); 
        return templine;
    }

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public Material FindGraphMaterial(bool lassocolor = false, int rank = 0)
    {
        Material new_material = new Material(transparent_mat);
        if (lassocolor)
        {
            Color color = colors[rank % (colors.Length)];
            new_material.SetColor("_Color", new Color(color.r, color.g, color.b, 0.2f));
        }
        else
        {
            new_material.SetColor("_Color", new Color(0.5f, 0.5f, 0.5f, 0.2f));
        }
        return new_material;
    }


    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishFunctionLine(GameObject templine, bool lassocolor = false, int rank = 0)
    {
        
        // compute centroid and bounds
        //templine.GetComponent<FunctionElementScript>().computeCentroid();
        templine.GetComponent<FunctionElementScript>().computeBounds();

        // set line renderer, width
        templine.GetComponent<LineRenderer>().widthCurve = templine.GetComponent<FunctionElementScript>().widthcurve;

        templine.GetComponent<LineRenderer>().numCapVertices = 15;
        templine.GetComponent<LineRenderer>().numCornerVertices = 15;

        templine.GetComponent<LineRenderer>().positionCount = templine.GetComponent<FunctionElementScript>().points.Count;
        templine.GetComponent<LineRenderer>().SetPositions(templine.GetComponent<FunctionElementScript>().points.ToArray());

        var lineRenderer = templine.GetComponent<LineRenderer>();
        if (lassocolor)
        {
            Color color = colors[rank % (colors.Length)];
            //color = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)); 
            
            
            /*How to change color of material -Unity Forum
            https://forum.unity.com/threads/how-to-change-color-of-material.874921/ */
            Material new_material = new Material(solid_mat);
            new_material.SetColor("_Color", color);
            templine.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().sharedMaterial = new_material;
        }
        else
        {
            templine.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().sharedMaterial =
                templine.GetComponent<FunctionElementScript>().icon_elem_material;
        }

        var meshFilter = templine.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshFilter>();
                

        // If a combined mesh is not passed, use the line renderer mesh (default case)        
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshFilter.sharedMesh = mesh; 

        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;
        templine.GetComponent<LineRenderer>().enabled = false;
        templine.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().enabled = true;

        templine.GetComponent<FunctionElementScript>().edge_position = meshFilter.sharedMesh.bounds.center;
                
        // set transform position
        templine.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;
        templine.GetComponent<FunctionElementScript>().mesh_holder.SetActive(true);
        templine.GetComponent<FunctionElementScript>().mesh_holder.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;
        //templine.GetComponent<FunctionElementScript>().points.Clear();
        return templine;
    }

    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishVideoFunctionLine(GameObject templine, bool lassocolor = false, int rank = 0)
    {
        var lineRenderer = templine.GetComponent<LineRenderer>();
        // set line renderer, width
        lineRenderer.widthCurve = templine.GetComponent<FunctionElementScript>().widthcurve;

        lineRenderer.numCapVertices = 15;
        lineRenderer.numCornerVertices = 15;

        lineRenderer.positionCount = templine.GetComponent<FunctionElementScript>().points.Count;
        lineRenderer.SetPositions(templine.GetComponent<FunctionElementScript>().points.ToArray());
                
        if (lassocolor)
        {
            Color color = colors[rank % (colors.Length)];
            //color = new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f)); 


            /*How to change color of material -Unity Forum
            https://forum.unity.com/threads/how-to-change-color-of-material.874921/ */
            Material new_material = new Material(solid_mat);
            new_material.SetColor("_Color", color);
            lineRenderer.material = new_material;
        }
        else
        {
            lineRenderer.material = templine.GetComponent<FunctionElementScript>().icon_elem_material;
        }
        
        // disable trail renderer, no longer needed
        templine.GetComponent<TrailRenderer>().enabled = false;
        templine.GetComponent<FunctionElementScript>().mesh_holder.SetActive(false);
        templine.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().enabled = false;

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0); //meshObj.transform.position;
        //templine.GetComponent<FunctionElementScript>().points.Clear();

        Debug.Log("inside_videolasso");
        return templine;
    }


    // Assumes templine has been initialized in pointer.start and pointer.moved
    public GameObject FinishEdgeLine(GameObject templine, Mesh combinedMesh = null)
    {
        var lineRenderer = templine.GetComponent<LineRenderer>();
        var meshFilter = templine.AddComponent<MeshFilter>();
        var meshRenderer = templine.AddComponent<MeshRenderer>();

        lineRenderer.widthMultiplier = 3f;
        templine.GetComponent<TrailRenderer>().widthMultiplier = 3f;

        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);

        // If a combined mesh is not passed, use the line renderer mesh (default case)
        if (combinedMesh == null)
        {
            meshFilter.sharedMesh = mesh;
        }
        else
        {
            meshFilter.sharedMesh = combinedMesh;
        }

        meshRenderer.sharedMaterial = lineRenderer.material;
        templine.GetComponent<TrailRenderer>().enabled = false;        
        lineRenderer.enabled = false;        

        // set transform position
        templine.transform.position = new Vector3(0, 0, 0);
        templine.GetComponent<VectorElementScript>().edge_weight = Mathf.RoundToInt(0.5f * templine.GetComponent<VectorElementScript>().totalLength);
        return templine;
    }

    // LOAD A SPRITE AS A PENLINE OBJECT
    /*public GameObject CreatePenLine(string sprite_filename)
    {
        GameObject templine;

        // CODE FROM POINTER.START

        paintable_object.GetComponent<Paintable>().totalLines++;

        templine = Instantiate(PenLine, GameObject.FindGameObjectWithTag("paintable_canvas_object").transform);
        templine.GetComponent<TrailRenderer>().material.color = Color.black;

        templine.name = "penLine_" + paintable_object.GetComponent<Paintable>().totalLines.ToString();
        templine.tag = "penline";

        templine.GetComponent<iconicElementScript>().isThisAnImage = true;
        //Sprite sprite = Resources.Load<Sprite>("ExampleImages/" + sprite_filename.Replace(".png", ""));
        Sprite sprite = IMG2Sprite.instance.LoadNewSprite(sprite_filename, 2f);
        templine.GetComponent<iconicElementScript>().sprite = sprite;
        templine.GetComponent<iconicElementScript>().sprite_filename = sprite_filename;

        templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;

        // color and width

        // disable the argument_label button, currently it's at the 3rd index
        templine.transform.GetChild(2).gameObject.SetActive(false);

        // FIND CONVEX HULL
        Vector3[] vertices = new Vector3[sprite.vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x = sprite.vertices[i].x;
            vertices[i].y = sprite.vertices[i].y;
            vertices[i].z = -40f;
        }

        // Compute convex hull. A concavity value less than 30 may produce tighter shape, but point in polygon test starts failing at that point.
        var hullAPI = new HullAPI();
        var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = vertices, Concavity = 30 });

        vertices = hull.vertices;

        // sort vertices according to angle

        // Store angles between texture's center and vertex position vector
        float[] angles = new float[vertices.Length];

        // Calculate the angle between each vertex and the texture's /center
        for (int i = 0; i < vertices.Length; i++)
        {
            angles[i] = Paintable_Script.AngleBetweenVectors(vertices[i], templine.transform.position);
            //vertexArray[i] -= texCenter;   // Offset vertex about texture center
        }

        // Sort angles into ascending order, use to put vertices in clockwise order
        Array.Sort(angles, vertices);

        templine.GetComponent<penLine_script>().points = vertices.ToList();

        // CODE FROM POINTER.MOVE
        templine.GetComponent<penLine_script>().calculateLengthAttributeFromPoints();

        templine.GetComponent<penLine_script>().widthcurve.AddKey(0f, 1f);
        templine.GetComponent<penLine_script>().widthcurve.AddKey(1f, 1f);

        // FINISH THE TEMPLINE

        // set the gesture/legible layer fields: no prediction was done. Set \ space as default.
        templine.GetComponent<penLine_script>().isPredictionDone = true;
        templine.GetComponent<penLine_script>().gestureTemplate = "legrectangle";

        // this one is needed for the penline script functions
        templine.GetComponent<penLine_script>().paintable_object =
            GameObject.FindGameObjectWithTag("paintable_canvas_object");

        // compute centroid and bounds
        templine.GetComponent<penLine_script>().computeCentroid();
        templine.GetComponent<penLine_script>().computeBounds();

        templine.GetComponent<LineRenderer>().enabled = false;
        templine.GetComponent<TrailRenderer>().enabled = false;

        GameObject meshObj = new GameObject("_meshobj");
        meshObj.AddComponent<SpriteRenderer>();

        meshObj.GetComponent<SpriteRenderer>().sprite = sprite;

        // add a collider
        templine.AddComponent<BoxCollider>();

        templine.GetComponent<BoxCollider>().size = meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size;
        templine.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
        //Debug.Log("box collider after: " + templine.GetComponent<BoxCollider>().center.ToString());

        // set collider trigger
        templine.GetComponent<BoxCollider>().isTrigger = true;

        // enable the collider and enable pan mode
        templine.GetComponent<BoxCollider>().enabled = true;
        Paintable_Script.pan_button.GetComponent<AllButtonsBehavior>().whenSelected();

        // set transform position
        templine.transform.position = new Vector3(0, 0, -40);

        // update the previous_position variable for templine for checkMove()
        templine.GetComponent<penLine_script>().previous_position = templine.transform.position;

        meshObj.transform.SetParent(templine.transform);

        // set sibling index. I want this to be Child 0, and the param_text component child 1.
        meshObj.transform.SetAsFirstSibling();

        // Save the area of the bounding box 
        templine.GetComponent<penLine_script>().attribute.Area =
            meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size.x *
            meshObj.GetComponent<SpriteRenderer>().sprite.bounds.size.y * unitScale * unitScale;

        // set current_attribute of penLine
        templine.GetComponent<penLine_script>().current_attribute =
            templine.GetComponent<penLine_script>().attribute.Length;

        // set the _name field of penline
        templine.GetComponent<penLine_script>()._name = "line";
        templine.GetComponent<penLine_script>().symbol_name = new Symbol("line");

        // now transform all points in the line script to local positions
        templine.GetComponent<penLine_script>().fromGlobalToLocalPoints();

        // set up the text labels for transformation GUI (param_text)
        templine.transform.GetChild(1).localScale = new Vector3(4, 4, 1);
        templine.transform.GetChild(2).localScale = new Vector3(4, 4, 1);
        templine.transform.GetChild(4).GetChild(0).localScale = new Vector3(4, 4, 1);

        // set the box collider as the size of the rect transform
        templine.transform.GetChild(4).GetChild(0).GetComponent<BoxCollider2D>().size =
            templine.transform.GetChild(4).GetChild(0).GetComponent<RectTransform>().sizeDelta;

        templine.transform.GetChild(4).GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(4).GetChild(0).GetComponent<BoxCollider2D>().enabled = false;

        templine.transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(2).GetComponent<BoxCollider2D>().enabled = false;

        templine.transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
        templine.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = false;
        // turn off the length (temporary display) text
        templine.transform.GetChild(2).GetComponent<TextMeshPro>().text = "0.";

        // Set up the argument_label button and text.
        // get the highest point on the pen line, in local coordinates
        templine.transform.GetChild(3).gameObject.SetActive(true);

        templine.GetComponent<penLine_script>().computeBounds();
        templine.GetComponent<penLine_script>().computeCentroid();

        templine.transform.GetChild(3).transform.localPosition =
            templine.GetComponent<penLine_script>().points[
                HighestMeshPointIndex(templine.GetComponent<penLine_script>().points)
                ];
        //new Vector3(templine.GetComponent<penLine_script>().centroid.x,
        //templine.GetComponent<penLine_script>().maxy, 0);   // z is already -40, hence putting 0 here in local.

        templine.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);

        templine.transform.GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

        templine.transform.GetChild(3).gameObject.SetActive(false); // turn it off now. Needed only for double function.

        // Don't check for membership in this case. Leave it be.

        return templine;
    }*/

    // ASSUMES: 1. points are in global space (not local)
    public GameObject CreatePenLine(GameObject[] penlines)
    {
        GameObject templine;

        List<GameObject> pen_meshobjs = new List<GameObject>();

        List<Vector3> points = new List<Vector3>();        

        int totalLength = 0, totalArea = 0;

        for (int k = 0; k < penlines.Length; k++)
        {
            if (penlines[k].GetComponent<MeshFilter>().sharedMesh != null)
        {                   
            pen_meshobjs.Add(penlines[k].transform.gameObject);

            points.AddRange(penlines[k].GetComponent<iconicElementScript>().points);

            totalLength += (int)penlines[k].GetComponent<iconicElementScript>().attribute.Length;
        }
        }

        // stop if no penline mesh objs found
        if (pen_meshobjs.Count == 0) return null;
        //else Debug.Log("total penline objects "+ pen_meshobjs.Count.ToString());
        
        CombineInstance[] combine = new CombineInstance[pen_meshobjs.Count];

        for (int i = 0; i < pen_meshobjs.Count; i++)
        {
            combine[i].mesh = pen_meshobjs[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = pen_meshobjs[i].GetComponent<MeshFilter>().transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);
        }

        Mesh cmesh = new Mesh();
        cmesh.CombineMeshes(combine);

        Paintable.totalLines++;

        templine = Instantiate(paintable_object.GetComponent<Paintable>().IconicElement, paintable_object.GetComponent<Paintable>().Objects_parent.transform);
        templine.GetComponent<TrailRenderer>().material = penlines[0].GetComponent<MeshRenderer>().sharedMaterial;
        templine.GetComponent<LineRenderer>().material = penlines[0].GetComponent<MeshRenderer>().sharedMaterial;

        int icon_num = Paintable.totalLines;
        templine.name = "iconic_" + icon_num.ToString();
        templine.tag = "iconic";
        templine.GetComponent<iconicElementScript>().icon_number = icon_num;
        templine.GetComponent<iconicElementScript>().icon_name = "iconic_" + icon_num.ToString();

        templine.transform.GetComponent<MeshRenderer>().enabled = true;
        if (templine.transform.GetComponent<BoxCollider2D>() != null)
            templine.transform.GetComponent<BoxCollider2D>().enabled = false;

        // color and width
        templine.GetComponent<TrailRenderer>().widthMultiplier = 1f;
        templine.GetComponent<LineRenderer>().widthMultiplier = 1f;

        
        // CODE FROM POINTER.MOVE
        templine.GetComponent<iconicElementScript>().attribute.Length = totalLength;
        //templine.GetComponent<penLine_script>().calculateLengthAttributeFromPoints();

        // if no width curve exists then assign a generic width cuve (won't get used for final mesh render anyway)
        templine.GetComponent<iconicElementScript>().widthcurve.AddKey(0f, 1f);
        templine.GetComponent<iconicElementScript>().widthcurve.AddKey(1f, 1f);

        templine.GetComponent<iconicElementScript>().points = points;

        // FINISH THE TEMPLINE, PASS COMBINED MESH
        templine = FinishPenLine(templine, cmesh);

        for (int i = 0; i < pen_meshobjs.Count; i++)
            Destroy(pen_meshobjs[i]);

        //paintable_object.GetComponent<Paintable>().templine = templine;
        return templine;
    }
    
    public int HighestMeshPointIndex(List<Vector3> points)
    {
        // which local point is the highest? Return its index.
        // find maxy from all the points
        float mxy = -100000f;
        int ind = -1;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].y > mxy)
            {
                mxy = points[i].y;
                ind = i;
            }
        }
        return ind;
    }

    public int LowestMeshPointIndex(List<Vector3> points)
    {
        // which local point is the highest? Return its index.
        // find maxy from all the points
        float mny = 100000f;
        int ind = -1;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].y < mny)
            {
                mny = points[i].y;
                ind = i;
            }
        }
        return ind;
    }
}
