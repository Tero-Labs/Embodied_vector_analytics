﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class AllButtonsBehaviors : MonoBehaviour
{

	public bool selected = false;
    public float width, height;
    public int macro_state_cnt;

    public Sprite record, play, normal;
        
	GameObject[] buttons;
    GameObject paint_canvas;
        
    public void whenSelected()
	{
		selected = true;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f); //new Color(1, 1, 1, 0.5f);
        		        
        // change scale
        transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);

        Vector3[] v = new Vector3[4];
        transform.GetComponent<RectTransform>().GetWorldCorners(v);

        Vector3 center = v[3];//(v[0] + v[3]) / 2;
        center.y -= (v[1].y - v[0].y);

        //https://docs.unity3d.com/ScriptReference/RectTransform.GetWorldCorners.html

        var temp_stat = Instantiate(paint_canvas.GetComponent<Paintable>().status_label_obj,
            center,
            Quaternion.identity, transform.parent);

        temp_stat.GetComponent<Status_label_text>().ChangeLabel(this.name + "\n selected");

        if (this.name == "Pan")
        {
            // enable all colliders to move primitives around
            enableAllPenObjectColliders();
            enableplayerColliders();
            paint_canvas.GetComponent<Paintable>().okayToPan = true;
            paint_canvas.GetComponent<Paintable>().panZoomLocked = false;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "IconicPen")
        {
            // allow drawing over existing pen/set etc. objects without interfering
            disableAllPenObjectColliders();
            
            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = Color.red;
        }

        else if (this.name == "Annotation")
        {
            // allow drawing over existing pen/set etc. objects without interfering
            disableAllPenObjectColliders();
            
            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = Color.red;
        }

        else if (this.name == "VectorBrush")
        {
            enableAllPenObjectColliders();
            
            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(true);
            paint_canvas.GetComponent<Paintable>().color_picker_script.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        
        else if (this.name == "Eraser")
        {
            enableAllPenObjectColliders();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "Copy")
        {
            enableAllPenObjectColliders();
            
            //paint_canvas.GetComponent<Paintable>().okayToPan = false;
            this.transform.GetComponent<CopyIconicObject>().start_copying = false;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "StrokeCombine")
        {
           
            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FunctionConversion")
        {
            enableAllPenObjectColliders();
            
            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/
            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FunctionBrush")
        {
            enableAllPenObjectColliders();
            //enablesimplicialColliders();
            //disable_menu_creation
            paint_canvas.GetComponent<Paintable>().panZoomLocked = true;
            Paintable.dragged_arg_textbox = null;

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "FileLoad")
        {            
            //paint_canvas.GetComponent<Paintable>().videoplayer.transform.parent.gameObject.SetActive(true);
            paint_canvas.GetComponent<FileLoadDialog>().DialogShow();

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "AnalysisPen")
        {
            enableAllPenObjectColliders();
            Paintable.dragged_arg_textbox = null;

            if (!paint_canvas.GetComponent<Paintable>().no_analysis_menu_open)
            {
                var analysis_menu = Instantiate(paint_canvas.GetComponent<Paintable>().analysis_radial_menu, transform.parent);
                analysis_menu.GetComponent<Graphplot>().paintable = paint_canvas;
                paint_canvas.GetComponent<Paintable>().no_analysis_menu_open = true;
            }

            /*ColorPickerClickCheck.Rpointer = false;
            ColorPickerClickCheck.Gpointer = false;
            ColorPickerClickCheck.Bpointer = false;
            ColorPickerClickCheck.Apointer = false;
            ColorPickerClickCheck.previewpointer = false;*/

            paint_canvas.GetComponent<Paintable>().color_picker.SetActive(false);
        }

        else if (this.name == "MacroRecord")
        {
            if (macro_state_cnt == 0)
            {
                transform.GetComponent<Image>().sprite = record;
                macro_state_cnt++;
            }
            else if (macro_state_cnt == 1)
            {
                transform.GetComponent<Image>().sprite = play;
                macro_state_cnt++;
            }
            else
            {
                macro_state_cnt = 0;
                transform.GetComponent<Image>().sprite = normal;
            }                

        }


        // deselect all other buttons
        for (int i = 0; i < buttons.Length; i++)
		{
			if (buttons[i].name == this.name) continue;

			buttons[i].GetComponent<AllButtonsBehaviors>().whenDeselected();
		}

	}

	public void whenDeselected()
	{
		selected = false;

		// change icon color
		transform.GetComponent<Image>().color = new Color(0.4f, 0.4f, 0.4f, 1f);

		// change scale
		transform.localScale = new Vector3(1f, 1f, 1f);

        // when a new button is selected, a templine might still exist. We need to destroy that as well.
        if (this.name == "IconicPen" || this.name == "Annotation")
        {
            if (paint_canvas.GetComponent<Paintable>().templine != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().templine);
                paint_canvas.GetComponent<Paintable>().templine = null;
            }

            StartCoroutine(CorrectIcons());
        }

        // incase any temp cylinder is left, we will clear them up 
        else if (this.name == "VectorBrush")
        {
            paint_canvas.GetComponent<Paintable>().DeleteEmptyEdgeObjects();

            StartCoroutine(CorrectVectorEdges());
        }

        else if (this.name == "Pan")
        {
            paint_canvas.GetComponent<Paintable>().okayToPan = false;
            disableplayerColliders();

            if (paint_canvas.GetComponent<Paintable>().canvas_radial.transform.childCount > 0)
            {
                for (int i = 0; i < paint_canvas.GetComponent<Paintable>().canvas_radial.transform.childCount; i++)
                {
                    Destroy(paint_canvas.GetComponent<Paintable>().canvas_radial.transform.GetChild(i).gameObject);
                }
            }
        }
                
        else if (this.name == "StrokeCombine")
        {
            if (paint_canvas.GetComponent<Paintable>().setline != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().setline);
                paint_canvas.GetComponent<Paintable>().setline = null;
            }
        }

        else if (this.name == "FunctionBrush")
        {           
            if (paint_canvas.GetComponent<Paintable>().functionline != null)
            {
                Destroy(paint_canvas.GetComponent<Paintable>().functionline);
                //paint_canvas.GetComponent<Paintable>().functionline = null;
            }

            StartCoroutine(CorrectFunctionLines());
        }
    }

    public void enableAllPenObjectColliders()
    {
        // enable the pen box colliders that are immediate children of the paintable object
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = true;
        }
    }

    public void disableAllPenObjectColliders()
    {
        // disable the pen box colliders that are immediate children of the paintable object
        GameObject[] drawnlist = GameObject.FindGameObjectsWithTag("iconic");

        foreach (GameObject icon in drawnlist)
        {
            if (icon.GetComponent<BoxCollider>() != null)
                icon.GetComponent<BoxCollider>().enabled = false;
        }
    }
    
    public void enableplayerColliders()
    {
        GameObject[] videoplayers = GameObject.FindGameObjectsWithTag("video_player");

        foreach (GameObject vp in videoplayers)
        {
            if (vp.GetComponent<MeshCollider>() != null)
                vp.GetComponent<MeshCollider>().enabled = true;
        }
    }

    public void disableplayerColliders()
    {
        GameObject[] videoplayers = GameObject.FindGameObjectsWithTag("video_player");

        foreach (GameObject vp in videoplayers)
        {
            if (vp.GetComponent<MeshCollider>() != null)
                vp.GetComponent<MeshCollider>().enabled = false;
        }
    }

    // Start is called before the first frame update
    void Start()
	{
		buttons = GameObject.FindGameObjectsWithTag("canvas_mode_button");
        paint_canvas = GameObject.FindGameObjectWithTag("paintable_canvas_object");

        width = transform.GetComponent<RectTransform>().sizeDelta.x * transform.GetComponent<RectTransform>().localScale.x;
        height = transform.GetComponent<RectTransform>().sizeDelta.y * transform.GetComponent<RectTransform>().localScale.y;
        macro_state_cnt = 0;
    }

    // Update is called once per frame
    void Update()
	{
		
	}

    IEnumerator CorrectIcons()
    {        
        yield return null;

        foreach(GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_icons)
        {
            if (cur == null) continue;
            if (cur.GetComponent<iconicElementScript>() != null)
            {
                if (cur.GetComponent<iconicElementScript>().points.Count < paint_canvas.GetComponent<Paintable>().min_point_count)
                {
                    Destroy(cur);
                }
            }
            
            else if(cur.GetComponent<BoxCollider>() == null)
            {
                Destroy(cur);
            }

            else if (cur.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Destroy(cur);
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_icons.Clear();
    }

    IEnumerator CorrectVectorEdges()
    {
        yield return null;

        foreach (GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_edges)
        {
            if (cur == null) continue;
            if (cur.GetComponent<VectorElementScript>().points.Count < 2)
            {
                Destroy(cur);
            }
            else if (cur.GetComponent<EdgeCollider2D>() == null)
            {
                Destroy(cur);
            }
            else
            {
                cur.GetComponent<VectorElementScript>().points.Clear();
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_edges.Clear();
    }
        
    IEnumerator CorrectFunctionLines()
    {
        yield return null;

        foreach (GameObject cur in paint_canvas.GetComponent<Paintable>().new_drawn_function_lines)
        {
            if (cur == null) continue;
            if (cur.transform.childCount < 3)
            {
                Destroy(cur);
            }
        }

        yield return null;
        paint_canvas.GetComponent<Paintable>().new_drawn_function_lines.Clear();
    }

}