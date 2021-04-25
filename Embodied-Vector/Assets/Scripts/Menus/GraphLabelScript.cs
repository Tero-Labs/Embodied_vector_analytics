using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphLabelScript : MonoBehaviour
{
    public TMP_Text tmptextlabel;
    bool draggable_now;
    bool menu_create;
    private Vector3 touchDelta = new Vector3();
    private Vector3 prevpos;
    private Vector3 menupos;
    public Image img;
    public GameObject paintable;

    // Start is called before the first frame update
    void Start()
    {
        paintable = transform.parent.GetComponent<GraphElementScript>().paintable;
        menupos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!(paintable.GetComponent<Paintable>().panZoomLocked))
        //if (paintable.GetComponent<Paintable>().pan_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            checkHitAndMove();
        }
    }

    public void checkHitAndMove()
    {

        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition, 
                transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera))
            {
                Debug.Log("started drag");

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = transform.GetChild(0).position - vec;
                // change anchor color
                img.color = Color.gray;
                prevpos = transform.GetChild(0).position;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow)
        {

            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                transform.parent.GetComponent<GraphElementScript>().paintable.GetComponent<Paintable>().main_camera))
            {
                //Debug.Log(transform.name);

                draggable_now = true;                
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    paintable.GetComponent<Paintable>().main_camera, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - transform.GetChild(0).position + touchDelta;
                diff.z = 0;

                // don't move right away, move if a threshold has been crossed
                // 5 seems to work well in higher zoom levels and for my finger
                // update the function position.               

                menupos += diff;
                transform.parent.position += diff;
                transform.parent.GetComponent<GraphElementScript>().checkHitAndMove(diff);
            }

        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)//(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && draggable_now)
        {            
            if (Vector3.Distance(prevpos, transform.GetChild(0).position)<5f)
            {
                if (paintable.GetComponent<Paintable>().canvas_radial.transform.childCount > 0)
                {
                    for (int i = 0; i < paintable.GetComponent<Paintable>().canvas_radial.transform.childCount; i++)
                    {
                        Destroy(paintable.GetComponent<Paintable>().canvas_radial.transform.GetChild(i).gameObject);
                    }
                }

                transform.parent.GetComponent<GraphElementScript>().createMenu(transform.position);
            }

            draggable_now = false;
            menu_create = false;

            touchDelta = new Vector3(); // reset touchDelta
            img.color = new Color32(125, 255, 165, 255);            
        }

    }
}

