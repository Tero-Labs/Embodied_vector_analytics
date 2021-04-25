using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class topoLabelScript : MonoBehaviour
{
    public GameObject parent;
    public TMP_Text tmptextlabel;
    public TMP_Text functiontextlabel;
    bool draggable_now;
    private Vector3 touchDelta = new Vector3();
    private Vector3 prevpos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void checkHitAndMove()
    {

        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                Camera.main))
            {
                Debug.Log("started drag");

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    Camera.main, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;

                touchDelta = transform.position - vec;
                prevpos = transform.position;
            }

            else
            {
                return;
            }
        }

        else if (PenTouchInfo.PressedNow)
        {

            if (TMP_TextUtilities.IsIntersectingRectTransform(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                Camera.main))
            {
                Debug.Log("dragging: " + transform.name);

                draggable_now = true;
                Vector3 vec = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(tmptextlabel.rectTransform, PenTouchInfo.penPosition,
                    Camera.main, out vec);

                // enforce the same z coordinate as the rest of the points in the parent set object
                vec.z = -5f;
                Vector3 diff = vec - transform.position + touchDelta;                
                diff.z = 0;

                transform.position += diff;
            }

        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)//(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && draggable_now)
        {

            draggable_now = false;
            touchDelta = new Vector3(); // reset touchDelta
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent.tag == "function" && Paintable.pan_button.GetComponent<AllButtonsBehaviors>().selected)
        {
            checkHitAndMove();
        }

        if (PenTouchInfo.PressedThisFrame)
        {
            if (TMP_TextUtilities.IsIntersectingRectTransform
                    (tmptextlabel.rectTransform, PenTouchInfo.penPosition, Camera.main))
            {
                //Debug.Log("success");
                draggable_now = true;

                if (transform.parent.tag == "iconic" && transform.parent.parent.parent.parent.tag == "function")
                {
                    Paintable.dragged_arg_textbox = transform.parent.parent.parent.parent.gameObject;
                    Paintable.dragged_icon_name = transform.parent.name;
                }
                    
                // iconic label output of graph details
                else if (parent != null && parent.tag == "iconic") Paintable.dragged_arg_textbox = parent;
                
                // function scalar output
                else if (transform.parent.tag == "function")
                {
                    Paintable.dragged_arg_textbox = transform.gameObject;
                }

            }
        }

        else if (PenTouchInfo.ReleasedThisFrame && draggable_now)
        {
            draggable_now = false;
        }
    }
}
