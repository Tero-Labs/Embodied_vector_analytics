using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;

public class CopyPenObject : MonoBehaviour
{
	public bool start_copying = false;

	//public Vector3 start_location;
	//public Vector3 last_copied_location;

	public GameObject toCopy;
	public GameObject paint_canvas;
	public GameObject newedge = null;

	//public int copy_count = 0;
	public List<Vector3> copy_path = new List<Vector3>();

	Pen currentPen;

	// Passed along long-press touch from copy menu button
	//public Touch copy_touch;
	//public Tap longTap;

	private void Awake()
	{
        /*
		EnhancedTouchSupport.Enable();

		paint_canvas = GameObject.FindGameObjectWithTag("paintable_canvas_object");*/
	}

	private void OnDestroy()
	{
		//EnhancedTouchSupport.Disable();
	}

	public void copyAlongPath(Vector2 touchpos)
	{
        /*
		var ray = Camera.main.ScreenPointToRay(touchpos);
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

		if (start_copying && copy_path.Count > 0)
		{
			// if the dragged distance is not too low, or to prevent adding objects when touch is stationary
			if (Vector3.Distance(copy_path[copy_path.Count - 1], pos) > 25)
			{
				copy_path.Add(pos);

				GameObject cp = Instantiate(toCopy, pos, Quaternion.identity, toCopy.transform.parent);
				cp.GetComponent<BoxCollider>().enabled = false;
				cp.GetComponent<penLine_script>().calculateTranslationPath();

				// find any edgeline associated with this object and create a copy too
				GameObject[] edges = GameObject.FindGameObjectsWithTag("edgeline");
				for (int k = 0; k < edges.Length; k++)
				{
					if (edges[k].GetComponent<edgeLine_script>().target_object == toCopy)
					{
						newedge = Instantiate(edges[k], GameObject.Find("Paintable").transform);
						// change the target object
						newedge.GetComponent<edgeLine_script>().target_object = cp;
						// don't update the target object, let it sit at the position it was copied to
						// newedge.GetComponent<edgeLine_script>().target_is_being_copied = true;

						break;
					}
				}

				// rename to include index in the copies
				cp.GetComponent<penLine_script>()._name += copy_path.Count.ToString();
				cp.GetComponent<penLine_script>().symbol_name = new Symbol(cp.GetComponent<penLine_script>()._name);
				// needs a unique name in the object hierarchy
				cp.name = "penLine_" + (++paint_canvas.GetComponent<Paintable>().totalLines).ToString();
			}
		}
		else if(start_copying && copy_path.Count == 0)
		{
			// add an offset upwards for the first copy
			pos += new Vector3(0, 20, 0);

			copy_path.Add(pos);

			GameObject cp = Instantiate(toCopy, pos, Quaternion.identity, toCopy.transform.parent);
			cp.GetComponent<BoxCollider>().enabled = false;
			cp.GetComponent<penLine_script>().calculateTranslationPath();

			// find any edgeline associated with this object and create a copy too
			GameObject[] edges = GameObject.FindGameObjectsWithTag("edgeline");
			for (int k = 0; k < edges.Length; k++)
			{
				if (edges[k].GetComponent<edgeLine_script>().target_object == toCopy)
				{
					GameObject newedge = Instantiate(edges[k], GameObject.Find("Paintable").transform);
					// change the target object
					newedge.GetComponent<edgeLine_script>().target_object = cp;
					// don't update the target object, let it sit at the position it was copied to
					// newedge.GetComponent<edgeLine_script>().target_is_being_copied = true;

					break;
				}
			}

			// rename to include index in the copies
			cp.GetComponent<penLine_script>()._name += copy_path.Count.ToString();
			cp.GetComponent<penLine_script>().symbol_name = new Symbol(cp.GetComponent<penLine_script>()._name);
			// needs a unique name in the object hierarchy
			cp.name = "penLine_" + (++paint_canvas.GetComponent<Paintable>().totalLines).ToString();
		}*/
	}

    // Update is called once per frame
    void Update()
    {
		//Debug.Log(copy_touch.position.ToString() + ", phase: " + copy_touch.phase.ToString());
        /*
		var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

		currentPen = Pen.current;*/


		// ========================Touch based copying==========================
		/*
		if (start_copying && (activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began ||
			activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Moved))
		{
			copyAlongPath(activeTouches[0].screenPosition);

			// stop panning capabilities as we copy on top of paintable canvas
			paint_canvas.GetComponent<Paintable_Script>().okayToPan = false;

			// DONE: DISABLE COLLIDERS ON ALL COPIED OBJECTS UNTIL TOUCH IS UP, OTHERWISE THE checkHitAndMove() ON EACH COPIED PEN OBJECT STARTS SHIFTING THE MENU 
		}
		else if (start_copying && activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended)
		{
			start_copying = false;
			copy_path.Clear();
			
			// re-enable the pan capability for paint canvas
			paint_canvas.GetComponent<Paintable_Script>().okayToPan = true;

			//GameObject.Find("InputTouches").GetComponent<TapDetector>().enabled = true;

			// enable all copied box colliders
			GameObject[] copies = GameObject.FindGameObjectsWithTag("penline");

			for (int i = 0; i < copies.Length; i++)
			{
				if (copies[i].name.Contains(toCopy.name))
				{
					copies[i].GetComponent<BoxCollider>().enabled = true;
				}
			}

			Debug.Log("copying ended.");
		}
		*/

		// ========================Pen based copying===========================

		/*if (transform.GetComponent<AllButtonsBehavior>().selected && !start_copying && PenTouchInfo.PressedThisFrame)
		{
			var ray = Camera.main.ScreenPointToRay(PenTouchInfo.penPosition);
			RaycastHit Hit;
			Vector3 pos;

			if (Physics.Raycast(ray, out Hit) && Hit.collider.gameObject.tag == "penline")
			{
				//pos = Hit.point + new Vector3(0, 0, -40);

				toCopy = Hit.collider.gameObject;
				start_copying = true;
			}
		}
		else if (transform.GetComponent<AllButtonsBehavior>().selected && start_copying && PenTouchInfo.PressedNow)
		{
			copyAlongPath(PenTouchInfo.penPosition);

			// stop panning capabilities as we copy on top of paintable canvas
			paint_canvas.GetComponent<Paintable_Script>().okayToPan = false;

			// DONE: DISABLE COLLIDERS ON ALL COPIED OBJECTS UNTIL TOUCH IS UP, OTHERWISE THE checkHitAndMove() ON EACH COPIED PEN OBJECT STARTS SHIFTING THE MENU 
		}
		else if (transform.GetComponent<AllButtonsBehavior>().selected && start_copying && PenTouchInfo.ReleasedThisFrame)
		{
			start_copying = false;
			copy_path.Clear();

			// re-enable the pan capability for paint canvas
			paint_canvas.GetComponent<Paintable_Script>().okayToPan = true;

			//GameObject.Find("InputTouches").GetComponent<TapDetector>().enabled = true;

			// enable all copied box colliders
			GameObject[] copies = GameObject.FindGameObjectsWithTag("penline");

			for (int i = 0; i < copies.Length; i++)
			{
				if (copies[i].name.Contains(toCopy.name))
				{
					copies[i].GetComponent<BoxCollider>().enabled = true;
				}
			}*/

			/*
			// at this point, the copied objects can be acted on (updateFeature) by the edge lines (if any)
			if (newedge != null)
			{
				GameObject[] edges = GameObject.FindGameObjectsWithTag("edgeline");
				for (int i = 0; i < edges.Length; i++)
				{
					if (edges[i].name.Contains(newedge.name))
					{
						edges[i].GetComponent<edgeLine_script>().target_is_being_copied = false;
					}
				}
			}
			*/

			//Debug.Log("penline copying ended.");
		//}
	}
}
