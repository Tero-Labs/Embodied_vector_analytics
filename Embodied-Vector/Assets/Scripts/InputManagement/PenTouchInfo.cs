using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class PenTouchInfo : MonoBehaviour
{

	Pen currentpen;

	public static bool PressedThisFrame, PressedNow, ReleasedThisFrame;

	public static Vector2 penPosition;

	public static float pressureValue;

	public static bool isPen;

	// Update is called once per frame
	void Update()
	{
		currentpen = Pen.current;

		if ((currentpen != null && currentpen.tip.wasPressedThisFrame) ||
			Input.GetMouseButtonDown(0))
		{
			PressedThisFrame = true;
		}
		else
		{
			PressedThisFrame = false;
		}

		if ((currentpen != null && currentpen.tip.wasReleasedThisFrame) ||
			Input.GetMouseButtonUp(0))
		{
			ReleasedThisFrame = true;
		}
		else
		{
			ReleasedThisFrame = false;
		}

		if ((currentpen != null && currentpen.tip.isPressed) ||
			Input.GetMouseButton(0))
		{
			PressedNow = true;
		}
		else
		{
			PressedNow = false;
		}

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButton(0))
		{
            penPosition = (Vector2)Input.mousePosition;
			isPen = false;
			pressureValue = 1;
		}
		else if (currentpen != null && (currentpen.tip.isPressed || currentpen.tip.wasPressedThisFrame || currentpen.tip.wasReleasedThisFrame))
		{
			penPosition = currentpen.position.ReadValue();
			isPen = true;
			pressureValue = currentpen.pressure.ReadValue();
		}
	}
}

