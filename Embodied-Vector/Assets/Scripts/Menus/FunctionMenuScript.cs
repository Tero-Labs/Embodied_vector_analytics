using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FunctionMenuScript : MonoBehaviour
{
    public Toggle evaluation_type;
    public bool instant_eval;
    public Toggle keepchilden;
    public Toggle show_result;
    public bool keep_child_object;
    public bool eval_finished;

    // to trace if it was initiated from the slider
    public bool passive_func_call = false;
    public bool passive_func_call_root = false;

    public InputField mainInputField;
    public Button perform_action;
    public Button settings;
    public GameObject text_label;
    
    public GameObject input_option;
    public bool match_found;
    private bool textbox_open;
    private bool draggable_now;

    private Vector3 touchDelta = new Vector3();

    public GameObject paintable;
    public bool red_flag;
    public TMP_Text tmptextlabel;
    public Image img;

    public GameObject topo_label;

    public GameObject message_box;
    public GameObject argument_text_box;
    public GameObject dragged_arg_object;
    public GameObject video_temp_graph;


    // functions_list
    private Dictionary<int,string> divergence_dict;

    private GameObject argument_text;
    public GameObject drag_text_ui;
    public Camera main_camera;

    public GameObject[] argument_objects;

    // current function dictionary
    public Dictionary<int, string> cur_dict;
    // current function order dictionary
    //public Dictionary<int, int> cur_order_dict;
    // needed to track the argument string for display
    public Dictionary<int, string> cur_arg_Str;
    public int cur_iter;

    public string output_type;

    // for drag interaction
    Color basecolor;

    // Start is called before the first frame update
    void Start()
    {
        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        perform_action.onClick.AddListener(delegate { OnClickButton(perform_action); });
        settings.onClick.AddListener(delegate { OnSettingsButton(settings); });
        show_result.onValueChanged.AddListener(delegate { ResultVisiblity(show_result); });

        argument_text = null;
        dragged_arg_object = null;

        keepchilden.onValueChanged.AddListener(delegate { ChildToggle(keepchilden); });
        //evaluation_type.onValueChanged.AddListener(delegate { InstantEvalToggle(evaluation_type); });
        instant_eval = false;
        keep_child_object = true;

        textbox_open = false;
        eval_finished = false;
        draggable_now = false;

        main_camera = Camera.main;

        basecolor = img.color;

        # region function arguments and types dictionary

        divergence_dict = new Dictionary<int, string>()
        {
            {0, "vectorfield"},
        };    

        # endregion
    }

    private void Update()
    {
       if (mainInputField != null && mainInputField.isFocused)
       {
            Paintable.click_on_inputfield = true;
            //return;
        }                

        if (paintable.GetComponent<Paintable>().function_brush_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.PressedThisFrame)
            {
                if (!eval_finished) message_box.GetComponent<TextMeshProUGUI>().text = "";
                if (TMP_TextUtilities.IsIntersectingRectTransform
                    (tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera))
                {
                    Paintable.dragged_arg_textbox = transform.gameObject;

                    /*if (!textbox_open && !eval_finished)
                    {
                        if (input_option.activeSelf) input_option.SetActive(false);
                        else input_option.SetActive(true);
                    }*/

                    if (match_found && !eval_finished)
                    {
                        //var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, main_camera, false);
                        var index = TMP_TextUtilities.FindNearestCharacter(tmptextlabel, PenTouchInfo.penPosition, main_camera, false);
                        //Debug.Log("Found character at " + index.ToString());

                        if (!textbox_open && index != -1)
                        {
                            //extra check for better UX
                            /*if (cur_dict.ContainsKey(index))
                                index = index;
                            else if(cur_dict.ContainsKey(index - 1))
                                index = index - 1;
                            else if (cur_dict.ContainsKey(index + 1))
                                index = index + 1;
                            else
                                index = -1;*/
                            if (tmptextlabel.textInfo.characterInfo[index].elementType.ToString() != "Sprite")
                            {
                                index = -1;
                            }

                        }

                        if (index != -1 && !textbox_open)
                        {

                            if (/*cur_dict[index] == "string" || cur_dict[index] == "int"*/ tmptextlabel.textInfo.characterInfo[index].spriteIndex == 2)
                            {
                                Vector3 TouchStart = Vector3.zero;

                                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, 
                                    main_camera, out TouchStart);
                                                                
                                //TouchStart = main_camera.transform.InverseTransformPoint(TouchStart);

                                //Debug.Log(TouchStart.ToString());

                                argument_text = Instantiate(argument_text_box,
                                    TouchStart,
                                    Quaternion.identity,
                                    tmptextlabel.rectTransform);

                                /*argument_text = Instantiate(argument_text_box,
                                    //TouchStart,
                                    paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(TouchStart),
                                    Quaternion.identity,
                                    paintable.GetComponent<Paintable>().canvas_radial.transform);*/

                                argument_text.GetComponent<FunctionTextInputMenu>().setArgument(transform.gameObject, /*index*/cur_iter);
                                textbox_open = true;
                            }
                        }
                    }
                }
                //else check if any other object is clicked on
                else
                {
                    
                    //remove active textbox, if any                    
                    if (textbox_open && argument_text.GetComponent<FunctionTextInputMenu>().str_arg.Length > 0)
                    {
                        int index = argument_text.GetComponent<FunctionTextInputMenu>().str_index;
                        string str_arg = argument_text.GetComponent<FunctionTextInputMenu>().str_arg;

                        if (str_arg.Length > 0)
                        {
                            cur_arg_Str[index] = str_arg;
                            string arg_Str = get_arguments_string();
                            text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text/*.Substring(0, 1).ToUpper() +
                                                    mainInputField.text.Substring(1).ToLower()*/ + arg_Str;
                            cur_iter++;
                            argument_objects[index] = text_label;
                        }

                        Destroy(argument_text);
                        argument_text = null;
                        textbox_open = false;
                    }

                    var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                    RaycastHit Hit;
                    RaycastHit2D hit2d = Physics2D.GetRayIntersection(ray);

                    if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
                    {
                        dragged_arg_object = Hit.collider.gameObject;

                        // label instantiate
                        /*drag_text_ui = Instantiate(topo_label,
                            paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(Hit.point),
                            Quaternion.identity,
                            paintable.GetComponent<Paintable>().canvas_radial.transform);*/

                        drag_text_ui = Instantiate(topo_label,
                            Hit.point,
                            Quaternion.identity,
                            paintable.GetComponent<Paintable>().Objects_parent.transform);

                        drag_text_ui.GetComponent<TMP_Text>().text = "icon: " +
                            dragged_arg_object.GetComponent<iconicElementScript>().icon_number.ToString();

                        //show graph name too
                        if (dragged_arg_object.transform.parent.tag == "node_parent")
                        {
                            drag_text_ui.GetComponent<TMP_Text>().text += "\n" + "graph: " +
                            dragged_arg_object.transform.parent.parent.GetComponent<GraphElementScript>().graph_name;
                        }                            
                    }
                    else if (hit2d.collider != null && hit2d.collider.gameObject.tag == "edge")
                    {
                        dragged_arg_object = hit2d.collider.gameObject;
                        Debug.Log("graph picked from edge cllick");

                        drag_text_ui = Instantiate(topo_label,
                            hit2d.point,
                            Quaternion.identity,
                            paintable.GetComponent<Paintable>().Objects_parent.transform);

                        drag_text_ui.GetComponent<TMP_Text>().text = "graph: " +
                            dragged_arg_object.transform.parent.parent.GetComponent<GraphElementScript>().graph_name;
                    }
                }
            }

            else if (PenTouchInfo.ReleasedThisFrame
                && (dragged_arg_object != null ||
                (Paintable.dragged_arg_textbox != null && Paintable.dragged_arg_textbox != transform.gameObject)))
            {
                if (drag_text_ui != null)
                {
                    Destroy(drag_text_ui);
                    drag_text_ui = null;
                }

                if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera))
                {
                    Debug.Log("here_dragged_func");
                    var index = TMP_TextUtilities.FindNearestCharacter(tmptextlabel, PenTouchInfo.penPosition, main_camera, false);
                    //var index = TMP_TextUtilities.FindIntersectingCharacter(tmptextlabel, PenTouchInfo.penPosition, main_camera, false);

                    if (dragged_arg_object == null && Paintable.dragged_arg_textbox != null)
                    {                        
                        dragged_arg_object = Paintable.dragged_arg_textbox;
                    }

                    //Debug.Log("Found draaged object" + dragged_arg_object.name + " character at " + index.ToString());

                    if (index != -1)
                    {
                        //extra check for better UX
                        /*if (cur_dict.ContainsKey(index))
                            index = index;
                        else if(cur_dict.ContainsKey(index - 1))
                            index = index - 1;
                        else if (cur_dict.ContainsKey(index + 1))
                            index = index + 1;
                        else
                            index = -1;*/
                        if (tmptextlabel.textInfo.characterInfo[index].elementType.ToString() != "Sprite")
                        {
                            index = -1;
                        }

                    }

                    if (index != -1)
                    {
                        index = cur_iter;
                        Transform temp = null;
                        // whether graph or icon is expected as arg.type
                        if (cur_dict[index] == "graph")
                        {
                            if (dragged_arg_object.transform.parent.tag == "node_parent")
                            {
                                temp = dragged_arg_object.transform.parent.parent;
                                temp = FindifInsideLasso(temp.gameObject);

                                // argument obj referece update for calculation
                                //argument_objects[cur_order_dict[index]] = temp.gameObject;
                                if (temp != null)
                                {
                                    argument_objects[index] = temp.gameObject;
                                    cur_arg_Str[index] = temp.GetComponent<GraphElementScript>().graph_name;
                                }
                                    
                            }

                            else if (dragged_arg_object.transform.parent.tag == "edge_parent")
                            {
                                temp = dragged_arg_object.transform.parent.parent;
                                temp = FindifInsideLasso(temp.gameObject);

                                // argument obj referece update for calculation
                                //argument_objects[cur_order_dict[index]] = temp.gameObject;
                                if (temp != null)
                                {
                                    argument_objects[index] = temp.gameObject;
                                    cur_arg_Str[index] = temp.GetComponent<GraphElementScript>().graph_name;
                                }                                    
                            }

                            else if (Paintable.dragged_arg_textbox != null &&
                                Paintable.dragged_arg_textbox.GetComponent<FunctionMenuScript>() != null &&
                                Paintable.dragged_arg_textbox.transform.GetComponent<FunctionMenuScript>().output_type != "scalar")
                            {
                                Debug.Log("here_in_arg_drag");
                                temp = Paintable.dragged_arg_textbox.transform;
                                cur_arg_Str[index] = temp.GetComponent<FunctionMenuScript>().text_label.GetComponent<TextMeshProUGUI>().text;
                                //argument_objects[index] = temp.parent.GetChild(1).gameObject;
                                argument_objects[index] = temp.parent.gameObject;
                            }
                        }

                        else if (cur_dict[index] == "iconic" && dragged_arg_object.tag == "iconic")
                        {
                            temp = dragged_arg_object.transform;

                            // argument obj referece update for calculation
                            //argument_objects[cur_order_dict[index]] = temp.gameObject;
                            if (transform.parent.GetComponent<FunctionElementScript>().isInsidePolygon(
                                                temp.GetComponent<iconicElementScript>().edge_position))
                            {
                                cur_arg_Str[index] = temp.GetComponent<iconicElementScript>().icon_number.ToString();
                                argument_objects[index] = temp.gameObject;
                            }
                            else
                                temp = null;                            
                           
                        }

                        else if (cur_dict[index] == "iconic" && dragged_arg_object.tag == "edge")
                        {
                            temp = dragged_arg_object.GetComponent<VectorElementScript>().edge_start.transform;

                            if (transform.parent.GetComponent<FunctionElementScript>().isInsidePolygon(
                                                temp.GetComponent<iconicElementScript>().edge_position))
                            {
                                cur_arg_Str[index] = temp.GetComponent<iconicElementScript>().icon_number.ToString();
                                argument_objects[index] = temp.gameObject;
                            }
                            else
                            {
                                temp = dragged_arg_object.GetComponent<VectorElementScript>().edge_end.transform;

                                if (transform.parent.GetComponent<FunctionElementScript>().isInsidePolygon(
                                                    temp.GetComponent<iconicElementScript>().edge_position))
                                {
                                    cur_arg_Str[index] = temp.GetComponent<iconicElementScript>().icon_number.ToString();
                                    argument_objects[index] = temp.gameObject;
                                }
                                else
                                    temp = null;
                            }
                            
                        }

                        else if (cur_dict[index] == "string" || cur_dict[index] == "int")
                        {
                            // double check if the draaged object is not null
                            if (Paintable.dragged_arg_textbox != null &&
                                Paintable.dragged_arg_textbox.GetComponent<FunctionMenuScript>() != null &&
                                Paintable.dragged_arg_textbox != transform.gameObject)
                            {
                                temp = Paintable.dragged_arg_textbox.transform;
                                cur_arg_Str[index] = temp.GetComponent<FunctionMenuScript>().message_box/*text_label*/.GetComponent<TextMeshProUGUI>().text;
                                // argument obj referece update for calculation
                                //TodO:add_for_int_arguments
                                //argument_objects[cur_order_dict[index]] = temp.gameObject;
                                argument_objects[index] = temp.gameObject;
                            }

                            else if (Paintable.dragged_arg_textbox != null &&
                                Paintable.dragged_arg_textbox.GetComponent<topoLabelScript>() != null)
                            {
                                temp = Paintable.dragged_arg_textbox.transform;
                                cur_arg_Str[index] = temp.GetComponent<topoLabelScript>().tmptextlabel.text;
                                
                                argument_objects[index] = temp.gameObject;
                            }
                        }


                        if (temp != null)
                        {
                            // argument string update for display
                            string arg_Str = get_arguments_string();
                            text_label.GetComponent<TextMeshProUGUI>().text = mainInputField.text/*.Substring(0, 1).ToUpper() +
                                                    mainInputField.text.Substring(1).ToLower()*/ + arg_Str;
                            cur_iter++;
                        }
                        else
                        {
                            message_box.GetComponent<TextMeshProUGUI>().text = "Invalid argument!";
                        }

                    }
                    
                }

                /*paintable.GetComponent<Paintable>().dragged_arg_textbox = null;
                dragged_arg_object = null;*/
                StartCoroutine(clearclickedobj());
            }

             
            else if (PenTouchInfo.PressedNow
                && drag_text_ui != null)
            {
                var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
                RaycastHit Hit;
                if (Physics.Raycast(ray, out Hit))
                {
                    //drag_text_ui.transform.position = paintable.GetComponent<Paintable>().canvas_radial.transform.TransformPoint(Hit.point);
                    drag_text_ui.transform.position = Hit.point;
                }                
            }

            else if (PenTouchInfo.ReleasedThisFrame
                && drag_text_ui != null)
            {
                Destroy(drag_text_ui);
                drag_text_ui = null;
            }
        }

        else if (PenTouchInfo.PressedNow && paintable.GetComponent<Paintable>().eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera))
            {
                Destroy(transform.parent.gameObject);
            }
        }
        else if (!(paintable.GetComponent<Paintable>().panZoomLocked))
        {
            checkHitAndMove();
        }
        else
            return;
    }

    Transform FindifInsideLasso(GameObject graph)
    {
        List<int> selected_icons = new List<int>();

        Transform node_parent = graph.transform.GetChild(0);

        for (int i = 0; i < node_parent.childCount; i++)
        {
            Transform child = node_parent.GetChild(i);
            if (child != null && child.tag == "iconic")
            {
                if (transform.parent.GetComponent<FunctionElementScript>().isInsidePolygon(
                                                child.GetComponent<iconicElementScript>().edge_position))
                {
                    selected_icons.Add(child.GetComponent<iconicElementScript>().icon_number);
                }
            }
        }
        
        if (node_parent.childCount == selected_icons.Count)
        {
            //Debug.Log("completely inside our lasso");
            return graph.transform;
        }
        // create new temporary graph
        else if (selected_icons.Count > 0) 
        {
            //Debug.Log("partially inside lasso");
            
            GameObject temp_graph = Instantiate(graph);
            temp_graph.transform.parent = paintable.GetComponent<Paintable>().Objects_parent.transform;
            temp_graph.name = "temp_graph";

            // store which graph it was subgraph of
            temp_graph.GetComponent<GraphElementScript>().parent_graph = graph;

            temp_graph.GetComponent<GraphElementScript>().graph.nodes = new List<int>();
            temp_graph.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();

            node_parent = temp_graph.transform.GetChild(0);
            for (int i = 0; i < node_parent.childCount; i++)
            {
                Transform child = node_parent.GetChild(i);
                if (child != null && child.tag == "iconic")
                {
                    if (selected_icons.Contains(child.GetComponent<iconicElementScript>().icon_number) == false)
                    {
                        Destroy(child.gameObject);
                    }
                    else
                    {
                        temp_graph.GetComponent<GraphElementScript>().graph.nodes.Add(child.GetComponent<iconicElementScript>().icon_number);
                        temp_graph.GetComponent<GraphElementScript>().nodeMaps.Add(child.GetComponent<iconicElementScript>().icon_number.ToString(), child);
                    }
                }
            }

            
            temp_graph.SetActive(false);
            return temp_graph.transform;
        }
        else
        {
            //Debug.Log("completely outside lasso");
            return null;
        }

    }

    public void checkHitAndMove()
    {
       
        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera))
            {
                paintable.GetComponent<Paintable>().current_dragged_function = transform.gameObject;
                Debug.Log("started drag");

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = text_label.transform.position - vec;
                // change anchor color
                img.color = Color.gray;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow && paintable.GetComponent<Paintable>().current_dragged_function != null
            && paintable.GetComponent<Paintable>().current_dragged_function == transform.gameObject)
        {
            
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera))
            {
                //Debug.Log(transform.name);

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition, main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - text_label.transform.position + touchDelta;
                diff.z = 0;

                // don't move right away, move if a threshold has been crossed
                // 5 seems to work well in higher zoom levels and for my finger
                //if (Vector3.Distance(transform.position, vec) > 5)
                // update the function position. 
                transform.parent.GetComponent<FunctionElementScript>().mesh_holder.transform.position += diff;
                transform.position += diff;

                if (transform.parent.GetChild(1).tag == "graph")
                {
                    transform.parent.GetChild(1).position += diff;
                    transform.parent.GetChild(1).GetComponent<GraphElementScript>().checkHitAndMove(diff);
                }
                    


                // update the menu position if a menu is currently open/created
                /*GameObject menu_obj = GameObject.Find(menu_name);
                if (menu_obj != null)
                {
                    GameObject.Find(menu_name).transform.position = vec;
                }*/

                // if there are pen objects in the immediate hierarchy, and they have a path defined and abs. layer = 1, 
                // then update the red path to move with the function hierarchy
                /*for (int i = 2; i < transform.childCount; i++)
                {
                    if (transform.GetChild(i).tag == "penline" &&
                        transform.GetChild(i).GetComponent<penLine_script>().abstraction_layer == 1)
                    {
                        if (transform.GetChild(i).GetComponent<penLine_script>().translation_path.Count > 0)
                            transform.GetChild(i).GetComponent<penLine_script>().calculateTranslationPathIfAlreadyDefined();
                    }
                }*/
            }
                        
        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now
              && paintable.GetComponent<Paintable>().current_dragged_function != null 
              && paintable.GetComponent<Paintable>().current_dragged_function == transform.gameObject)
        {
            draggable_now = false;

            touchDelta = new Vector3(); // reset touchDelta
            img.color = basecolor;//new Color32(125, 255, 165, 255);

            paintable.GetComponent<Paintable>().current_dragged_function = null;

            // change anchor color

            // TODO: as an added safety measure, we could use a raycast at the touch position to see if it hits the set anchor
            // This might be the reason for weird function behavior when changing the slider or moving the function around??
            // Comments from penline object's checkHitAndMove() Touch.ended block:
            // // important: touch can start and end when interacting with other UI elements like a set's slider.
            // // so, double check that this touch was still on top of the penline object, not, say, on a slider.

            // check if the function came out of any parent function's area, and went into another function
            /*GameObject[] functions = GameObject.FindGameObjectsWithTag("function");
            bool in_canvas = false;

            for (int i = 0; i < functions.Length; i++)
            {
                // is anchor ( child(0) ) inside the polygon?
                if (functions[i].GetComponent<functionLine_script>().isInsidePolygon(
                    functions[i].GetComponent<functionLine_script>().transform.InverseTransformPoint(
                    transform.GetChild(0).position))
                    &&
                    (functions[i].GetComponent<functionLine_script>().abstraction_layer == 1 ||
                    functions[i].GetComponent<functionLine_script>().abstraction_layer == 2)
                    )
                {
                    transform.parent = functions[i].transform;
                    in_canvas = true;

                    // save log
                    // save coordinates wrt parent center
                    if (abstraction_layer == 1)
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
                // in case it was invisible, make it visible
                parent_asked_to_lower_layer = false;

                // save log
                string tkey = "paintable|" + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                if (!saved_positions.ContainsKey(tkey))
                    saved_positions.Add(tkey, (Vector2)transform.position);
            }*/
        }
        
    }
    
    string get_arguments_string()
    {
        string argument_str = "( ";

        /*foreach (GameObject each_graph in transform.parent.GetComponent<FunctionElementScript>().grapharray)
        {
            argument_str += each_graph.GetComponent<GraphElementScript>().graph_name + ", ";
        }*/

        foreach (int cur_key in cur_arg_Str.Keys)
        {
            argument_str += cur_arg_Str[cur_key] + ", ";
        }
        argument_str = argument_str.Remove(argument_str.Length - 2) + " )";

        return argument_str;
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/

        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {            
            match_found = false;            

            // ToDo: show red flag where no match

            // check if a valid function has been mapped, then inform paintable they can now instantiate drawing functions
            if (input.text.ToLower().Equals("divergence"))
            {
                match_found = true;
                cur_dict = divergence_dict;
                //cur_order_dict = addition_order_dict;
                
                output_type = "scalar";
                input.text = "Divergence";
            }
            
            if (match_found)
            {
                cur_iter = 0;
                cur_arg_Str = new Dictionary<int, string>();
                argument_objects = new GameObject[cur_dict.Count];

                foreach (int key in cur_dict.Keys)
                {
                    if (cur_dict[key] == "vectorfield")
                        cur_arg_Str.Add(key, transform.parent.GetComponent<FunctionElementScript>().selected_vectors[0].transform.parent.name);
                        // cur_arg_Str.Add(key, "<sprite name=\"graph_box\">");
                    else if (cur_dict[key] == "iconic")
                        cur_arg_Str.Add(key, "<sprite name=\"node_box\">");
                    else
                        cur_arg_Str.Add(key, "<sprite name=\"text_box\">");
                }

                string argument_str = get_arguments_string();

                text_label.GetComponent<TextMeshProUGUI>().text = input.text/*.Substring(0,1).ToUpper() +
                                                    input.text.Substring(1).ToLower()*/ + argument_str;

                //paintable.GetComponent<Paintable>().no_func_menu_open = false;
            }

        }
    }

    void OnClickButton(Button perform_action)
    {
        Debug.Log("evaluation_began");
        if (match_found)
        {
            //transform.parent.GetComponent<FunctionElementScript>().updateLassoPoints();
                        
            /*if (output_type != "scalar")
            {*/
                passive_func_call = false;
                StartCoroutine(CheckUnevaluatedFunctionArguments(transform.gameObject));
                //transform.parent.GetComponent<FunctionCaller>().GetGraphJson(argument_objects, mainInputField.text.ToLower());                
            //}

            input_option.SetActive(false);
            paintable.GetComponent<Paintable>().no_func_menu_open = true;            
        }
    }

    public void InitiateFunctionCallHelper(GameObject video_player = null)
    {
        passive_func_call = true;
        passive_func_call_root = true;
        transform.parent.GetComponent<FunctionElementScript>().video_player = video_player;

        if (video_player != null)
        {
            //video_temp_graph = FindifInsideLasso(argument_objects[0]).gameObject;
            for (int i = 0; i < argument_objects.Length; i++)
            {
                if (argument_objects[i].tag == "graph" && argument_objects[i].GetComponent<GraphElementScript>().video_graph)
                {
                    Transform temp_graph_transform = FindifInsideLasso(argument_objects[i]);
                    argument_objects[i] = temp_graph_transform.gameObject;
                }
            }
        }            

        StartCoroutine(CheckUnevaluatedFunctionArguments());
    }
    
    public void Hide()
    {
        foreach (GameObject child_graph in argument_objects)
        {
            if (child_graph != null && child_graph.tag == "graph")
            {
                // if it is under a function, hide that as well 
                if (child_graph.transform.parent.name.Contains("function_line_"))
                {
                    // setting as true, otherwise the function will not be called
                    child_graph.transform.parent.GetChild(0).gameObject.SetActive(true);
                    child_graph.transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                    child_graph.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    child_graph.SetActive(false);
                }
            }

            else if (child_graph != null && child_graph.tag == "function")
            {
                // setting as true, otherwise the function will not be called
                child_graph.transform.GetChild(0).gameObject.SetActive(true);
                child_graph.transform.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                child_graph.transform.gameObject.SetActive(false);
            }
        }
    }

    public void Show()
    {
        // we need not show anything, if the final evaluation was done 
        if (eval_finished) return;

        // show if it was a passive function call or a call through slider 
        /*transform.parent.GetComponent<FunctionElementScript>().mesh_holder.SetActive(true);
        message_box.GetComponent<TextMeshProUGUI>().text = "";
        settings.transform.gameObject.SetActive(true);
        perform_action.transform.gameObject.SetActive(true);
        transform.gameObject.SetActive(true);*/

        transform.parent.gameObject.SetActive(true);
        // very important. we will be hiding the child object produced earlier
        transform.parent.GetChild(1).gameObject.SetActive(false);        

        foreach (GameObject child_graph in argument_objects)
        {            
            if (child_graph != null && child_graph.tag == "graph")
            {
                Debug.Log("showing: " + child_graph.transform.parent.name);

                // if it is under a function, hide that as well 
                if (child_graph.transform.parent.name.Contains("function_line_"))                    
                {
                    child_graph.transform.parent.gameObject.SetActive(true);

                    child_graph.transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().Show();                    
                }
                else
                {
                    child_graph.SetActive(true);
                }
            }
            else if (child_graph != null && child_graph.tag == "function")
            {
                // setting as true, otherwise the function will not be called
                child_graph.SetActive(true);
                child_graph.transform.GetChild(0).GetComponent<FunctionMenuScript>().Show();
            }
        }
    }
    
    public void videohide()
    {
        perform_action.transform.parent.gameObject.SetActive(false);
        transform.gameObject.SetActive(false);
    }

    public void PostProcess(string output = null)
    {
        
        if (mainInputField.text.ToLower().Equals("topologicalsort") || mainInputField.text.ToLower().Equals("degreesort"))
        {
            transform.parent.GetComponent<FunctionElementScript>().InstantiateTopoGraph();
        }
        else
        {
            if (output_type == "vector") transform.parent.GetComponent<FunctionElementScript>().InstantiateGraph(); //(output);
            else
            {
                transform.parent.GetComponent<FunctionElementScript>().InstantiateScalarOutput(output);
            }
        }

        // moved to functionelementscript because the coroutine can not run if i hide the object 
        //UIsetafterEval(output);
    }

    public void LocalFunctionEvaluate()
    {
        passive_func_call = false;
        if (mainInputField.text.ToLower().Equals("topologicalsort") || mainInputField.text.ToLower().Equals("degreesort"))
        {
            transform.parent.GetComponent<FunctionElementScript>().InstantiateTopoGraph();
        }
        else if (mainInputField.text.ToLower().Equals("divergence"))
        {
            transform.parent.GetComponent<FunctionElementScript>().DivergenceCalculate();
        }

        // moved to functionelementscript because the coroutine can not run if i hide the object 
        //UIsetafterEval(output);
    }

    public void UIsetafterEval(string output = null)
    {        
        // hide the children graph if the option was set or passive call was introduced from the current function line
        if (!keep_child_object || (passive_func_call && passive_func_call_root))
        {
            foreach (GameObject child_graph in argument_objects)
            {
                if (child_graph.tag == "graph")
                {
                    // if it is under a function, hide that as well 
                    if (child_graph.transform.parent.name.Contains("function_line_"))
                    {
                        // recursive hide call
                        // setting as true, otherwise the function will not be called
                        child_graph.transform.parent.GetChild(0).gameObject.SetActive(true);
                        child_graph.transform.parent.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                        // not sure about the following line
                        child_graph.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        child_graph.SetActive(false);
                    }
                }

                else if (child_graph.tag == "function")
                {
                    // recursive hide call
                    // setting as true, otherwise the function will not be called
                    child_graph.transform.GetChild(0).gameObject.SetActive(true);
                    child_graph.transform.GetChild(0).GetComponent<FunctionMenuScript>().Hide();
                    // not sure about the following line
                    child_graph.SetActive(false);
                }

                else if (child_graph.tag == "Untagged")
                    child_graph.SetActive(false);
            }
        }

        text_label.GetComponent<TextMeshProUGUI>().text = text_label.GetComponent<TextMeshProUGUI>().text.Replace(" ", "");

        if (passive_func_call == false)
        {
            Debug.Log("hiding");
            // to enable calling again when it was called passively
            eval_finished = true;            
            transform.parent.GetComponent<FunctionElementScript>().mesh_holder.SetActive(false);
            transform.gameObject.SetActive(false);

            settings.transform.gameObject.SetActive(false);
            perform_action.transform.parent.gameObject.SetActive(false);            
            perform_action.transform.gameObject.SetActive(false);            
            /*message_box.GetComponent<TextMeshProUGUI>().text = "";
            text_label.GetComponent<TextMeshProUGUI>().text = "";*/

            input_option.SetActive(false);
        }
        else
        {
            eval_finished = false;
        }                
        
        

        if (Paintable.dragged_arg_textbox == transform.gameObject)
            Paintable.dragged_arg_textbox = null;
    }

    void ChildToggle(Toggle toggle)
    {
        if (toggle.isOn) keep_child_object = true;
        else keep_child_object = false;
    }

    void OnSettingsButton(Button settings)
    {
        bool flag = input_option.activeSelf;
        input_option.SetActive(!(flag));
        paintable.GetComponent<Paintable>().no_func_menu_open = flag;

        if (eval_finished)
        {
            evaluation_type.gameObject.SetActive(false);
            keepchilden.gameObject.SetActive(false);
            mainInputField.gameObject.SetActive(false);
            show_result.gameObject.SetActive(true);
            /*keepchilden.interactable = false;
            mainInputField.interactable = false;*/
        }
    }

    void ResultVisiblity(Toggle toggle)
    {
        if (eval_finished)
        {
            if (output_type == "graph")
            {
                transform.parent.GetChild(1).gameObject.SetActive(toggle.isOn);
                //transform.parent.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        if (input_option.activeSelf && paintable != null)
        {
            paintable.GetComponent<Paintable>().no_func_menu_open = true;
            if (Paintable.dragged_arg_textbox == transform.gameObject)
                Paintable.dragged_arg_textbox = null;
        } 

        if (textbox_open)
        {
            Destroy(argument_text);
            argument_text = null;
            textbox_open = false;
        }
    }

    IEnumerator CheckUnevaluatedFunctionArguments(GameObject self_call = null)
    {
        bool temp_flag = true;
        if (!eval_finished || self_call == null)
        {
            /*
            int idx = 0;            
            foreach (GameObject child_graph in argument_objects)
            {
                // if an argument is missing
                if (child_graph == null)
                {
                    temp_flag = false;
                    message_box.GetComponent<TextMeshProUGUI>().text = "Can not evaluate!";
                    break;
                }

                // if it is under a function, call that as well if it is not evaluated already
                if (child_graph.name.Contains("function_line_")
                    && !child_graph.transform.GetChild(0).GetComponent<FunctionMenuScript>().eval_finished)
                {
                    Transform cur_function_line = child_graph.transform;
                    cur_function_line.GetChild(0).gameObject.SetActive(true);

                    cur_function_line.GetChild(0).GetComponent<FunctionMenuScript>().passive_func_call = passive_func_call;

                    Debug.Log("child: " +
                        cur_function_line.GetChild(0).GetComponent<FunctionMenuScript>().mainInputField.text.ToLower() +
                        " called.");
                    yield return StartCoroutine
                        (cur_function_line.GetChild(0).GetComponent<FunctionMenuScript>().CheckUnevaluatedFunctionArguments());

                    yield return null;
                    //argument_objects[idx] = cur_function_line.gameObject;
                }
                
                idx++;
            }            
            */

            if (temp_flag)
            {
                transform.parent.GetComponent<FunctionElementScript>().vector_analysis_done = false;                               

                // i_initiated
                // transform.parent.GetComponent<FunctionCaller>().GetGraphJson(argument_objects, mainInputField.text.ToLower());
                LocalFunctionEvaluate();

                while (transform.parent.GetComponent<FunctionElementScript>().vector_analysis_done == false)
                {
                    Debug.Log("waiting_until_" + transform.parent.name + "_finished_executing");
                    yield return null;
                }                
            }

            // an extra yield for double check
            yield return null;            
        }

        else
        {
            transform.parent.GetChild(1).gameObject.SetActive(true);
            //UIsetafterEval();
        }
    }

    // if we set null directly, other open function menu can not access the parameters
    IEnumerator clearclickedobj()
    {
        yield return null;
        Paintable.dragged_arg_textbox = null;
        dragged_arg_object = null;
    }
}
