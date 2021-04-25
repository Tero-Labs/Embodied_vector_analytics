using Jobberwocky.GeometryAlgorithms.Examples;
using Jobberwocky.GeometryAlgorithms.Examples.Data;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleGeometry3D : ExampleGeometryAlgorithms
{
    // Colors defined by the user
    public Color pointColor;
    public Color wireframeColor;
    public Color polygonColor;

    // The mesh used ti visualize spheres and cylinders
    public Mesh cylinderMesh;
    public Mesh sphereMesh;

    // The type of material used to visualize all the geometries
    public Material material;

    // Game objects used to visualize the results
    private GameObject GeometryObject;
    private GameObject Triangles;
    private GameObject Lines;
    private GameObject Points;

    // Materials used for the visualization of the game objects
    private Material pointMaterial;
    private Material wireframeMaterial;
    private Material polygonMaterial;

    // Available algorithms
    private List<Dropdown.OptionData> optionsAlgorithms = new List<Dropdown.OptionData> {
        new Dropdown.OptionData("Triangulation"),
        new Dropdown.OptionData("Convex hull"),
        new Dropdown.OptionData("Voronoi diagram"),
    };

    // Available data
    private List<Dropdown.OptionData> optionsData = new List<Dropdown.OptionData> {
        new Dropdown.OptionData("Cube"),
        new Dropdown.OptionData("Random"),
        new Dropdown.OptionData("Sphere"),
    };

    // Assign shape to each data string
    private Dictionary<string, ShapeType> shapeTypes = new Dictionary<string, ShapeType>()
    {
        { "Cube", ShapeType.Cube },
        { "Random", ShapeType.Random3D },
        { "Sphere", ShapeType.Sphere },
    };

    // Use this for initialization
    void Start()
    {
        // Assign the dropdown data to the dropdowns
        var dropdownData = GetDropdown("DropdownData");
        var dropdownAlgorithm = GetDropdown("DropdownAlgorithm");

        dropdownData.AddOptions(optionsData);
        dropdownAlgorithm.AddOptions(optionsAlgorithms);

        // Add event listeners to the dropdowns
        dropdownData.onValueChanged.AddListener(delegate
        {
            UpdateGeometry(dropdownData.captionText.text, dropdownAlgorithm.captionText.text);
        });

        dropdownAlgorithm.onValueChanged.AddListener(delegate
        {
            UpdateGeometry(dropdownData.captionText.text, dropdownAlgorithm.captionText.text);
        });

        //Create the materials
        pointMaterial = new Material(material);
        pointMaterial.SetColor("_Color", pointColor);
        wireframeMaterial = new Material(material);
        wireframeMaterial.SetColor("_Color", wireframeColor);
        polygonMaterial = new Material(material);
        polygonMaterial.SetColor("_Color", polygonColor);

        // Setup gameobjects
        GeometryObject = new GameObject("Geometry Object");
        GeometryObject.transform.parent = gameObject.transform;
        Triangles = new GameObject("Triangles");
        Triangles.transform.parent = GeometryObject.transform;
        Triangles.AddComponent<MeshFilter>().mesh = new Mesh();
        Triangles.AddComponent<MeshRenderer>().material = polygonMaterial;

        UpdateGeometry("Cube", "Triangulation");
    }

    /// <summary>
    /// Creates the triangulation for a certain shape
    /// </summary>
    /// <param name="shape"></param>
    private void CreateTriangulation(Shape shape)
    {
        var parameters = new Triangulation3DParameters() { Points = shape.GetAllPoints(), BoundaryOnly = true };

        var triangulationAPI = new TriangulationAPI();
        var mesh = triangulationAPI.Triangulate3D(parameters);
        Triangles.GetComponent<MeshFilter>().mesh = mesh;
        // Visualize the results
        var scalePoints = Mathf.Abs(shape.CameraPoint.z / 45f);
        var scaleWireframe = Mathf.Abs(shape.CameraPoint.z / 150f);
        CreatePointSpheres(mesh.vertices, scalePoints, sphereMesh, pointMaterial, Points);
        CreateWireframe(mesh, scaleWireframe, cylinderMesh, wireframeMaterial, Lines);
    }

    /// <summary>
    /// Creates the hull for a certain shape
    /// </summary>
    /// <param name="shape"></param>
    private void CreateHull(Shape shape)
    {
        var points = shape.GetAllPoints();
        var parameters = new Hull3DParameters() { Points = points };

        var hullAPI = new HullAPI();
        var mesh = hullAPI.ConvexHull3D(parameters);
        Triangles.GetComponent<MeshFilter>().mesh = mesh;
        // Visualize the results
        var scalePoints = Mathf.Abs(shape.CameraPoint.z / 45f);
        var scaleWireframe = Mathf.Abs(shape.CameraPoint.z / 150f);
        CreatePointSpheres(points, scalePoints, sphereMesh, pointMaterial, Points);
        CreateWireframe(mesh, scaleWireframe, cylinderMesh, wireframeMaterial, Lines);
    }

    private void CreateVoronoi(Shape shape)
    {
        Triangles.GetComponent<MeshFilter>().mesh = new Mesh();

        var points = shape.GetAllPoints();
        var parameters = new Voronoi3DParameters() { Points = points };

        var voronoiAPI = new VoronoiAPI();
        var mesh = voronoiAPI.Voronoi3D(parameters);
        // Visualize the results
        var scalePoints = Mathf.Abs(shape.CameraPoint.z / 45f);
        var scaleWireframe = Mathf.Abs(shape.CameraPoint.z / 150f);
        CreatePointSpheres(points, scalePoints, sphereMesh, pointMaterial, Points);

        Lines.AddComponent<MeshFilter>().mesh = mesh;
        Lines.AddComponent<MeshRenderer>().material = wireframeMaterial;
        //CreateWireframe(mesh, scaleWireframe, cylinderMesh, wireframeMaterial, Lines);
    }

    /// <summary>
    /// Given the dataName and algorithmName the geometry is updated with the corresponding shape and method
    /// </summary>
    /// <param name="dataName"></param>
    /// <param name="algorithmName"></param>
    private void UpdateGeometry(string dataName, string algorithmName)
    {
        Destroy(Points);
        Destroy(Lines);
        
        Points = new GameObject("Points");
        Lines = new GameObject("Lines");

        Points.transform.parent = GeometryObject.transform;
        Lines.transform.parent = GeometryObject.transform;

        var shape = Data.Get(shapeTypes[dataName]);
        switch (algorithmName)
        {
            case "Triangulation":
                CreateTriangulation(shape);
                break;
            case "Convex hull":
                CreateHull(shape);
                break;
            case "Voronoi diagram":
                CreateVoronoi(shape);
                break;
        }
        // Set the camera correctly so that the shape is visible
        Camera.main.transform.position = shape.CameraPoint;
        Camera.main.transform.rotation = shape.CameraRotation;
    }

    /// <summary>
    /// Get Dropdown object based on its name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private Dropdown GetDropdown(string name)
    {
        var go = GameObject.Find(name);
        var dropdown = go.GetComponent<Dropdown>();

        return dropdown;
    }
}
