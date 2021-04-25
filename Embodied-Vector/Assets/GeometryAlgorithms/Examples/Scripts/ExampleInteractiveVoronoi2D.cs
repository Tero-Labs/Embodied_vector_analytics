using Jobberwocky.GeometryAlgorithms.Examples;
using Jobberwocky.GeometryAlgorithms.Examples.Data;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using System.Collections.Generic;
using UnityEngine;

public class ExampleInteractiveVoronoi2D : ExampleGeometryAlgorithms
{
    public Mesh pointMesh;
    public Mesh lineMesh;

    public Color pointColor;
    public Color triangulationLineColor;
    public Color voronoiLineColor;
    public Color voronoiCellColor;

    public Material material;

    private Material pointMaterial;
    private Material triangulationLineMaterial;
    private Material voronoiLineMaterial;
    private Material voronoiCellMaterial;

    private Vector3[] data;

    private GameObject mousePosition;
    private GameObject voronoiLines;
    private GameObject triangulationLines;
    private GameObject voronoiCell;

    private VoronoiAPI voronoiAPI;
    private TriangulationAPI triangulationAPI;

    private List<GameObject> triLineObjects;
    private List<GameObject> voronoiLineObjects;

    // Use this for initialization
    void Start()
    {
        //Create the materials
        pointMaterial = new Material(material);
        pointMaterial.SetColor("_Color", pointColor);
        triangulationLineMaterial = new Material(material);
        triangulationLineMaterial.SetColor("_Color", triangulationLineColor);
        voronoiLineMaterial = new Material(material);
        voronoiLineMaterial.SetColor("_Color", voronoiLineColor);
        voronoiCellMaterial = new Material(material);
        voronoiCellMaterial.SetColor("_Color", voronoiCellColor);

        // Set the lists to store the various game objects
        triLineObjects = new List<GameObject>();
        voronoiLineObjects = new List<GameObject>();

        var points = new GameObject("Points");
        points.transform.parent = gameObject.transform;

        var shapeGenerator = new ShapeGenerator();
        var scale = 0.08f;
        var tempData = shapeGenerator.CreateRandomPoints2D(200, 35, 20);
        CreatePointSpheres(tempData, scale, pointMesh, pointMaterial, points);

        data = new Vector3[tempData.Length + 1];
        for (int i = 0; i < tempData.Length; i++)
        {
            data[i + 1] = tempData[i];
        }

        data[0] = new Vector3(0, 0);

        triangulationAPI = new TriangulationAPI();
        var triangulation = triangulationAPI.Triangulate2D(new Triangulation2DParameters() { Points = data, Side = Side.Back });
        triangulationLines = new GameObject("Triangulation Lines");
        triangulationLines.transform.parent = gameObject.transform;
        CreateLineCylinders(CreateWireframe(triangulation), triangulationLineMaterial, 0.03f, triangulationLines, triLineObjects);

        voronoiAPI = new VoronoiAPI();
        var voronoi = voronoiAPI.Voronoi2DRaw(new Voronoi2DParameters() { Points = data });
        voronoiLines = new GameObject("Voronoi Lines");
        voronoiLines.transform.parent = gameObject.transform;
        CreateLineCylinders(voronoi.ToUnityMesh(), voronoiLineMaterial, 0.05f, voronoiLines, voronoiLineObjects);

        var voronoiCellMesh = triangulationAPI.Triangulate2D(new Triangulation2DParameters() { Points = voronoi.Cells[0].ToUnityMesh().vertices, Side = Side.Back });
        voronoiCell = new GameObject("Voronoi Cell");
        voronoiCell.transform.parent = gameObject.transform;
        voronoiCell.AddComponent<MeshFilter>().mesh = voronoiCellMesh;
        voronoiCell.AddComponent<MeshRenderer>().material = voronoiCellMaterial;

        mousePosition = new GameObject("Mouse Position");
        mousePosition.transform.parent = gameObject.transform;
        mousePosition.transform.localScale = new Vector3(scale, scale, scale);
        mousePosition.AddComponent<MeshFilter>().mesh = pointMesh;
        mousePosition.AddComponent<MeshRenderer>().material = pointMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        var threshold = 0.01;
        var xMovement = Input.GetAxis("Mouse X");
        var yMovement = Input.GetAxis("Mouse Y");

        if (xMovement - threshold > 0 || xMovement + threshold < 0 || yMovement - threshold > 0 || yMovement + threshold < 0)
        {
            var mouseScreenPosition = Input.mousePosition;

            if ((mouseScreenPosition.x > 0 && mouseScreenPosition.y > 0) && (mouseScreenPosition.x < Screen.width && mouseScreenPosition.y < Screen.height))
            {
                var camera = Camera.main;
                mouseScreenPosition.z = Mathf.Abs(camera.transform.position.z);
                var p = camera.ScreenToWorldPoint(mouseScreenPosition);

                mousePosition.transform.localPosition = p;
                data[0] = p;

                var voronoi = voronoiAPI.Voronoi2DRaw(new Voronoi2DParameters() { Points = data });
                CreateLineCylinders(voronoi.ToUnityMesh(), voronoiLineMaterial, 0.05f, voronoiLines, voronoiLineObjects);
                CreateLineCylinders(CreateWireframe(triangulationAPI.Triangulate2D(new Triangulation2DParameters() { Points = data, Side = Side.Back })), triangulationLineMaterial, 0.03f, triangulationLines, triLineObjects);
                voronoiCell.GetComponent<MeshFilter>().mesh = triangulationAPI.Triangulate2D(new Triangulation2DParameters() { Points = voronoi.Cells[0].ToUnityMesh().vertices, Side = Side.Back });
            }
        }
    }

    /// <summary>
    /// Creates the line cylinders from a mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <param name="material"></param>
    /// <param name="scale"></param>
    /// <param name="parent"></param>
    /// <param name="existingObjects"></param>
    private void CreateLineCylinders(Mesh mesh, Material material, float scale, GameObject parent, List<GameObject> existingObjects)
    {
        var vertices = mesh.vertices;
        var indices = mesh.GetIndices(0);
        for (int i = 0; i < indices.Length; i += 2)
        {
            var startVertex = vertices[indices[i]];
            var endVertex = vertices[indices[i + 1]];

            GameObject cylinder;
            if (i / 2 < existingObjects.Count)
            {
                cylinder = existingObjects[i / 2];
            }
            else
            {
                cylinder = new GameObject(parent.name + " Cylinder " + i);
                cylinder.transform.parent = parent.transform;
                cylinder.AddComponent<MeshFilter>();
                cylinder.AddComponent<MeshRenderer>().material = material;
                existingObjects.Add(cylinder);
            }

            cylinder.transform.localPosition = (endVertex - startVertex) / 2.0f + startVertex;
            cylinder.transform.localScale = new Vector3(scale, (endVertex - startVertex).magnitude / 2.0f, scale);
            cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, (endVertex - startVertex));
            cylinder.SetActive(true);

            cylinder.GetComponent<MeshFilter>().mesh = lineMesh;
        }

        for (int i = indices.Length / 2; i < existingObjects.Count; i++)
        {
            existingObjects[i].SetActive(false);
        }
    }

    /// <summary>
    /// Method that generates the wireframe for a given mesh
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    private Mesh CreateWireframe(Mesh mesh)
    {
        var indices = mesh.GetIndices(0);
        var wireframeIndices = new int[indices.Length * 2];
        for (var i = 0; i < indices.Length; i += 3)
        {
            wireframeIndices[i * 2 + 0] = indices[i];
            wireframeIndices[i * 2 + 1] = indices[i + 1];
            wireframeIndices[i * 2 + 2] = indices[i + 1];
            wireframeIndices[i * 2 + 3] = indices[i + 2];
            wireframeIndices[i * 2 + 4] = indices[i + 2];
            wireframeIndices[i * 2 + 5] = indices[i];
        }

        var meshWireframe = new Mesh();
        meshWireframe.vertices = mesh.vertices;
        meshWireframe.SetIndices(wireframeIndices, MeshTopology.Lines, 0);

        return meshWireframe;
    }
}
