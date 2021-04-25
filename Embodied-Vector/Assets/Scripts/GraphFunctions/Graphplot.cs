using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;
using UnityEngine.UI;
using TMPro;


public class Graphplot : MonoBehaviour
{
    public GameObject paintable;
    //public Camera main_camera;

    public GraphChart Graph;
    public int TotalPoints = 5;
    float lastTime = 0f;
    float lastX = 0f;

    public Button show_graph;
    public InputField mainInputField;
    public Image img;
    // for drag interaction
    Color basecolor;

    public TMP_Text arg_1;
    public TMP_Text arg_2;
    public TMP_Text arg_3;
    public GameObject arg_1_obj;
    public GameObject arg_2_obj;
    public GameObject arg_3_obj;

    public string arg_1_str;
    public string arg_2_str;
    public string arg_3_str;

    public GameObject first_arg;
    public GameObject second_arg;
    public string first_arg_str;
    public string second_arg_str;

    // holders
    public TMP_Text first_arg_tmp;
    public TMP_Text second_arg_tmp;

    public bool graph_flag;
    private bool draggable_now;
    private Vector3 touchDelta = new Vector3();

    void Start()
    {
        //main_camera = Camera.main;
        show_graph.onClick.AddListener(delegate { OnGraphShow(); });
        //mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        basecolor = img.color;
    }

    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (input.text.Length > 0)
        {
            Paintable.click_on_inputfield = true;
        }*/

        Paintable.click_on_inputfield = true;
        
    }


    void OnGraphShow()
    {
        graph_flag = !graph_flag;
        
        if (graph_flag)
        {
            string command = mainInputField.text;
            var args = command.Split('/');

            if (args[0] == "arg_1")
            {
                first_arg = arg_1_obj;
                if (first_arg.tag == "function") first_arg_str = arg_1_str;
                first_arg_tmp = arg_1;
            }
            else if (args[0] == "arg_2")
            {
                first_arg = arg_2_obj;
                if (first_arg.tag == "function") first_arg_str = arg_2_str;
                first_arg_tmp = arg_2;
            }
            else if (args[0] == "arg_3")
            {
                first_arg = arg_3_obj;
                if (first_arg.tag == "function") first_arg_str = arg_3_str;
                first_arg_tmp = arg_3;
            }
            else
            {
                var temp_stat = Instantiate(paintable.GetComponent<Paintable>().status_label_obj, transform.parent);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("not enough argument");
                return;
            }

            if (args[1] == "arg_1")
            {
                second_arg = arg_1_obj;
                if (second_arg.tag == "function") second_arg_str = arg_1_str;
                second_arg_tmp = arg_1;
            }
            else if (args[1] == "arg_2")
            {
                second_arg = arg_2_obj;
                if (second_arg.tag == "function") second_arg_str = arg_2_str;
                second_arg_tmp = arg_2;
            }
            else if (args[1] == "arg_3")
            {
                second_arg = arg_3_obj;
                if (second_arg.tag == "function") second_arg_str = arg_3_str;
                second_arg_tmp = arg_3;
            }
            else
            {
                var temp_stat = Instantiate(paintable.GetComponent<Paintable>().status_label_obj, transform.parent);
                temp_stat.GetComponent<Status_label_text>().ChangeLabel("not enough argument");
                return;
            }

            Graph.gameObject.SetActive(graph_flag);
            DataInit();
        }
        else
            Graph.gameObject.SetActive(graph_flag);

    }

    void DataInit()
    {
        if (Graph == null) // the ChartGraph info is obtained via the inspector
            return;
        float x = 3f * TotalPoints;
        Graph.DataSource.StartBatch(); // calling StartBatch allows changing the graph data without redrawing the graph for every change
        Graph.DataSource.ClearCategory("Player 1"); // clear the "Player 1" category. this category is defined using the GraphChart inspector


        for (int i = 0; i < TotalPoints; i++)  //add random points to the graph
        {
            // x: time, y: distance
            Graph.DataSource.AddPointToCategory("Player 1", System.DateTime.Now - System.TimeSpan.FromSeconds(x), 0); 

            // each time we call AddPointToCategory 
            x -= Random.value * 3f;
            lastX = x;
        }

        Graph.DataSource.EndBatch(); // finally we call EndBatch , this will cause the GraphChart to redraw itself
    }

    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
        {
            Paintable.click_on_inputfield = true;
            return;
        }

        // only work in graph analysis operation        
        checkDragOrPress();

        if (Paintable.pan_button.GetComponent<AllButtonsBehaviors>().selected) checkHitAndMove();

        if (Graph.gameObject.activeSelf == false) return;
        
        float time = Time.time;
        if (lastTime + 2f < time)
        {
            lastTime = time;
            lastX += Random.value * 3f;

            if (first_arg.tag == "iconic" && second_arg.tag == "iconic")
            Graph.DataSource.AddPointToCategoryRealtime("Player 1", System.DateTime.Now, 
                Vector3.Distance(first_arg.GetComponent<iconicElementScript>().edge_position, second_arg.GetComponent<iconicElementScript>().edge_position), 
                1f);

            else if (first_arg.tag == "function" && second_arg.tag == "function")
            {
                if (first_arg.transform.GetChild(1).tag != "graph") return;

                GameObject node_parent = first_arg.transform.GetChild(1).GetChild(0).gameObject;

                GameObject temp_1 = node_parent.transform.Find(first_arg_str).gameObject;
                if (temp_1.transform.childCount == 0) return;
                temp_1 = temp_1.transform.Find("label").gameObject;

                GameObject temp_2 = node_parent.transform.Find(second_arg_str).gameObject;
                if (temp_2.transform.childCount == 0) return;
                temp_2 = temp_2.transform.Find("label").gameObject;

                first_arg_tmp.GetComponent<TextMeshProUGUI>().text = temp_1.GetComponent<TextMeshProUGUI>().text;
                second_arg_tmp.GetComponent<TextMeshProUGUI>().text = temp_2.GetComponent<TextMeshProUGUI>().text;

                float val = int.Parse(temp_1.GetComponent<TextMeshProUGUI>().text) / int.Parse(temp_2.GetComponent<TextMeshProUGUI>().text);

                Graph.DataSource.AddPointToCategoryRealtime("Player 1", System.DateTime.Now, val, 1f);

            }
        }

    }

    void checkDragOrPress()
    {
        if (paintable.GetComponent<Paintable>().eraser_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            if (PenTouchInfo.ReleasedThisFrame)
            {
                if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
                {
                    paintable.GetComponent<Paintable>().no_analysis_menu_open = false;
                    Destroy(transform.gameObject);
                }
            }
        }

        if (!paintable.GetComponent<Paintable>().AnalysisPen_button.GetComponent<AllButtonsBehaviors>().selected) return;

        if (PenTouchInfo.ReleasedThisFrame)
        {
            if (Paintable.dragged_arg_textbox == null)
            {
                return;
            }

           
            var dragged_arg_object = Paintable.dragged_arg_textbox;

            if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
            {
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_1.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_1_obj = dragged_arg_object;
                }

                else if (dragged_arg_object.tag == "function")
                {
                    GameObject node_parent = dragged_arg_object.transform.GetChild(1).GetChild(0).gameObject;
                    GameObject temp_1 = node_parent.transform.Find(Paintable.dragged_icon_name).gameObject;
                    arg_1_str = temp_1.name;

                    temp_1 = temp_1.transform.Find("label").gameObject;
                    arg_1.GetComponent<TextMeshProUGUI>().text = temp_1.GetComponent<TextMeshProUGUI>().text;
                    arg_1_obj = dragged_arg_object;                    
                }
                    
            }

            else if (TMP_TextUtilities.IsIntersectingRectTransform(arg_2.rectTransform, PenTouchInfo.penPosition, null))
            {
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_2.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_2_obj = dragged_arg_object;
                }

                else if (dragged_arg_object.tag == "function")
                {
                    GameObject node_parent = dragged_arg_object.transform.GetChild(1).GetChild(0).gameObject;
                    GameObject temp_1 = node_parent.transform.Find(Paintable.dragged_icon_name).gameObject;
                    arg_2_str = temp_1.name;

                    temp_1 = temp_1.transform.Find("label").gameObject;
                    arg_2.GetComponent<TextMeshProUGUI>().text = temp_1.GetComponent<TextMeshProUGUI>().text;
                    arg_2_obj = dragged_arg_object;
                }
            }

            else if (TMP_TextUtilities.IsIntersectingRectTransform(arg_3.rectTransform, PenTouchInfo.penPosition, null))
            {
                if (dragged_arg_object.tag == "iconic")
                {
                    arg_3.GetComponent<TextMeshProUGUI>().text =
                        dragged_arg_object.transform.GetComponent<iconicElementScript>().icon_number.ToString();
                    arg_3_obj = dragged_arg_object;
                }

                else if (dragged_arg_object.tag == "function")
                {
                    GameObject node_parent = dragged_arg_object.transform.GetChild(1).GetChild(0).gameObject;
                    GameObject temp_1 = node_parent.transform.Find(Paintable.dragged_icon_name).gameObject;
                    arg_3_str = temp_1.name;

                    temp_1 = temp_1.transform.Find("label").gameObject;
                    arg_3.GetComponent<TextMeshProUGUI>().text = temp_1.GetComponent<TextMeshProUGUI>().text;
                    arg_3_obj = dragged_arg_object;
                }
            }
            
        }
    }

    public void checkHitAndMove()
    {

        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
            {                
                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(arg_1.rectTransform, PenTouchInfo.penPosition, null, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = arg_1.transform.position - vec;
                // change anchor color
                img.color = Color.gray;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow && draggable_now)
        {

            if (TMP_TextUtilities.IsIntersectingRectTransform(arg_1.rectTransform, PenTouchInfo.penPosition, null))
            {
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(arg_1.rectTransform, PenTouchInfo.penPosition, null, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - arg_1.transform.position + touchDelta;
                diff.z = 0;

                transform.position += diff;
            }

        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)
        {
            draggable_now = false;

            touchDelta = new Vector3(); // reset touchDelta
            img.color = basecolor;//new Color32(125, 255, 165, 255);

        }

    }

}
