using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CopyFunction : MonoBehaviour
{
	public bool start_copying = false;
	public GameObject toCopy;
	public List<Vector3> copy_path = new List<Vector3>();
	public float distance_threshold = 55f;

	public GameObject paint_canvas;

	Pen currentPen;

	private void Awake()
	{
		paint_canvas = GameObject.FindGameObjectWithTag("paintable_canvas_object");
	}

	public void copyAlongPath(Vector2 penpos)
	{
        /*
		var ray = Camera.main.ScreenPointToRay(penpos);
		RaycastHit Hit;
		Vector3 pos;

		if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "paintable_canvas_object")
		{
			pos = Hit.point + new Vector3(0, 0, -40);
		}
		else
		{
			return;
		}

		if (start_copying && copy_path.Count == 0)
		{
			// add an offset upwards for the first copy
			pos += new Vector3(0, 20, 0);

			copy_path.Add(pos);

			GameObject cp = Instantiate(toCopy, pos, Quaternion.identity, toCopy.transform.parent);
			cp.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;

			// rename to include index in the copies
			//cp.GetComponent<setLine_script>().this_set_name += copy_path.Count.ToString();
			
			// needs a unique name in the object hierarchy
			cp.name = "Function_" + (++paint_canvas.GetComponent<Paintable>().totalLines).ToString();

			// turn the abstraction layer to symbolic by default, to avoid visual clutter and members intersecting with other lists
			cp.GetComponent<functionLine_script>().retained_slider_value = 3;

			// set the current operator
			cp.GetComponent<functionLine_script>().current_op = toCopy.GetComponent<functionLine_script>().current_op;

			cp.GetComponent<functionLine_script>().updateFeature();
		}

		else if (start_copying && copy_path.Count > 0)
		{
			// if the dragged distance is not too low, or to prevent adding objects when touch is stationary
			if (Vector3.Distance(copy_path[copy_path.Count - 1], pos) > distance_threshold)
			{
				copy_path.Add(pos);

				GameObject cp = Instantiate(toCopy, pos, Quaternion.identity, toCopy.transform.parent);
				cp.transform.GetChild(0).GetComponent<BoxCollider>().enabled = false;

				// rename to include index in the copies
				//cp.GetComponent<setLine_script>().this_set_name += copy_path.Count.ToString();
				// needs a unique name in the object hierarchy
				cp.name = "Function_" + (++paint_canvas.GetComponent<Paintable>().totalLines).ToString();

				// turn the abstraction layer to symbolic by default, to avoid visual clutter and members intersecting with other lists
				cp.GetComponent<functionLine_script>().retained_slider_value = 3;

				// set the current operator
				cp.GetComponent<functionLine_script>().current_op = toCopy.GetComponent<functionLine_script>().current_op;

				cp.GetComponent<functionLine_script>().updateFeature();
			}
		}*/

	}

	// Update is called once per frame
	void Update()
	{
		//Debug.Log(copy_touch.position.ToString() + ", phase: " + copy_touch.phase.ToString());

		//currentPen = Pen.current;
        /*

		if (transform.GetComponent<AllButtonsBehavior>().selected && !start_copying && PenTouchInfo.PressedThisFrame)
		{
			var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
			RaycastHit Hit;

			if (Physics.Raycast(ray, out Hit) && Hit.collider.transform.parent.tag == "function")
			{
				toCopy = Hit.collider.transform.parent.gameObject;
				start_copying = true;
			}
		}

		else if (transform.GetComponent<AllButtonsBehavior>().selected && start_copying && PenTouchInfo.PressedNow)
		{
			copyAlongPath(PenTouchInfo.penPosition);

			// stop panning capabilities as we copy on top of paintable canvas
			paint_canvas.GetComponent<Paintable>().okayToPan = false;

			// DONE: DISABLE COLLIDERS ON ALL COPIED OBJECTS UNTIL TOUCH IS UP, OTHERWISE THE checkHitAndMove() ON EACH COPIED PEN OBJECT STARTS SHIFTING THE MENU 
		}
		else if (transform.GetComponent<AllButtonsBehavior>().selected && start_copying && PenTouchInfo.ReleasedThisFrame)
		{
			start_copying = false;
			copy_path.Clear();

			// re-enable the pan capability for paint canvas
			paint_canvas.GetComponent<Paintable>().okayToPan = true;

			//GameObject.Find("InputTouches").GetComponent<TapDetector>().enabled = true;

			// enable all copied sets' anchor box colliders
			GameObject[] copies = GameObject.FindGameObjectsWithTag("function");

			for (int i = 0; i < copies.Length; i++)
			{
				if (copies[i].name.Contains(toCopy.name))
				{
					copies[i].transform.GetChild(0).GetComponent<BoxCollider>().enabled = true;
				}
			}

			Debug.Log("function copying ended.");
		}*/
	}
}
