using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;


public class CopyIconicObject : MonoBehaviour
{
	public bool start_copying = false;

    //public Vector3 start_location;
    //public Vector3 last_copied_location;

    public GameObject toCopy;
    public GameObject paint_canvas;
    public GameObject Objects_parent;
    public GameObject newedge = null;

    //public int copy_count = 0;
    public List<Vector3> copy_path = new List<Vector3>();

    Pen currentPen;

    // Passed along long-press touch from copy menu button
    //public Touch copy_touch;
    //public Tap longTap;

    private void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    public void copyAlongPath(Vector2 touchpos)
    {
        var ray = Camera.main.ScreenPointToRay(touchpos);
        RaycastHit Hit;
        Vector3 pos;

        if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
        {
            pos = Hit.point + new Vector3(0, 0, -5f);
        }
        else
        {
            return;
        }

        if (start_copying && copy_path.Count > 0)
        {
            // if the dragged distance is not too low, or to prevent adding objects when touch is stationary
            if (Vector3.Distance(copy_path[copy_path.Count - 1], pos) > 50)
            {
                pos.z = toCopy.GetComponent<iconicElementScript>().edge_position.z;
                copy_path.Add(pos);

                Vector3 target_pos = pos - toCopy.GetComponent<iconicElementScript>().edge_position;

                if (paint_canvas.GetComponent<Paintable>().graphlocked)
                // copy graph
                {
                    if (toCopy.transform.parent.tag == "node_parent")
                    {
                        GameObject graph = toCopy.transform.parent.parent.gameObject;
                        GameObject cp = Instantiate(graph, graph.transform.position + target_pos, Quaternion.identity, Objects_parent.transform);
                        cp.GetComponent<GraphElementScript>().checkHitAndMove(target_pos);
                    }
                }
                else
                {
                    GameObject cp = Instantiate(toCopy, toCopy.transform.position + target_pos, Quaternion.identity, toCopy.transform.parent);
                    cp.GetComponent<BoxCollider>().enabled = false;
                    //cp.GetComponent<iconicElementScript>().calculateTranslationPath();
                    cp.GetComponent<iconicElementScript>().edge_position = toCopy.GetComponent<iconicElementScript>().edge_position + target_pos;
                    Paintable.totalLines++;
                    cp.GetComponent<iconicElementScript>().icon_number = Paintable.totalLines;
                    cp.GetComponent<iconicElementScript>().icon_name = "iconic_" + Paintable.totalLines.ToString();
                    
                    // needs a unique name in the object hierarchy
                    cp.name = "iconic_" + (Paintable.totalLines).ToString();
                }                
            }
        }
        else if (start_copying && copy_path.Count == 0)
        {
            // add an offset upwards for the first copy
            pos += new Vector3(0, 20, 0);
            pos.z = toCopy.GetComponent<iconicElementScript>().edge_position.z;

            copy_path.Add(pos);

            Vector3 target_pos = pos - toCopy.GetComponent<iconicElementScript>().edge_position;

            if (paint_canvas.GetComponent<Paintable>().graphlocked)
            // copy graph
            {
                if (toCopy.transform.parent.tag == "node_parent")
                {
                    GameObject graph = toCopy.transform.parent.parent.gameObject;
                    GameObject cp = Instantiate(graph, graph.transform.position + target_pos, Quaternion.identity, Objects_parent.transform);
                    cp.GetComponent<GraphElementScript>().checkHitAndMove(target_pos);
                }
            }
            else
            {
                GameObject cp = Instantiate(toCopy, toCopy.transform.position + target_pos, Quaternion.identity, toCopy.transform.parent);
                //GameObject cp = Instantiate(toCopy, target_pos, Quaternion.identity, Objects_parent.transform);
                cp.GetComponent<BoxCollider>().enabled = false;
                //cp.transform.position = target_pos;// new Vector3(0, 0, 0);
                //cp.GetComponent<iconicElementScript>().calculateTranslationPath();
                cp.GetComponent<iconicElementScript>().edge_position = toCopy.GetComponent<iconicElementScript>().edge_position + target_pos;

                Paintable.totalLines++;
                cp.GetComponent<iconicElementScript>().icon_number = Paintable.totalLines;
                cp.GetComponent<iconicElementScript>().icon_name = "iconic_" + Paintable.totalLines.ToString();

                // needs a unique name in the object hierarchy
                cp.name = "iconic_" + (Paintable.totalLines).ToString();
            }

            
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(copy_touch.position.ToString() + ", phase: " + copy_touch.phase.ToString());

        
        if (transform.GetComponent<AllButtonsBehaviors>().selected && !start_copying && PenTouchInfo.PressedThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
            RaycastHit Hit;

            if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "iconic")
            {
                //Debug.Log("copy_started");
                toCopy = Hit.collider.gameObject;
                start_copying = true;
            }
        }
        else if (transform.GetComponent<AllButtonsBehaviors>().selected && start_copying && PenTouchInfo.PressedNow)
        {
            //Debug.Log("copy_continued");
            copyAlongPath(PenTouchInfo.penPosition);

            // stop panning capabilities as we copy on top of paintable canvas
            paint_canvas.GetComponent<Paintable>().okayToPan = false;

            // DONE: DISABLE COLLIDERS ON ALL COPIED OBJECTS UNTIL TOUCH IS UP, OTHERWISE THE checkHitAndMove() ON EACH COPIED PEN OBJECT STARTS SHIFTING THE MENU 
        }
        else if (transform.GetComponent<AllButtonsBehaviors>().selected && start_copying && PenTouchInfo.ReleasedThisFrame)
        {
            start_copying = false;
            copy_path.Clear();

            // re-enable the pan capability for paint canvas
            paint_canvas.GetComponent<Paintable>().okayToPan = true;

            //GameObject.Find("InputTouches").GetComponent<TapDetector>().enabled = true;

            // enable all copied box colliders
            GameObject[] copies = GameObject.FindGameObjectsWithTag("iconic");

            for (int i = 0; i < copies.Length; i++)
            {
                if (copies[i].name.Contains(toCopy.name))
                {
                    copies[i].GetComponent<BoxCollider>().enabled = true;
                }
            }

           
            //Debug.Log("iconic copying ended.");
        }
    }
}