using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerChildrenAccess : MonoBehaviour
{
    public GameObject slider, PlayorPause, quad;
    public Canvas canvas;
    public Button settings, delete;
    public GameObject settings_menu;
    public GameObject control_menu;
    public GameObject paintable;

    public InputField mainInputField;
    public InputField WindowInputField;
    public Toggle node_radius, site_specific;
    public Toggle auto_track, manual_track;

    public float width, height;
    public static int time_slice;


    // Start is called before the first frame update
    void Start()
    {
        settings.onClick.AddListener(delegate { SettingsMenu(); });
        delete.onClick.AddListener(delegate { Delete(); });
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        WindowInputField.onValueChanged.AddListener(delegate { LockWindowInput(WindowInputField); });

        node_radius.onValueChanged.AddListener(delegate { GraphType(); });
        site_specific.onValueChanged.AddListener(delegate { GraphType(); });

        auto_track.onValueChanged.AddListener(delegate { TrackType(auto_track); });
        manual_track.onValueChanged.AddListener(delegate { TrackType(manual_track); });

        // to setup initial values
        GraphType();
        TrackType(auto_track);

        width = quad.transform.localScale.x;
        height = quad.transform.localScale.y;

        time_slice = 5;
        UIlayout();
    }

    public void UIlayout()
    {
        //http://www.robotmonkeybrain.com/convert-unity-ui-screen-space-position-to-world-position/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://forum.unity.com/threads/world-position-to-local-recttransform-position.445256/
        //https://stackoverflow.com/a/43736203

        Vector3 temp_pos = new Vector3(quad.transform.position.x,
                                                    quad.transform.position.y - (height/2) - 45,
                                                    0f);

        Vector3 screen_temp_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos);

        Vector2 rect_Try;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(control_menu.transform.parent.GetComponent<RectTransform>(), screen_temp_pos,
                                    null, out rect_Try);

        control_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try;

        Vector3 temp_pos_2 = new Vector3(quad.transform.position.x + (width / 2) + 72,
                                                   quad.transform.position.y,
                                                   0f);

        Vector3 screen_temp_pos_2 = RectTransformUtility.WorldToScreenPoint(Camera.main, temp_pos_2);

        Vector2 rect_Try_2;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(settings_menu.transform.parent.GetComponent<RectTransform>(),
                    screen_temp_pos_2, null, out rect_Try_2);

        settings_menu.GetComponent<RectTransform>().anchoredPosition = rect_Try_2;
    }

    void LockInput(InputField input)
    {
        // if a function is getting evaluated now, we will not receive any further input
        /*if (paintable.GetComponent<Paintable>().no_func_menu_open)
            return;*/

        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {            
            float result = slider.GetComponent<VideoController>().node_radius;
            float.TryParse(input.text, out result);
            slider.GetComponent<VideoController>().node_radius = result;
        }
    }

    void LockWindowInput(InputField input)
    {
        
        if (input.text.Length > 0)
        {
            int.TryParse(input.text, out time_slice);
        }
    }

    public void GraphType()
    {        
        if (site_specific.isOn) slider.GetComponent<VideoController>().graph_type = "SiteSpecific";
        else slider.GetComponent<VideoController>().graph_type = "NodeRadius";

        mainInputField.interactable = node_radius.isOn;
    }

    void TrackType(Toggle toggle)
    {
        slider.GetComponent<VideoController>().auto_track = auto_track.isOn;
    }

    void SettingsMenu()
    {
        settings_menu.SetActive(!settings_menu.activeSelf);
    }

    void Delete()
    {
        if (slider.GetComponent<VideoController>().graph_holder != null)
        {
            Destroy(slider.GetComponent<VideoController>().graph_holder);
        }

        Destroy(transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
        {
            Paintable.click_on_inputfield = true;
            return;
        }
    }

    public void checkHitAndMove(Vector3 diff)
    {
        transform.position += diff;
        control_menu.transform.position += diff;
        settings_menu.transform.position += diff;
        if (slider.GetComponent<VideoController>().graph_holder != null)
        {
            slider.GetComponent<VideoController>().graph_holder.transform.position += diff;
            slider.GetComponent<VideoController>().graph_holder.GetComponent<GraphElementScript>().checkHitAndMove(diff);
        }
            
    }
        
}
