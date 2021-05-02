using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine.EventSystems;

public class Paintable : MonoBehaviour
{

    public static bool allowOpacity = false;

    // Length, area, distance units
    public static float unitScale = 0.025f;

    // PAN AND ZOOM RELATED PARAMETERS
    public Camera main_camera;
	public float finger_move_tolerance = 100f;
	private static float zoom_multiplier = 1.5f;
	public static float zoom_min = 200;
	public static float zoom_max = 850;
	public float pan_amount = 5f;
	public Vector2 panTouchStart;
    public Vector2 moveTouchStart;
    public bool previousTouchEnded;
    public GameObject curtouched_obj = null;
    public bool okayToPan = true;
	public bool panZoomLocked = false;
    public bool graphlocked;
    public bool directed_edge = false;

    public bool free_hand_edge = false;

    private Vector2 prev_move_pos;
    public Vector3 touchDelta;

    // Prefabs
    public GameObject IconicElement;
    public GameObject ImageIconicElement;
    public GameObject VectorElement;
    public GameObject VectorFieldElement;
    public GameObject CombineLineElement;
    public GameObject FunctionLineElement;
    public GameObject GraphElement;

    // Canvas buttons
    public GameObject staticElementButton;
    public GameObject iconicElementButton;
    public static GameObject pan_button;
    public GameObject vector_pen_button;
    public GameObject vector_field_pen_button;
    public GameObject eraser_button;
    public GameObject copy_button;
    public GameObject stroke_combine_button;
    public GameObject fused_rep_button;
    public GameObject function_brush_button;
    public GameObject AnalysisPen_button;
    public GameObject video_op_button;
    public GameObject history_view;
    public GameObject history_list_viewer;
    public GameObject canvas_radial;
    public GameObject color_picker;    
    public FlexibleColorPicker color_picker_script;

    public GameObject text_message_worldspace;

    public GameObject vectorline;
    public GameObject vectorfieldline;
    public GameObject setline;
    public GameObject function_menu;
    public GameObject edge_radial_menu;
    public GameObject node_radial_menu;
    public GameObject graph_radial_menu;   
    public GameObject analysis_radial_menu;

    // needed for drawing
    public GameObject templine;
	public static int totalLines = 0;
    static public int min_point_count = 30;

    // holder of all game objects
    public GameObject Objects_parent;

    // edge paint control
    // box collider control
    public static bool enablecollider = false;
    public int selected_obj_count = 0;
    public List<GameObject> selected_obj = new List<GameObject>();
    public GameObject edge_start, edge_end;

    // needed for vector field
    public int cell_width, cell_height;
    public int total_width, total_height;
    public int row_num, col_num;
    public Dictionary<int, GameObject> gridcells;

    // needed for graph
    public static int graph_count = 0;

    // needed for panning
    private bool taping_flag;
    int LastPhaseHappened; // 1 = S, 2 = M, 3 = E
    float TouchTime; // Time elapsed between touch beginning and ending
    float StartTouchTime; // Time.realtimeSinceStartup at start of touch
    float EndTouchTime; // Time.realtimeSinceStartup at end of touch
    float TraversedDistance; 
    public Vector2 startPos;

    // needed for dragging using mouse
    public GameObject pen_dragged_obj;
    public Vector3 drag_prev_pos;

    // needed for function
    public bool no_func_menu_open;
    public bool function_name_performed = true;
    public GameObject functionline;
    public static int function_count = 0;
    public static GameObject dragged_arg_textbox;
    public GameObject current_dragged_function;
    public GameObject potential_tapped_graph;    

    // needed for video
    public GameObject videoplayer;

    // needed for fusion
    public GameObject fused_obj;

    // needed for graph_analysis
    public bool no_analysis_menu_open;
    public static string dragged_icon_name;

    // objects history
    public List<GameObject> history = new List<GameObject>();
    public List<GameObject> new_drawn_icons = new List<GameObject>();
    public List<GameObject> new_drawn_vectors = new List<GameObject>();
    public List<GameObject> new_drawn_vectorfields = new List<GameObject>();
    public List<GameObject> new_drawn_function_lines = new List<GameObject>();

    // extra check for inputfields
    public static bool click_on_inputfield;

    // Action History
    public static bool ActionHistoryEnabled = false;

    public GameObject status_label_obj;

    // camera movement
    public float speed = 10.0f;

    // graph layout
    // dictionary with init values
    public static Dictionary<string, int> layout_dict = new Dictionary<string, int>()
    {
        ["manual"] = 0,
        ["circular"] = 1,
        ["random"] = 2,
        ["spring"] = 3,
        ["spectral"] = 4,
        ["fruchterman"] = 5
    };

    public static Dictionary<string, int> weight_dict = new Dictionary<string, int>()
    {
        ["auto"] = 0,
        ["custom"] = 1
    };

    // Start is called before the first frame update
    void Start()
	{
        /*iconicElementButton = GameObject.Find("IconicPen");        
        graph_pen_button = GameObject.Find("GraphPen");
        simplicial_pen_button = GameObject.Find("SimplicialPen");
        hyper_pen_button = GameObject.Find("HyperPen"); 
        eraser_button = GameObject.Find("Eraser");
        copy_button = GameObject.Find("Copy");
        stroke_combine_button = GameObject.Find("StrokeCombine");
        fused_rep_button = GameObject.Find("Fused");
        function_brush_button = GameObject.Find("FunctionPen");
        video_op_button = GameObject.Find("FileLoad");

        text_message_worldspace = GameObject.Find("text_message_worldspace");

        canvas_radial = GameObject.Find("canvas_radial");*/

        pan_button = GameObject.Find("Pan");

        no_func_menu_open = true;
        dragged_arg_textbox = null;
        potential_tapped_graph = null;

        gridcells= new Dictionary<int, GameObject>();

        cell_width = 50;
        cell_height = 50;
        total_width = (int)transform.GetComponent<Transform>().localScale.x;
        total_height = (int)transform.GetComponent<Transform>().localScale.y;
        row_num = (int)Mathf.Ceil(total_width / (float)cell_width);
        col_num = (int)Mathf.Ceil(total_height / (float)cell_height);
    }

    // Update is called once per frame
    void Update()
    {
        #region prevent unwanted touch on canvas
        if (EventSystem.current.IsPointerOverGameObject(0))
        {
            Debug.Log("detected_touch_over_UI");
            return;
        }
                

        /*if (AllButtonsBehaviors.isPointerOverStaticPen || AllButtonsBehaviors.isPointerOverIconicPen || 
            AllButtonsBehaviors.isPointerOverGraphPen || AllButtonsBehaviors.isPointerOverSimplicialPen
        || AllButtonsBehaviors.isPointerOverHyperPen || AllButtonsBehaviors.isPointerOverPan ||
        AllButtonsBehaviors.isPointerOverEraser || AllButtonsBehaviors.isPointerOverCopy || AllButtonsBehaviors.isPointerOverCombine
        || AllButtonsBehaviors.isPointerOverFuse || AllButtonsBehaviors.isPointerOverFunction || AllButtonsBehaviors.isPointerOverAnalysis
        || AllButtonsBehaviors.isPointerOverLoad)
        {
            Debug.Log("unwanted_touch_removed");
            return;
        }

        if (DropDownMenu.isPointerOverDropDown)
        {
            Debug.Log("unwanted_touch_on_dropdown_removed");
            return;
        }*/


        #endregion

        #region static element brush

        if (staticElementButton.GetComponent<AllButtonsBehaviors>().selected)
        //!iconicElementButton.GetComponent<AllButtonsBehaviors>().isPredictivePen)
        {

            //Debug.Log("entered");
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) &&
                   (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"
                   || Hit.collider.gameObject.tag == "static" || Hit.collider.gameObject.tag == "iconic"))
                {                    
                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); 
                    
                    templine = Instantiate(IconicElement, vec, Quaternion.identity, Objects_parent.transform);

                    templine.name = "static_" ;
                    templine.tag = "static";

                    templine.GetComponent<iconicElementScript>().points.Add(vec);

                    templine.transform.GetComponent<LineRenderer>().material.SetColor("_Color", color_picker_script.color);
                    templine.transform.GetComponent<TrailRenderer>().material.SetColor("_Color", color_picker_script.color);
                    //Debug.Log("colorpicker_color:" + color_picker_script.color.ToString());


                    templine.GetComponent<TrailRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

                    templine.GetComponent<LineRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;
                    new_drawn_icons.Add(templine);
                }
            }

            else if (templine != null &&
                PenTouchInfo.PressedNow //currentPen.tip.isPressed
                && (PenTouchInfo.penPosition -
                (Vector2)templine.GetComponent<iconicElementScript>().points[templine.GetComponent<iconicElementScript>().points.Count - 1]).magnitude > 0f)
            {
                // add points to the last line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"
                    || Hit.collider.gameObject.tag == "static" || Hit.collider.gameObject.tag == "iconic"))
                {

                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;

                    templine.GetComponent<TrailRenderer>().transform.position = vec;
                    templine.GetComponent<iconicElementScript>().points.Add(vec);

                    // pressure based pen width
                    templine.GetComponent<iconicElementScript>().updateLengthFromPoints();
                    templine.GetComponent<iconicElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                    templine.GetComponent<iconicElementScript>().reNormalizeCurveWidth();
                    templine.GetComponent<TrailRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;

                }
            }

            else if (templine != null && PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); //currentPen.position.ReadValue());// Input.GetTouch(0).position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"
                    || Hit.collider.gameObject.tag == "static" || Hit.collider.gameObject.tag == "iconic"))
                {
                    if (templine.GetComponent<iconicElementScript>().points.Count > min_point_count)
                    {

                        templine = transform.GetComponent<CreatePrimitives>().FinishStaticLine(templine);
                        templine = null;

                    }
                    else
                    {
                        Destroy(templine);
                        templine = null;
                    }
                }
                else
                {
                    Destroy(templine);
                    templine = null;
                }

            }

        }


        #endregion


        #region iconic element brush

        if (iconicElementButton.GetComponent<AllButtonsBehaviors>().selected)
        //!iconicElementButton.GetComponent<AllButtonsBehaviors>().isPredictivePen)
        {
            
            //Debug.Log("entered");
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) &&
                   (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {
                    //Debug.Log("instantiated_templine");

                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;
                                                                      //Debug.Log(vec);

                    totalLines++;
                    templine = Instantiate(IconicElement, vec, Quaternion.identity, Objects_parent.transform);
                    //templine.GetComponent<TrailRenderer>().material.color = Color.black;

                    templine.name = "iconic_" + totalLines.ToString();
                    templine.tag = "iconic";

                    templine.GetComponent<iconicElementScript>().points.Add(vec);
                    templine.GetComponent<iconicElementScript>().icon_number = totalLines;
                    templine.GetComponent<iconicElementScript>().icon_name = templine.name;

                    templine.transform.GetComponent<LineRenderer>().material.SetColor("_Color", color_picker_script.color);
                    templine.transform.GetComponent<TrailRenderer>().material.SetColor("_Color", color_picker_script.color);
                    //Debug.Log("colorpicker_color:" + color_picker_script.color.ToString());

                    
                    templine.GetComponent<TrailRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;

                    templine.GetComponent<LineRenderer>().widthMultiplier = 2;
                    //pencil_button.GetComponent<AllButtonsBehavior>().penWidthSliderInstance.GetComponent<Slider>().value;
                    new_drawn_icons.Add(templine);
                }
            }

            else if (templine != null &&
                PenTouchInfo.PressedNow //currentPen.tip.isPressed
                && (PenTouchInfo.penPosition -
                (Vector2)templine.GetComponent<iconicElementScript>().points[templine.GetComponent<iconicElementScript>().points.Count - 1]).magnitude > 0f)
            {
                // add points to the last line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {

                    Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;

                    templine.GetComponent<TrailRenderer>().transform.position = vec;
                    templine.GetComponent<iconicElementScript>().points.Add(vec);


                    // Show the distance, format to a fixed decimal place.
                    //templine.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();
                    /*templine.transform.GetChild(1).GetComponent<TextMeshPro>().text =
						templine.GetComponent<penLine_script>().attribute.Length.ToString("F1");*/

                    // pressure based pen width
                    templine.GetComponent<iconicElementScript>().updateLengthFromPoints();
                    templine.GetComponent<iconicElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                    templine.GetComponent<iconicElementScript>().reNormalizeCurveWidth();
                    templine.GetComponent<TrailRenderer>().widthCurve = templine.GetComponent<iconicElementScript>().widthcurve;

                }
            }

            else if (templine != null && PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); //currentPen.position.ReadValue());// Input.GetTouch(0).position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                {
                    if (templine.GetComponent<iconicElementScript>().points.Count > min_point_count)
                    {
                        
                        templine = transform.GetComponent<CreatePrimitives>().FinishPenLine(templine);
                        templine.GetComponent<iconicElementScript>().paintable_object = transform.gameObject;

                        //new_drawn_icons.Add(templine);
                        templine = null;

                    }
                    else
                    {
                        // delete the templine, not enough points
                        // Debug.Log("here_in_destroy");
                        Destroy(templine);
                        totalLines--;
                        templine = null;
                    }
                }
                else
                {
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(templine);
                    totalLines--;
                    templine = null;
                }

            }

        }


        #endregion

        #region pan

        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;      

        // Handle screen touches.                     
        //https://docs.unity3d.com/ScriptReference/TouchPhase.Moved.html

        if (pan_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            DragorMenuCreateOnClick();

            if (activeTouches.Count == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
            {

                // NO ANCHOR TAPPED, JUST ZOOM IN/PAN
                //main_camera.GetComponent<MobileTouchCamera>().enabled = true;

                UnityEngine.InputSystem.EnhancedTouch.Touch touchzero = activeTouches[0];
                UnityEngine.InputSystem.EnhancedTouch.Touch touchone = activeTouches[1];

                Vector2 touchzeroprevpos = touchzero.screenPosition - touchzero.delta;
                Vector2 touchoneprevpos = touchone.screenPosition - touchone.delta;

                float prevmag = (touchzeroprevpos - touchoneprevpos).magnitude;
                float currmag = (touchzero.screenPosition - touchone.screenPosition).magnitude;

                float difference = currmag - prevmag;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

                // CHECK AND DELETE INCOMPLETE LINES
                deleteTempLineIfDoubleFinger();

                // show zoom percentage text
                // Assuming these parameters for orthographic size: min: 200, max: 500
                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);

                text_message_worldspace.SetActive(true);
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";
                //}

            }
            /*else if (Input.touchCount == 2 && !panZoomLocked) // && pan_button.GetComponent<PanButtonBehavior>().selected)
            {
                //Debug.Log("double_finger_tap");
                // NO ANCHOR TAPPED, JUST ZOOM IN/PAN
                //main_camera.GetComponent<MobileTouchCamera>().enabled = true;

                UnityEngine.Touch touchzero = Input.GetTouch(0);
                UnityEngine.Touch touchone = Input.GetTouch(1);

                Vector2 touchzeroprevpos = touchzero.position - touchzero.deltaPosition;
                Vector2 touchoneprevpos = touchone.position - touchone.deltaPosition;

                float prevmag = (touchzeroprevpos - touchoneprevpos).magnitude;
                float currmag = (touchzero.position - touchone.position).magnitude;

                float difference = currmag - prevmag;

                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

                // CHECK AND DELETE INCOMPLETE LINES
                deleteTempLineIfDoubleFinger();

                int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);
                text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";


            }*/
            else if (activeTouches.Count == 1 && !panZoomLocked)
            {

                // Only pan when the touch is on top of the canvas. Otherwise,
                var ray = Camera.main.ScreenPointToRay(activeTouches[0].screenPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
                {

                    // EnhanchedTouch acts weirdly in the sense that the Start phase is not detected many times,
                    // only Moved, Stationary, and Ended phases are detected most of the times.
                    // So, introducing a bool that will only update the start pan position if a touch ended before.

                    if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended)
                    {
                        previousTouchEnded = true;
                    }

                    //if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    else if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved && previousTouchEnded && okayToPan)
                    {
                        panTouchStart = Camera.main.ScreenToWorldPoint(activeTouches[0].screenPosition);
                        //Debug.Log("touch start: " + panTouchStart.ToString());

                        previousTouchEnded = false;

                        if (canvas_radial.transform.childCount > 0)
                        {
                            for (int i = 0; i < canvas_radial.transform.childCount; i++)
                            {
                                Destroy(canvas_radial.transform.GetChild(i).gameObject);
                            }
                        }
                    }

                    //else if (touchScreen.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved)
                    else if (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved && !previousTouchEnded && okayToPan)
                    {
                        Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeTouches[0].screenPosition);
                        Camera.main.transform.position += (Vector3)panDirection;
                    }
                }

                // if not panning, check for tap
                if (activeTouches[0].isTap)
                {
                    // delegate single tap to short tap function
                    //Debug.Log("tap detected.");
                    menucreation(activeTouches[0].screenPosition);
                    // OnLongTap(activeTouches[0].screenPosition);
                }
            }
            /*else if (Input.touchCount == 1 && !panZoomLocked)
            {
                UnityEngine.Touch activeoldTouches = Input.GetTouch(0);

                // Only pan when the touch is on top of the canvas. Otherwise,
                var ray = Camera.main.ScreenPointToRay(activeoldTouches.position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "simplicial")
                {

                    GameObject temp = Hit.collider.gameObject;
                    Debug.Log("collided_with" + temp.tag);

                    if (activeoldTouches.phase == UnityEngine.TouchPhase.Ended && okayToPan)
                    {
                        previousTouchEnded = true;
                        if (curtouched_obj.tag == "iconic")
                        {
                            curtouched_obj.transform.localScale = curtouched_obj.transform.localScale / 1.05f;
                            //curtouched_obj.transform.localScale = new Vector3(1f, 1f, 1f);
                            if (!graphlocked)
                                curtouched_obj.GetComponent<iconicElementScript>().searchFunctionAndUpdateLasso();
                        }

                        else if (curtouched_obj.tag == "video_player")
                        {
                            curtouched_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().UIlayout();
                        }
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Began && okayToPan)
                    {
                        curtouched_obj = temp;

                        if (curtouched_obj.tag == "iconic")
                        {
                            curtouched_obj.transform.localScale = curtouched_obj.transform.localScale * 1.05f; //new Vector3(1.25f, 1.25f, 1.25f);
                        }

                        Vector3 vec = Hit.point;
                        // enforce the same z coordinate as the rest of the points in the parent set object
                        vec.z = -5f;
                        touchDelta = curtouched_obj.transform.position - vec;
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Moved && previousTouchEnded && okayToPan)//&& (curtouched_obj == temp))
                    {
                        panTouchStart = Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                        //Debug.Log("touch start: " + panTouchStart.ToString());

                        previousTouchEnded = false;
                        prev_move_pos = panTouchStart;
                    }

                    else if (activeoldTouches.phase == UnityEngine.TouchPhase.Moved && !previousTouchEnded && okayToPan)// && (curtouched_obj == temp))
                    {
                        if (Vector2.Distance(prev_move_pos, (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position)) > 2)
                        {
                            Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                            //Debug.Log("position changed from "+ Camera.main.transform.position.ToString() + " to " + (Camera.main.transform.position + (Vector3)panDirection).ToString());

                            Vector3 vec = Hit.point;
                            // enforce the same z coordinate as the rest of the points in the parent set object
                            vec.z = -5f;

                            Vector3 diff = vec - curtouched_obj.transform.position + touchDelta;
                            diff.z = 0;

                            //transform.position += diff;

                            if (curtouched_obj.tag == "paintable_canvas_object")
                            {
                                Camera.main.transform.position += (Vector3)panDirection;
                            }
                            else if (curtouched_obj.tag == "iconic")
                            {
                                //curtouched_obj.transform.position -= (Vector3)panDirection;
                                if (graphlocked)
                                {
                                    if (curtouched_obj.transform.parent.tag == "node_parent"
                                        && curtouched_obj.transform.parent.parent.GetComponent<GraphElementScript>().video_graph == false)
                                    {
                                        curtouched_obj.transform.parent.parent.position += diff;
                                        curtouched_obj.transform.parent.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);
                                    }
                                }
                                else if (curtouched_obj.GetComponent<iconicElementScript>().video_icon == false)
                                {
                                    curtouched_obj.transform.position += diff;
                                    curtouched_obj.GetComponent<iconicElementScript>().edge_position += diff;
                                    //curtouched_obj.GetComponent<iconicElementScript>().edge_position -= (Vector3)panDirection;
                                    curtouched_obj.GetComponent<iconicElementScript>().searchNodeAndUpdateEdge();
                                }

                            }
                            else if (curtouched_obj.tag == "video_player")
                            {
                                curtouched_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().checkHitAndMove(diff);
                            }
                            else if (curtouched_obj.tag == "hyper")
                            {
                                //curtouched_obj.transform.position -= (Vector3)panDirection;
                                curtouched_obj.transform.position += diff;
                                curtouched_obj.GetComponent<HyperElementScript>().UpdateChildren();
                            }

                            prev_move_pos = (Vector2)Camera.main.ScreenToWorldPoint(activeoldTouches.position);
                        }

                    }

                }

                OnShortTap();
            }*/
            else
            {
                previousTouchEnded = true;
                //main_camera.GetComponent<MobileTouchCamera>().enabled = false;
                text_message_worldspace.SetActive(false);
                //text_message_worldspace.GetComponent<TextMeshProUGUI>().text = "";
            }

        }

        #endregion

        #region Vector Pen
        if (vector_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            VectorCreation();
        }
        #endregion

        #region VectorField Pen
        if (vector_field_pen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            VectorFieldCreation();
        }
        #endregion

        // ERASER BRUSH
        #region eraser
        if (PenTouchInfo.PressedNow && eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {

            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);

            RaycastHit Hit;
            RaycastHit2D hit2d;

            if (Physics.Raycast(ray, out Hit))
            {
                // handle individually -- in case a type requires special treatment or extra code in the future
                // delete edges which has ties to erased nodes (sets, penlines, functions etc.)

                if (Hit.collider.gameObject.tag == "iconic")
                {                    
                    Destroy(Hit.collider.gameObject);
                }
                                
                else if (Hit.collider.gameObject.tag == "static")
                {                    
                    Destroy(Hit.collider.gameObject);
                }
            }

            hit2d = Physics2D.GetRayIntersection(ray);
            if (hit2d.collider != null && hit2d.collider.gameObject.tag == "vector")
            {
                Transform temp = hit2d.collider.gameObject.transform.parent;
                Destroy(hit2d.collider.gameObject);
                /*if (temp.tag == "edge_parent")
                    StartCoroutine(ClearGraphData("edge", temp.gameObject));*/

            }
        }
        #endregion

        #region stroke combine brush

        if (stroke_combine_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            //Debug.Log("entered");
            if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
            {
                // start drawing a new line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
                {
                    //Debug.Log("instantiated_templine");

                    Vector3 vec = Hit.point + new Vector3(0, 0, -5);
                    setline = Instantiate(CombineLineElement, vec, Quaternion.identity, Objects_parent.transform);

                    setline.name = "temp_set_line";
                    setline.GetComponent<iconicElementScript>().points.Add(vec);

                }
            }

            else if (setline != null &&
                PenTouchInfo.PressedNow //currentPen.tip.isPressed
                && (PenTouchInfo.penPosition -
                (Vector2)setline.GetComponent<iconicElementScript>().points[setline.GetComponent<iconicElementScript>().points.Count - 1]).magnitude > 0f)
            {
                // add points to the last line
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
                {

                    Vector3 vec = Hit.point + new Vector3(0, 0, -5); // Vector3.up * 0.1f;

                    setline.GetComponent<TrailRenderer>().transform.position = vec;
                    setline.GetComponent<iconicElementScript>().points.Add(vec);
                    setline.GetComponent<iconicElementScript>().calculateLengthAttributeFromPoints();

                    // pressure based pen width
                    setline.GetComponent<iconicElementScript>().updateLengthFromPoints();
                    setline.GetComponent<iconicElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                    setline.GetComponent<iconicElementScript>().reNormalizeCurveWidth();
                    setline.GetComponent<TrailRenderer>().widthCurve = setline.GetComponent<iconicElementScript>().widthcurve;

                }
            }

            else if (setline != null && PenTouchInfo.ReleasedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); //currentPen.position.ReadValue());// Input.GetTouch(0).position);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.name == "Paintable")
                {
                    if (setline.GetComponent<iconicElementScript>().points.Count > min_point_count)
                    {

                        List<GameObject> icon_meshobjs = new List<GameObject>();
                        GameObject[] iconarray = GameObject.FindGameObjectsWithTag("iconic");

                        for (int i = 0; i < iconarray.Length; i++)
                        {

                            // check if the lines are inside the drawn set polygon -- in respective local coordinates
                            if (setline.GetComponent<iconicElementScript>().isInsidePolygon(
                                //setline.GetComponent<iconicElementScript>().transform.InverseTransformPoint(
                                iconarray[i].GetComponent<iconicElementScript>().edge_position)
                                )//)
                            {
                                icon_meshobjs.Add(iconarray[i].transform.gameObject);
                                //penLines[i].transform.SetParent(templine.transform);
                                //Debug.Log("iconic found");

                            }
                        }

                        GameObject[] selected_icons = new GameObject[icon_meshobjs.Count];
                        int temp_iter = 0;
                        foreach (var p in icon_meshobjs)
                        {
                            selected_icons[temp_iter] = p;
                            temp_iter += 1;
                        }

                        Destroy(setline);
                        transform.GetComponent<CreatePrimitives>().CreatePenLine(selected_icons);
                        setline = null;
                    }
                    else
                    {
                        // delete the templine, not enough points
                        // Debug.Log("here_in_destroy");
                        Destroy(setline);
                        setline = null;
                    }
                }
                else
                {
                    // the touch didn't end on a line, destroy the line
                    // Debug.Log("here_in_destroy_different_Hit");
                    Destroy(setline);
                    setline = null;
                }

            }

        }

        #endregion

        #region fused representation

        if (fused_rep_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.PressedThisFrame)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    fused_obj = Hit.collider.gameObject;
                    StartTouchTime = Time.realtimeSinceStartup;
                    fused_obj.transform.localScale = fused_obj.transform.localScale * 1.05f;
                }
            }            

            else if (fused_obj != null && PenTouchInfo.ReleasedThisFrame)
            {
                fused_obj.transform.localScale = fused_obj.transform.localScale / 1.05f;

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition); 
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject == fused_obj)
                {
                    EndTouchTime = Time.realtimeSinceStartup;
                    TouchTime = EndTouchTime - StartTouchTime;

                    if (TouchTime > 0.5f)
                        ConvertToFunction(fused_obj);
                }

                fused_obj = null;
            }

        }
        #endregion

        #region function brush

        if (function_brush_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            //Debug.Log("entered");
            //if (no_func_menu_open == false) return;
            if (dragged_arg_textbox == null)
            {
                
                // normal operation                
                if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
                {
                    // start drawing a new line
                    var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                    RaycastHit Hit;
                    //Debug.Log("here");
                    if (Physics.Raycast(ray, out Hit) &&
                        (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                    {
                        Debug.Log("instantiated_templine");

                        Vector3 vec = Hit.point + new Vector3(0, 0, -5f);
                        functionline = Instantiate(FunctionLineElement, vec, Quaternion.identity, Objects_parent.transform);

                        functionline.name = "function_line_" + function_count.ToString();
                        function_count++;
                        functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                        functionline.GetComponent<FunctionElementScript>().paintable_object = transform.gameObject;

                        new_drawn_function_lines.Add(functionline);
                        //function_menu.GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text = "Brush loaded";
                    }
                }

                else if (functionline != null &&
                    PenTouchInfo.PressedNow //currentPen.tip.isPressed
                    && (PenTouchInfo.penPosition -
                    (Vector2)functionline.GetComponent<FunctionElementScript>().points[functionline.GetComponent<FunctionElementScript>().points.Count - 1]).magnitude > 0f)
                {
                    // add points to the last line
                    var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                    RaycastHit Hit;
                    if (Physics.Raycast(ray, out Hit) &&
                        (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                    {

                        Vector3 vec = Hit.point + new Vector3(0, 0, -5f); // Vector3.up * 0.1f;

                        functionline.GetComponent<TrailRenderer>().transform.position = vec;
                        functionline.GetComponent<FunctionElementScript>().AddPoint(vec);
                        //functionline.GetComponent<FunctionElementScript>().calculateLengthAttributeFromPoints();

                        // pressure based pen width
                        functionline.GetComponent<FunctionElementScript>().updateLengthFromPoints();
                        functionline.GetComponent<FunctionElementScript>().addPressureValue(PenTouchInfo.pressureValue);
                        functionline.GetComponent<FunctionElementScript>().reNormalizeCurveWidth();
                        functionline.GetComponent<TrailRenderer>().widthCurve = functionline.GetComponent<FunctionElementScript>().widthcurve;

                    }
                }

                else if (functionline != null && PenTouchInfo.ReleasedThisFrame)
                {
                    var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                    RaycastHit Hit;
                    
                    if (Physics.Raycast(ray, out Hit) &&
                    (Hit.collider.gameObject.name == "Paintable" || Hit.collider.gameObject.tag == "video_player"))
                    {
                        if (functionline.GetComponent<FunctionElementScript>().points.Count > min_point_count)
                        {
                            int maxx = -100000, maxy = -100000, minx = 100000, miny = 100000;
                            List<GameObject> selected_vectors = new List<GameObject>();
                            GameObject[] vectorarray = GameObject.FindGameObjectsWithTag("vector");

                            functionline.GetComponent<FunctionElementScript>().vectorField = new VectorField();
                            functionline.GetComponent<FunctionElementScript>().vectorField.vectors = new List<Vector>();

                            for (int i = 0; i < vectorarray.Length; i++)
                            {
                                if (functionline.GetComponent<FunctionElementScript>().isInsidePolygon(
                                        vectorarray[i].GetComponent<LineRenderer>().GetPosition(0)))//)
                                {
                                    selected_vectors.Add(vectorarray[i]);
                                    int x = vectorarray[i].GetComponent<VectorElementScript>().x;
                                    int y = vectorarray[i].GetComponent<VectorElementScript>().y;

                                    Vector vector = new Vector {
                                        x = x,
                                        y = y,
                                        f_x = vectorarray[i].GetComponent<VectorElementScript>().f_x,
                                        f_y = vectorarray[i].GetComponent<VectorElementScript>().f_y
                                    };
                                    functionline.GetComponent<FunctionElementScript>().vectorField.vectors.Add(vector);

                                    if (maxx < x) maxx = x;
                                    if (minx > x) minx = x;

                                    if (maxy < y) maxy = y;                                    
                                    if (miny > y) miny = y;

                                }
                            }

                            //Debug.Log("found graph count in lasso: " + selected_graphs.Count.ToString());
                            if (selected_vectors.Count == 0)
                            {
                                Destroy(functionline);
                                functionline = null;
                                function_count--;
                                return;
                            }

                            var hullAPI = new HullAPI();
                            var hull = hullAPI.Hull2D(new Hull2DParameters()
                            { Points = functionline.GetComponent<FunctionElementScript>().points.ToArray(), Concavity = 3000 });

                            Vector3[] vertices = hull.vertices;

                            functionline.GetComponent<FunctionElementScript>().points = vertices.ToList();
                            functionline = transform.GetComponent<CreatePrimitives>().FinishFunctionLine(functionline);

                            functionline.GetComponent<FunctionElementScript>().gridmaxx = maxx;
                            functionline.GetComponent<FunctionElementScript>().gridminx = minx;
                            functionline.GetComponent<FunctionElementScript>().gridmaxy = maxy;
                            functionline.GetComponent<FunctionElementScript>().gridminy = miny;

                            functionline.GetComponent<FunctionElementScript>().vectorField.gridmaxx = maxx;
                            functionline.GetComponent<FunctionElementScript>().vectorField.gridminx = minx;
                            functionline.GetComponent<FunctionElementScript>().vectorField.gridmaxy = maxy;
                            functionline.GetComponent<FunctionElementScript>().vectorField.gridminy = miny;

                            functionline.GetComponent<FunctionElementScript>().InstantiateNameBox();
                            functionline.GetComponent<FunctionElementScript>().InstantiateMatrix();
                            functionline.GetComponent<FunctionElementScript>().selected_vectors = selected_vectors;

                            //File.WriteAllText("Assets/Resources/" + "data.json", JsonUtility.ToJson(functionline.GetComponent<FunctionElementScript>().vectorField));

                            //StartCoroutine(HistoryModify(functionline));
                            //functionline.GetComponent<FunctionElementScript>().grapharray = selected_graphs.ToArray();
                            functionline = null;
                            //selected_vectors.Clear();
                        }
                        else
                        {
                            // delete the templine, not enough points
                            // Debug.Log("here_in_destroy");
                            Destroy(functionline);
                            functionline = null;
                            function_count--;
                        }
                    }
                    else
                    {
                        // the touch didn't end on a line, destroy the line
                        // Debug.Log("here_in_destroy_different_Hit");
                        Destroy(functionline);
                        functionline = null;
                        function_count--;
                    }
                    
                }                     
            }            
        }

        #endregion

        #region analysis
        if (AnalysisPen_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.PressedThisFrame)
            {
                Debug.Log("pressed");

                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;

                if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                {
                    dragged_arg_textbox = Hit.collider.gameObject;
                    Debug.Log("collided_with" + dragged_arg_textbox.tag);                    
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame)
                StartCoroutine(clearclickedobj());
        }
        #endregion

        // HANDLE ANY RELEVANT KEY INPUT FOR PAINTABLE'S OPERATIONS
        //handleKeyInteractions();

        /*if (!function_brush_button.GetComponent<AllButtonsBehaviors>().selected &&
            !AnalysisPen_button.GetComponent<AllButtonsBehaviors>().selected)
        {*/
            StartCoroutine(HandleKeyboardInput());
        //}        
    }
        
    public void ConvertToFunction(GameObject fused_obj)
    {
        GameObject fused_function_lasso = Instantiate(FunctionLineElement, fused_obj.transform.position, Quaternion.identity, Objects_parent.transform);
        fused_function_lasso.name = "function_line_" + function_count.ToString();
        function_count++;

        StartCoroutine(FusedRepresentation(fused_obj, fused_function_lasso));

        
    }

    IEnumerator FusedRepresentation(GameObject fused_obj, GameObject fused_function_lasso)
    {
        if (fused_obj.GetComponent<MeshFilter>() != null)
        {
            var meshFilter = fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshFilter>();

            fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>().sharedMaterial = fused_obj.transform.GetComponent<MeshRenderer>().sharedMaterial;

            Mesh mesh = fused_obj.GetComponent<MeshFilter>().sharedMesh;
            meshFilter.sharedMesh = mesh;
            fused_function_lasso.GetComponent<FunctionElementScript>().edge_position = fused_obj.GetComponent<MeshFilter>().sharedMesh.bounds.center;
        }
        else
        {
            Destroy(fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshFilter>());
            Destroy(fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.GetComponent<MeshRenderer>());

            yield return null;

            var sr = fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.AddComponent<SpriteRenderer>();
            sr.sprite = fused_obj.GetComponent<iconicElementScript>().recognized_sprite;
            fused_function_lasso.GetComponent<FunctionElementScript>().mesh_holder.transform.localScale = new Vector3(30f, 30f, 1f);

            fused_obj.GetComponent<iconicElementScript>().getImagepts();
            fused_function_lasso.GetComponent<FunctionElementScript>().edge_position = fused_obj.transform.GetComponent<SpriteRenderer>().bounds.extents;
            fused_function_lasso.GetComponent<FunctionElementScript>().edge_position.z = -5f;
        }


        fused_function_lasso.GetComponent<TrailRenderer>().enabled = false;
        fused_function_lasso.GetComponent<LineRenderer>().enabled = false;

        fused_function_lasso.GetComponent<FunctionElementScript>().points = fused_obj.GetComponent<iconicElementScript>().points;

        fused_function_lasso.GetComponent<FunctionElementScript>().maxx = fused_obj.GetComponent<iconicElementScript>().maxx;
        fused_function_lasso.GetComponent<FunctionElementScript>().maxy = fused_obj.GetComponent<iconicElementScript>().maxy;
        fused_function_lasso.GetComponent<FunctionElementScript>().minx = fused_obj.GetComponent<iconicElementScript>().minx;
        fused_function_lasso.GetComponent<FunctionElementScript>().miny = fused_obj.GetComponent<iconicElementScript>().miny;

        fused_function_lasso.GetComponent<FunctionElementScript>().InstantiateNameBox();
        fused_function_lasso.GetComponent<FunctionElementScript>().fused_function = true;

        //StartCoroutine(HistoryModify(fused_function_lasso));
        yield return null;
        Destroy(fused_obj);
    }

    public void DragorMenuCreateOnClick()
    {
        if (PenTouchInfo.PressedThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag != "simplicial")
            {
                pen_dragged_obj = Hit.collider.gameObject;
                Debug.Log("collided_with" + pen_dragged_obj.tag);

                if (pen_dragged_obj.tag == "paintable_canvas_object")
                {
                    if (canvas_radial.transform.childCount > 0)
                    {
                        for (int i = 0; i < canvas_radial.transform.childCount; i++)
                        {
                            Destroy(canvas_radial.transform.GetChild(i).gameObject);
                        }
                    }
                }

                if (pen_dragged_obj.tag == "iconic")
                {
                    pen_dragged_obj.transform.localScale = pen_dragged_obj.transform.localScale * 1.05f;

                    if (graphlocked)
                    {
                        if (pen_dragged_obj.transform.parent.tag == "node_parent")
                            drag_prev_pos = pen_dragged_obj.transform.parent.parent.position;
                        else
                        {
                            pen_dragged_obj = null;
                            return;
                        }
                            
                    }
                        
                    else drag_prev_pos = pen_dragged_obj.transform.position;
                }

                Vector3 vec = Hit.point;
                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                touchDelta = pen_dragged_obj.transform.position - vec;                

                panTouchStart = Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
                prev_move_pos = panTouchStart;
            }
        }

        else if (PenTouchInfo.PressedNow && pen_dragged_obj != null)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Vector2.Distance(prev_move_pos, (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition)) < 2)
                return;

            if (Physics.Raycast(ray, out Hit))
            {
                Vector2 panDirection = panTouchStart - (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
                Vector3 vec = Hit.point;

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - pen_dragged_obj.transform.position + touchDelta;
                diff.z = 0;

                
                if (pen_dragged_obj.tag == "paintable_canvas_object")
                {
                    //Camera.main.transform.position += (Vector3)panDirection;
                }
                else if (pen_dragged_obj.tag == "iconic")
                {
                    
                    if (graphlocked)
                    {
                        if (pen_dragged_obj.transform.parent.tag == "node_parent"
                            && pen_dragged_obj.transform.parent.parent.GetComponent<GraphElementScript>().video_graph==false)
                        {
                            
                            if (!pen_dragged_obj.transform.parent.parent.GetComponent<GraphElementScript>().video_graph)
                            {
                                pen_dragged_obj.transform.parent.parent.position += diff;
                                pen_dragged_obj.transform.parent.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);
                            }                                
                        }
                    }
                    else if(pen_dragged_obj.GetComponent<iconicElementScript>().video_icon == false)
                    {
                        pen_dragged_obj.transform.position += diff;
                        pen_dragged_obj.GetComponent<iconicElementScript>().edge_position += diff;
                        pen_dragged_obj.GetComponent<iconicElementScript>().searchNodeAndUpdateEdge();
                    }

                }
                else if (pen_dragged_obj.tag == "video_player")
                {
                    pen_dragged_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().checkHitAndMove(diff);
                }
                

                prev_move_pos = (Vector2)Camera.main.ScreenToWorldPoint(PenTouchInfo.penPosition);
            }
        }

        else if (PenTouchInfo.ReleasedThisFrame && pen_dragged_obj != null)
        {            
            if (pen_dragged_obj.tag == "iconic")
            {
                if (canvas_radial.transform.childCount > 0)
                {
                    for (int i = 0; i < canvas_radial.transform.childCount; i++)
                    {
                        Destroy(canvas_radial.transform.GetChild(i).gameObject);
                    }
                }

                pen_dragged_obj.transform.localScale = pen_dragged_obj.transform.localScale / 1.05f;
                if (!graphlocked)
                    pen_dragged_obj.GetComponent<iconicElementScript>().searchFunctionAndUpdateLasso();

                if (graphlocked)
                {
                    if (pen_dragged_obj.transform.parent.tag == "node_parent")
                    {

                        if (Vector3.Distance(drag_prev_pos, pen_dragged_obj.transform.parent.parent.position) < 5f)
                        {
                            menucreation(PenTouchInfo.penPosition);
                        }
                    }
                }
                else
                {                    
                    if (Vector3.Distance(drag_prev_pos, pen_dragged_obj.transform.position) < 5f)
                    {
                        menucreation(PenTouchInfo.penPosition);
                    }
                }
            }

            else if (pen_dragged_obj.tag == "video_player")
            {
                pen_dragged_obj.transform.parent.GetComponent<VideoPlayerChildrenAccess>().UIlayout();
            }

            pen_dragged_obj = null;
        }
                
    }

    // create iconic element from an image
    public GameObject createImageIcon(string FilePath, int track = -1)
    {
        GameObject temp = Instantiate(ImageIconicElement, new Vector3(0,0,-40f), Quaternion.identity, Objects_parent.transform);
        temp.tag = "iconic";

        if (track != -1)
        {
            temp.name = "iconic_" + track.ToString();
            temp.GetComponent<iconicElementScript>().icon_number = track;
            temp.GetComponent<iconicElementScript>().icon_name = "person_" + track.ToString();
        }
        else
        {
            totalLines++;
            temp.name = "iconic_" + totalLines.ToString();
            temp.GetComponent<iconicElementScript>().icon_number = totalLines;
            temp.GetComponent<iconicElementScript>().icon_name = "iconic_" + totalLines.ToString();
        }
        
        temp.GetComponent<iconicElementScript>().image_icon = true;
        temp.GetComponent<iconicElementScript>().LoadNewSprite(FilePath);
        return temp;
    }
    
    
    //https://stackoverflow.com/questions/38728714/unity3d-how-to-detect-taps-on-android
    void OnShortTap()
    {
        UnityEngine.Touch currentTouch = Input.GetTouch(0);
        if (canvas_radial.transform.childCount > 0)
        {
            for (int i = 0; i < canvas_radial.transform.childCount; i++)
            {
                Destroy(canvas_radial.transform.GetChild(i).gameObject);
            }
        }
        switch (currentTouch.phase)
        {
            case UnityEngine.TouchPhase.Began:
                if (LastPhaseHappened != 1)
                {
                    StartTouchTime = Time.realtimeSinceStartup;
                    taping_flag = true;
                }
                LastPhaseHappened = 1;
                startPos = currentTouch.position;
                break;

            case UnityEngine.TouchPhase.Moved:
                //if (LastPhaseHappend != 2)
                if (Vector2.Distance(currentTouch.position, startPos) > 2)
                {
                    taping_flag = false;
                }
                LastPhaseHappened = 2;
                break;

            case UnityEngine.TouchPhase.Ended:
                if (LastPhaseHappened != 3)
                {
                    EndTouchTime = Time.realtimeSinceStartup;
                    TouchTime = EndTouchTime - StartTouchTime;
                }
                LastPhaseHappened = 3;

                if (taping_flag && TouchTime > 0.5f)
                // TouchTime for a tap can be further defined
                {
                    //Tap has happened;
                    Debug.Log("tap_detected for duration: " + TouchTime.ToString());
                    if (TouchTime > 1f)
                        graphlocked = true;
                    else
                        graphlocked = false;

                    menucreation(currentTouch.position);
                }
                break;
        }
        
    }

    void menucreation(Vector2 menu_position)
    {
        bool edge_menu_create = false;

        if (canvas_radial.transform.childCount > 0)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(menu_position);

        RaycastHit Hit;
        RaycastHit2D hit2d;

        hit2d = Physics2D.GetRayIntersection(ray);
        if (!graphlocked && hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
        {
            Debug.Log("hit:" + hit2d.collider.gameObject.tag);
            Vector3 vec_radius_offset = new Vector3(0f, 10f, 0f);
            GameObject radmenu = Instantiate(edge_radial_menu, Vector3.zero, Quaternion.identity, canvas_radial.transform);

            radmenu.GetComponent<EdgeMenuScript>().menu_parent = hit2d.collider.gameObject;
            edge_menu_create = true;

            Vector3 temp_pos = hit2d.collider.gameObject.GetComponent<EdgeCollider2D>().bounds.center - vec_radius_offset;
            Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos);

            Vector2 anchored_pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_radial.transform.GetComponent<RectTransform>(), screen_temp_pos,
                                        null, out anchored_pos);

            radmenu.GetComponent<RectTransform>().anchoredPosition = anchored_pos;
        }

        if (edge_menu_create) return;

        if (Physics.Raycast(ray, out Hit))
        {
            Debug.Log("hit in menu creation: " + Hit.collider.gameObject.tag);

            if (Hit.collider.gameObject.tag == "iconic")
            {
                if(graphlocked)
                {
                    Debug.Log("graph_menu_created");
                    Transform node_parent = Hit.collider.gameObject.transform.parent;
                    if (node_parent.tag == "node_parent")
                    {
                        node_parent.parent.GetComponent<GraphElementScript>().createMenu(canvas_radial);
                        edge_menu_create = false;
                    }                    
                }
                else
                {
                    Debug.Log("node_menu_created");

                    var radius_offset = Hit.collider.gameObject.GetComponent<iconicElementScript>().radius;
                    Vector3 vec_radius_offset = new Vector3(0f, 1.25f * radius_offset, 0f);
                    GameObject radmenu = Instantiate(node_radial_menu, Vector3.zero,
                            Quaternion.identity, canvas_radial.transform);

                    radmenu.GetComponent<NodeMenuScript>().menu_parent = Hit.collider.gameObject;
                    Hit.collider.gameObject.GetComponent<iconicElementScript>().menu_open = true;
                    edge_menu_create = false;

                    Vector3 temp_pos = Hit.collider.gameObject.GetComponent<iconicElementScript>().edge_position - vec_radius_offset;
                    Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos);

                    Vector2 anchored_pos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas_radial.transform.GetComponent<RectTransform>(), screen_temp_pos,
                                                null, out anchored_pos);

                    radmenu.GetComponent<RectTransform>().anchoredPosition = anchored_pos;
                }
            }

        }

        

               
    }

    void VectorFieldCreation()
    {
        if (PenTouchInfo.PressedThisFrame)
        {
            vectorfieldline = Instantiate(VectorFieldElement, Vector3.zero, 
                Quaternion.identity, Objects_parent.transform);

            new_drawn_vectorfields.Add(vectorfieldline);

            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
            {
                Vector3 vec = Hit.point + new Vector3(0, 0, -40);
                vectorfieldline.GetComponent<VectorFieldElement>().points.Add(vec);
                vectorfieldline.GetComponent<VectorFieldElement>().last_drawn_pos = vec;
                
            }
        }

        else if (vectorfieldline != null && PenTouchInfo.PressedNow)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
            {
                Vector3 vec = Hit.point + new Vector3(0, 0, -40);

                float dist = Vector3.Distance(vec, vectorfieldline.GetComponent<VectorFieldElement>().last_drawn_pos);

                if (dist > (vectorfieldline.GetComponent<VectorFieldElement>().magnitude + vectorfieldline.GetComponent<VectorFieldElement>().offset))
                {
                    vectorfieldline.GetComponent<VectorFieldElement>().last_drawn_pos = vec;
                    vectorfieldline.GetComponent<VectorFieldElement>().drawn = false;
                }
                else if (!vectorfieldline.GetComponent<VectorFieldElement>().drawn &&
                    dist > vectorfieldline.GetComponent<VectorFieldElement>().magnitude)
                {
                    Vector3 temp_vec = vectorfieldline.GetComponent<VectorFieldElement>().last_drawn_pos;
                    single_cell temp_cell = new single_cell();
                    temp_cell.x = (int)Mathf.Ceil((temp_vec.x + (total_width / 2)) / (float)cell_width);
                    temp_cell.y = (int)Mathf.Ceil((temp_vec.y + (total_height / 2)) / (float)cell_height);

                    // similar to 2d to 1d array conversion, we are saving a pointer with the vector
                    int id = (Mathf.Max(temp_cell.x - 1, 0) * row_num) + temp_cell.y;

                    /*Debug.Log("counter: " + id.ToString());
                    Debug.Log("counter: " + temp_cell.x.ToString() + " " + temp_cell.y.ToString());*/

                    //create a new vector
                    vectorline = VectorCreation(vectorfieldline.GetComponent<VectorFieldElement>().last_drawn_pos, vec, vectorfieldline);
                    vectorfieldline.GetComponent<VectorFieldElement>().drawn = true;
                    vectorline.GetComponent<VectorElementScript>().x = temp_cell.x;
                    vectorline.GetComponent<VectorElementScript>().y = temp_cell.y;

                    if (gridcells.ContainsKey(id))
                    {
                        Debug.Log("caught_ya");
                        Destroy(gridcells[id]);

                        gridcells[id] = vectorline;
                        //return;
                    }
                    else
                    {    
                        gridcells.Add(id, vectorline);
                    }

                    // set everything back to null
                    vectorline = null;
                }

                vectorfieldline.GetComponent<VectorFieldElement>().points.Add(vec);
            }
        }

        else if (PenTouchInfo.ReleasedThisFrame)
        {
            vectorfieldline = null;
        }
    }

    void VectorCreation()
    {
        if (PenTouchInfo.PressedThisFrame)//currentPen.tip.wasPressedThisFrame)
        {
            // start drawing a new line
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
            {
                Vector3 vec = Hit.point + new Vector3(0, 0, -40); // Vector3.up * 0.1f;

                vectorline = Instantiate(VectorElement, vec, Quaternion.identity, Objects_parent.transform);
                vectorline.name = "vector_" + selected_obj_count.ToString();
                vectorline.tag = "vector";

                selected_obj_count++;

                vectorline.GetComponent<VectorElementScript>().points.Add(vec);
                //CreateEmptyEdgeObjects();

                //https://generalistprogrammer.com/unity/unity-line-renderer-tutorial/
                LineRenderer l = vectorline.transform.GetComponent<LineRenderer>();
                l.material.SetColor("_Color", color_picker_script.color);

                new_drawn_vectors.Add(vectorline);
                /*l.startWidth = 2f;
                l.endWidth = 2f;*/

                // set up the line renderer                    
                l.positionCount = 2;
                l.SetPosition(0, vec);// + new Vector3(0, 0, -2f));
                l.SetPosition(1, vec + new Vector3(1f, 0, -2f));
            }

        }

        else if (PenTouchInfo.PressedNow)
        {
            // add points to the last line, but check that if an edge line has been created already
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (vectorline != null && Physics.Raycast(ray, out Hit) &&
                (Hit.collider.gameObject.tag == "paintable_canvas_object"))
            {
                Vector3 vec = Hit.point + new Vector3(0, 0, -5f); // Vector3.up * 0.1f;   
                vectorline.GetComponent<VectorElementScript>().points.Add(vec);
                vectorline.GetComponent<LineRenderer>().SetPosition(1, vec);// + new Vector3(0, 0, -2f));                    
            }
        }

        else if (PenTouchInfo.ReleasedThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (vectorline != null && Physics.Raycast(ray, out Hit) && (Hit.collider.gameObject.tag == "paintable_canvas_object"))
            {
                if (vectorline.GetComponent<VectorElementScript>().points.Count > 2)
                {
                    Vector3 vec = Hit.point + new Vector3(0, 0, -40);
                    LineRenderer l = vectorline.transform.GetComponent<LineRenderer>();
                    l.SetPosition(1, vec);

                    vectorline.GetComponent<VectorElementScript>().directed_edge = true;// directed_edge;

                    // set line renderer end point
                    vectorline.GetComponent<VectorElementScript>().addEndPoint();

                    //GraphCreation();

                    // set everything back to null
                    vectorline = null;
                }
                else
                {
                    // delete the templine, not enough points
                    Destroy(vectorline);
                    vectorline = null;
                    selected_obj_count--;
                }

            }

            // in all other cases, to be safe, just delete the entire edgeline structure
            else
            {
                Destroy(vectorline);
                vectorline = null;
                selected_obj_count--;
            }

            // the touch has ended, destroy all temp edge cylinders now
            // DeleteEmptyEdgeObjects();
        }

    }

    GameObject VectorCreation(Vector3 start, Vector3 end, GameObject vector_field)
    {                        
        vectorline = Instantiate(VectorElement, start, Quaternion.identity, vector_field.transform);
        vectorline.name = "vector_" + selected_obj_count.ToString();
        vectorline.tag = "vector";

        selected_obj_count++;

        vectorline.GetComponent<VectorElementScript>().points.Add(start);
        //CreateEmptyEdgeObjects();

        //https://generalistprogrammer.com/unity/unity-line-renderer-tutorial/
        LineRenderer l = vectorline.transform.GetComponent<LineRenderer>();
        l.material.SetColor("_Color", color_picker_script.color);

        new_drawn_vectors.Add(vectorline);
        /*l.startWidth = 2f;
        l.endWidth = 2f;*/

        // set up the line renderer                    
        l.positionCount = 2;
        l.SetPosition(0, start);
        l.SetPosition(1, end);

        vectorline.GetComponent<VectorElementScript>().directed_edge = true;// directed_edge;
        // set line renderer end point
        vectorline.GetComponent<VectorElementScript>().addEndPoint();

        return vectorline;
        // set everything back to null
        vectorline = null;               
    }

    // normal simple graph
    void GraphCreation()
    {
        // if they are already under the same graph, no need to create a new one. Just assign the new edgeline to the previous parent
        if (edge_start.transform.parent == edge_end.transform.parent && edge_start.transform.parent.tag == "node_parent")
        {
            Transform Prev_graph_parent = edge_start.transform.parent.transform.parent;
            Prev_graph_parent.GetComponent<GraphElementScript>().abstraction_layer = "graph";
            Prev_graph_parent.GetChild(1).gameObject.SetActive(true);
            vectorline.transform.parent = Prev_graph_parent.GetChild(1);
            Prev_graph_parent.GetComponent<GraphElementScript>().edges_init();
            //Prev_graph_parent.GetComponent<GraphElementScript>().edges_as_Str();
            return;
        }

        graph_count++;
        GameObject tempgraph = Instantiate(GraphElement);
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "graph";
        tempgraph.GetComponent<GraphElementScript>().paintable = transform.gameObject;
        tempgraph.name = "graph_"+graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.transform.parent = Objects_parent.transform;
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + graph_count.ToString();

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        /*new GameObject("node_parent_" + graph_count.ToString());
        tempnodeparent.tag = "node_parent";
        tempnodeparent.transform.parent = tempgraph.transform;
        tempnodeparent.transform.SetSiblingIndex(0);*/

        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;
        /*new GameObject("edge_parent_" + graph_count.ToString());
        tempedgeparent.tag = "edge_parent";
        tempedgeparent.transform.parent = tempgraph.transform;
        tempedgeparent.transform.SetSiblingIndex(1);*/

        GameObject tempsimplicialparent = tempgraph.transform.GetChild(2).gameObject;
        /*new GameObject("simplicial_parent_" + graph_count.ToString());
        tempsimplicialparent.tag = "simplicial_parent";
        tempsimplicialparent.transform.parent = tempgraph.transform;
        tempsimplicialparent.transform.SetSiblingIndex(2);*/

        GameObject temphyperparent = tempgraph.transform.GetChild(3).gameObject;
        /*new GameObject("hyper_parent_" + graph_count.ToString());
        temphyperparent.tag = "hyper_parent";
        temphyperparent.transform.parent = tempgraph.transform;
        temphyperparent.transform.SetSiblingIndex(3);*/

        //assign_the_newly_created_Edge_to_temp_edge_parent_object
        vectorline.transform.parent = tempedgeparent.transform;

        List<GameObject> temp_edge_obj = new List<GameObject>();
        temp_edge_obj.Add(edge_start);
        temp_edge_obj.Add(edge_end);

        //change_parent
        foreach (GameObject each_node in temp_edge_obj)
        {
            //change_parent 
            // if already in a graph, change parent of every siblings of it,but make sure not under the current graph
            if (each_node.transform.parent.tag == "node_parent" && each_node.transform.parent != tempnodeparent.transform)
            {
                Transform Prev_node_parent = each_node.transform.parent;
                Transform Prev_graph_parent = Prev_node_parent.transform.parent;
                Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
                Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
                Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);
                Transform[] allChildrennode = Prev_node_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
                Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();


                foreach (Transform child in allChildrennode)
                {
                    child.parent = tempnodeparent.transform;
                }

                if (Prev_edge_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrenedge)
                    {
                        if (child.tag == "edge")
                            child.parent = tempedgeparent.transform;
                    }
                }

                if (Prev_simplicial_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrensimpli)
                    {
                        if (child.tag == "simplicial")
                            child.parent = tempsimplicialparent.transform;
                    }
                }

                if (Prev_hyper_parent.gameObject.activeSelf)
                {
                    foreach (Transform child in allChildrenhyper)
                    {
                        if (child.tag == "hyper")
                            child.parent = temphyperparent.transform;
                    }
                }

                Destroy(Prev_graph_parent.gameObject);
                Destroy(Prev_node_parent.gameObject);
                Destroy(Prev_edge_parent.gameObject);
                Destroy(Prev_simplicial_parent.gameObject);
                Destroy(Prev_hyper_parent.gameObject);
            }
            else
            {
                each_node.transform.parent = tempnodeparent.transform;
            }
        }

        //tempgraph.GetComponent<GraphElementScript>().Graph_as_Str();
        tempgraph.GetComponent<GraphElementScript>().Graph_init();
    }

    void CreateEmptyEdgeObjects()
    {
        // for all penline objects, create an anchor on top of them for a possible edge end
        GameObject[] penobjs = GameObject.FindGameObjectsWithTag("iconic");
        for (int i = 0; i < penobjs.Length; i++)
        {            
                GameObject tempcyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tempcyl.tag = "temp_edge_primitive";
                tempcyl.transform.position = penobjs[i].GetComponent<iconicElementScript>().edge_position + new Vector3(0f, 0f, 20f);
                //tempcyl.transform.localScale = new Vector3(10f, 10f, 10f);
                tempcyl.transform.localScale = new Vector3(20f, 20f, 20f);
                tempcyl.transform.Rotate(new Vector3(90f, 0f, 0f));
                tempcyl.transform.parent = penobjs[i].transform;
                tempcyl.GetComponent<Renderer>().material.color = Color.blue;            
        }
    }

    public void DeleteEmptyEdgeObjects()
    {
        GameObject[] tempcyls = GameObject.FindGameObjectsWithTag("temp_edge_primitive");
        for (int k = 0; k < tempcyls.Length; k++)
        {
            Destroy(tempcyls[k]);
        }
    }

    void DisableIconicCollider()
    {
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void searchNodeAndDeleteEdge(GameObject node_name)
    {
        Transform Prev_node_parent = node_name.transform.parent;

        if (Prev_node_parent.tag != "node_parent") return;
        
        Transform Prev_graph_parent = Prev_node_parent.transform.parent;
        Transform Prev_edge_parent = Prev_graph_parent.GetChild(1);
        Transform Prev_simplicial_parent = Prev_graph_parent.GetChild(2);
        Transform Prev_hyper_parent = Prev_graph_parent.GetChild(3);

        Transform[] allChildrenedge = Prev_edge_parent.GetComponentsInChildren<Transform>();
        Transform[] allChildrensimpli = Prev_simplicial_parent.GetComponentsInChildren<Transform>();
        Transform[] allChildrenhyper = Prev_hyper_parent.GetComponentsInChildren<Transform>();
                        
        foreach (Transform child in allChildrenedge)
        {
            if (child.tag == "edge")
            {
                GameObject source = child.GetComponent<VectorElementScript>().edge_start;
                GameObject target = child.GetComponent<VectorElementScript>().edge_end;

                if (source == node_name || target == node_name)
                {
                    Destroy(child.gameObject);
                    //break;
                }
            }
        }
                
            
    }
       
    void deleteTempLineIfDoubleFinger()
    {
        // Should only be called when necessary -- to get rid of incomplete lines when a double finger is detected and we are not in the pan mode.
        if (Input.touchCount > 1 && templine != null && pan_button.GetComponent<AllButtonsBehaviors>().selected == false)
        {
            Destroy(templine);
        }
    }

    public IEnumerator HistoryModify(GameObject functionline)
    {
        Debug.Log("called from " + functionline.name);

        history.Add(functionline);
        yield return null;

        for (int j = 0; j < history.Count; j++)
        {
            // remove deleted functions
            if (history[j] == null) history.RemoveAt(j);
        }

        if (history.Count > 10)
        {            
            for(int j = 0; j < (history.Count - 10); j++)
            {
                history.RemoveAt(0);
            }
        }

        if (ActionHistoryEnabled)
        {
            StartCoroutine(HistoryShow());
        }

    }

    public IEnumerator HistoryShow()
    {
        Debug.Log("modifying ");
        GameObject first_item = history_list_viewer.transform.GetChild(0).gameObject;

        if (history_list_viewer.transform.childCount > 1)
        {
            for (int j = 1; j < history_list_viewer.transform.childCount; j++)
            {
                Destroy(history_list_viewer.transform.GetChild(j).gameObject);
            }
        }

        int child_iter = 1;

        for (int j = 0; j < history.Count; j++)
        {
            if (history[j] == null)
            {
                history.RemoveAt(j);
            }

            else if (history[j].GetComponent<FunctionElementScript>().vector_analysis_done)
            {
                GameObject temp_func_item = Instantiate(first_item);
                temp_func_item.transform.parent = history_list_viewer.transform;
                temp_func_item.transform.SetSiblingIndex(child_iter);

                temp_func_item.GetComponent<TextMeshProUGUI>().text = child_iter.ToString() + ". " + /*history[j].name + ": " + "\n" +*/
                    history[j].transform.GetChild(0).GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text;
                temp_func_item.GetComponent<TextMeshProUGUI>().fontSize = 8;
                //temp_func_item.transform.localScale = new Vector3(1f, 1f, 1f);
                child_iter++;

                yield return null;
            }
                            
        }
    }

    // clear dragged object in a coroutine, otherwise some scripts can not access it
    public IEnumerator clearclickedobj()
    {
        yield return null;
        dragged_arg_textbox = null;
    }

    // call graph_init in coroutine, after the destory has been taken care of
    public IEnumerator ClearGraphData(string deleted_mode, GameObject temp)
    {
        yield return null;

        if (temp != null && deleted_mode == "iconic")
            temp.transform.parent.GetComponent<GraphElementScript>().nodes_init();

        else if (temp != null && deleted_mode == "edge")
            temp.transform.parent.GetComponent<GraphElementScript>().edges_init();
                
    }

    public IEnumerator HandleKeyboardInput()
    {
        //Debug.Log("click_on_inputfield: " + click_on_inputfield.ToString());

        if (!click_on_inputfield)
        {

            //we don't want any redundant operation when a function name is being typed
            if (pan_button.GetComponent<AllButtonsBehaviors>().selected)
            {
                if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    Camera.main.transform.position += Vector3.right * speed /** Time.deltaTime*/;
                }
                if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    Camera.main.transform.position += Vector3.left * speed /** Time.deltaTime*/;
                }
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    Camera.main.transform.position += Vector3.up * speed /** Time.deltaTime*/;
                }
                if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    Camera.main.transform.position += Vector3.down * speed /** Time.deltaTime*/;
                }
                if (Input.GetKeyUp(KeyCode.Equals) || Input.GetKeyUp(KeyCode.KeypadPlus))
                {
                    float difference = 100;
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - zoom_multiplier * difference, zoom_min, zoom_max);

                    int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);

                    text_message_worldspace.SetActive(true);
                    text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";
                }
                if (Input.GetKeyUp(KeyCode.Minus) || Input.GetKeyUp(KeyCode.KeypadMinus))
                {
                    float difference = 100;
                    Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize + zoom_multiplier * difference, zoom_min, zoom_max);

                    int zoom = (int)((1f - ((main_camera.orthographicSize - zoom_min) / zoom_max)) * 100f);                    

                    text_message_worldspace.SetActive(true);
                    text_message_worldspace.GetComponent<TextMeshProUGUI>().text = zoom.ToString("F0") + "%";
                }
            }


            if (Input.GetKeyUp(KeyCode.L))
            {
                panZoomLocked = !panZoomLocked;
                Debug.Log("panning_value_change" + panZoomLocked.ToString());
                GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("panning: " + panZoomLocked.ToString());
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                free_hand_edge = !free_hand_edge;
                GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("free hand drawing: " + free_hand_edge.ToString());
                Debug.Log("NOT WORKING?");
            }


            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //cleanUpRadialCanvas();
            }

            if (Input.GetKeyUp(KeyCode.F10))
            {
                allowOpacity = !allowOpacity;
                Debug.Log("allow opacity: " + allowOpacity.ToString());
            }

            if (Input.GetKeyUp(KeyCode.Delete))
            {
                // delete game objects from history, starting with the latest
                if (history.Count > 0)
                {
                    Destroy(history[history.Count - 1]);
                    history.RemoveAt(history.Count - 1);
                }
            }

            // test thumbnail
            if (Input.GetKeyUp(KeyCode.T))
            {
                /*
                if (gameObject.transform.childCount > 0)
                {
                    RuntimePreviewGenerator.PreviewDirection = new Vector3(0, 0, 1);
                    RuntimePreviewGenerator.BackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0f);
                    RuntimePreviewGenerator.OrthographicMode = true;

                    Sprite action = Sprite.Create(RuntimePreviewGenerator.GenerateModelPreview(gameObject.transform.GetChild(0), 128, 128)
                        , new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 20f);
                    GameObject.Find("Action").GetComponent<Image>().sprite = action;
                }
                */
            }

            // test adding to action history from one template
            if (Input.GetKeyUp(KeyCode.M))
            {
                // This test works
                /*
                GameObject actionhist = GameObject.Find("ActionHistory");
                GameObject listtocopy = actionhist.transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
                GameObject newitem = Instantiate(listtocopy, listtocopy.transform.parent);
                */

                ActionHistoryEnabled = !ActionHistoryEnabled;

                history_view.gameObject.SetActive(ActionHistoryEnabled);

                if (ActionHistoryEnabled)
                {
                    StartCoroutine(HistoryShow());
                }
            }

            //Graph
            if (Input.GetKeyUp(KeyCode.G))
            {
                graphlocked = !graphlocked;
                GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("graph lock: " + graphlocked.ToString());
            }

            if (Input.GetKeyUp(KeyCode.D))
            {
                directed_edge = !directed_edge;
                GameObject temp_stat = Instantiate(status_label_obj, canvas_radial.transform);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("directed edge: " + directed_edge.ToString());
            }
            
            //StartCoroutine(clearkeyboard());
           

        }

        click_on_inputfield = false;

        /*if (PenTouchInfo.ReleasedThisFrame)
            dragged_arg_textbox = null;*/

        yield return null;
    }

}
