using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorFieldElement : MonoBehaviour
{
    public int width, magnitude;
    public float offset;
    public Vector3 last_drawn_pos;
    public List<Vector3> points = new List<Vector3>();
    public bool drawn;

    // Start is called before the first frame update
    void Start()
    {
        /*width = 1;
        magnitude = 50;*/
        offset = 30;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void VectorCreation()
    {

    }
}
