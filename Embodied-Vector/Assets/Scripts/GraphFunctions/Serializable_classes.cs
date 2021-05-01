using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

// useful links
/* https://allison-liem.medium.com/unity-reading-external-json-files-878ed0978977 */
/* https://medium.com/@pudding_entertainment/unity-how-to-save-and-load-files-using-json-and-base64-a033def09a47 */
/* https://allison-liem.medium.com/unity-reading-external-json-files-878ed0978977 */
/* https://github.com/Enigo/UnityEasyFileSaving/blob/master/Assets/Scripts/DataHandler.cs */

[Serializable]
public class Edge
{
    public int edge_start;
    public int edge_end;
    public int weight;
}

[Serializable]
public class Graph
{
    public List<Edge> edges;
    public List<int> nodes;
}

[Serializable]
public class Graphs
{
    public List<Graph> graphs;
}

[Serializable]
public class Vector
{
    public int x;
    public int y;
    public float f_x;
    public float f_y;
}

[Serializable]
public class VectorField
{
    public int gridmaxx, gridmaxy, gridminx, gridminy;
    public List<Vector> vectors;
}

[Serializable]
public class single_cell
{
    public int x;
    public int y;
}

[Serializable]
public class single_node_cord
{
    public int node_id;
    public string x;
    public string y;
}

[Serializable]
public class GraphCordinate
{
    public List<single_node_cord> node_cord;
}

public class JsonHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Graph graph = JsonUtility.FromJson<Graph>(File.ReadAllText("Assets/Resources/" + "output.json"));
        //Debug.Log(JsonUtility.ToJson(graph));
    }

    void Sample()
    {
        Edge edge_1 = new Edge();
        edge_1.edge_start = 1;
        edge_1.edge_end = 2;
        edge_1.weight = 3;

        Graph graph_1 = new Graph();
        graph_1.edges = new List<Edge>();
        graph_1.nodes = new List<int>();

        graph_1.edges.Add(edge_1);
        graph_1.nodes.Add(10);

        Graphs graphs = new Graphs();
        graphs.graphs = new List<Graph>();
        graphs.graphs.Add(graph_1);
        graphs.graphs.Add(graph_1);
        graphs.graphs.Add(graph_1);

        SaveData(graphs, "data.json");
        Graphs new_graphs = LoadData("data.json");
        Debug.Log(new_graphs.graphs[0].nodes[0].ToString() + " could read");
    }

    void SaveData(Graphs graphs, string filename)
    {
        Debug.Log(JsonUtility.ToJson(graphs));
        File.WriteAllText("Assets/Resources/" + filename, JsonUtility.ToJson(graphs));
    }

    Graphs LoadData(string filename)
    {
        return JsonUtility.FromJson<Graphs>(File.ReadAllText("Assets/Resources/" + filename));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
