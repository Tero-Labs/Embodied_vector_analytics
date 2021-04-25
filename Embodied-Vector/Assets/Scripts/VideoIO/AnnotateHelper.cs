using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class bounds
{
    public int x;
    public int y;
}

[Serializable]
public class tracked_object
{
    public List<bounds> bounds;    
    public int id;
    public string name;
}

[Serializable]
public class frame
{
    public List<tracked_object> objects;
}

[Serializable]
public class frames
{
    public List<frame> all_frame;
    public int width;
    public int height;
    public string node_type;
}

[Serializable]
public class route
{
    public List<Edge> edges;
}

[Serializable]
public class all_route
{
    public List<route> edge_list;
}

public class AnnotateHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        /*frames frames = JsonUtility.FromJson<frames>(File.ReadAllText("Assets/Resources/" + "trimmed.json"));
        Debug.Log(JsonUtility.ToJson(frames.all_frame[0].objects[0]));*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
