using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorPickerClickCheck : MonoBehaviour
{
    public static bool Rpointer;
    public static bool Gpointer;
    public static bool Bpointer;
    public static bool Apointer;
    public static bool previewpointer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject(0))
        {
            if (this.name == "RPicker")
                Rpointer = true;
            else Rpointer = false;

            if (this.name == "GPicker")
                Gpointer = true;
            else Gpointer = false;

            if (this.name == "BPicker")
                Bpointer = true;
            else Bpointer = false;

            if (this.name == "APickerBackground")
                Apointer = true;
            else Apointer = false;

            if (this.name == "ColorPreview")
                previewpointer = true;
            else previewpointer = false;            
        }
        else
        {
            Rpointer = false;
            Gpointer = false;
            Bpointer = false;
            Apointer = false;
            previewpointer = false;
        }
    }
}
