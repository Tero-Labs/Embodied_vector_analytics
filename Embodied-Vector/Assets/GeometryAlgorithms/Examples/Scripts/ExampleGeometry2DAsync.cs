using Jobberwocky.GeometryAlgorithms.Examples.Data;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jobberwocky.GeometryAlgorithms.Examples
{
    public class ExampleGeometry2DAsync : ExampleGeometryAlgorithms
    {
        // Colors defined by the user
        public Color pointColor;
        public Color wireframeColor;
        public Color boundaryColor;
        public Color polygonColor;

        // The mesh used ti visualize spheres and cylinders
        public Mesh cylinderMesh;
        public Mesh sphereMesh;

        // Sets the standard material used to visualize all geometry
        public Material material;

        // Game objects used to visualize the results
        private GameObject GeometryObject;
        private GameObject Points;
        private GameObject Lines;
        private GameObject Boundary;
        private GameObject Triangles;

        // Materials used for the visualization of the game objects
        private Material pointMaterial;
        private Material wireframeMaterial;
        private Material boundaryMaterial;
        private Material polygonMaterial;

        // Available algorithms
        private List<Dropdown.OptionData> optionsAlgorithms = new List<Dropdown.OptionData> {
            new Dropdown.OptionData("Triangulation"),
            new Dropdown.OptionData("Hull"),
        };

        // Available data
        private List<Dropdown.OptionData> optionsData = new List<Dropdown.OptionData> {
            new Dropdown.OptionData("Horse"),
            new Dropdown.OptionData("Owl"),
        };

        // Assign shape to each data string
        private Dictionary<string, ShapeType> shapeTypes = new Dictionary<string, ShapeType>()
        {
            { "Horse", ShapeType.Horse13k },
            { "Owl", ShapeType.Owl15k },
        };

        // The API objects
        private TriangulationAPI triangulationAPI;
        private HullAPI hullAPI;

        // Use this for initialization
        void Start()
        {
            triangulationAPI = new TriangulationAPI();
            hullAPI = new HullAPI();

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
            boundaryMaterial = new Material(material);
            boundaryMaterial.SetColor("_Color", boundaryColor);
            polygonMaterial = new Material(material);
            polygonMaterial.SetColor("_Color", polygonColor);

            // Setup gameobjects
            GeometryObject = new GameObject("Geometry Object");
            GeometryObject.transform.parent = gameObject.transform;
            Triangles = new GameObject("Triangles");
            Triangles.transform.parent = GeometryObject.transform;
            Triangles.AddComponent<MeshFilter>().mesh = new Mesh();
            Triangles.AddComponent<MeshRenderer>().material = polygonMaterial;
            Lines = new GameObject("Lines");
            Lines.transform.parent = GeometryObject.transform;
            Lines.AddComponent<MeshFilter>().mesh = new Mesh();
            Lines.AddComponent<MeshRenderer>().material = wireframeMaterial;
            Points = new GameObject("Points");
            Points.transform.parent = GeometryObject.transform;
            Points.AddComponent<MeshFilter>().mesh = new Mesh();
            Points.AddComponent<MeshRenderer>().material = pointMaterial;

            UpdateGeometry("Horse", "Triangulation");
        }


        void Update()
        {
            // In the update function we check whether there is a callback available from one of the async methods
            // This is IMPORTANT, because else the callback defined in the async method will not trigger!!!
            triangulationAPI.ActivateCallbacks();
            hullAPI.ActivateCallbacks();
        }

        /// <summary>
        /// Creates the triangulation for a certain shape
        /// </summary>
        /// <param name="shape"></param>
        private void CreateTriangulation(Shape shape)
        {
            hullAPI.Hull2DAsync((geometryHull) =>
            {
                var hull = geometryHull.ToUnityMesh();
                triangulationAPI.Triangulate2DAsync((geometry) =>
                {
                    var mesh = geometry.ToUnityMesh();
                    var wireframe = CreateWireframe(mesh);

                    Triangles.GetComponent<MeshFilter>().mesh = mesh;
                    Lines.GetComponent<MeshFilter>().mesh = wireframe;

                }, new Triangulation2DParameters() { Points = shape.Points, Boundary = hull.vertices, Side = Side.Back });
            }, new Hull2DParameters() { Points = shape.Points, Concavity = 30 });
            Camera.main.backgroundColor = new Color(238f / 255f, 89f / 255f, 108f / 255f);
        }

        /// <summary>
        /// Creates the hull for a certain shape
        /// </summary>
        /// <param name="shape"></param>
        private void CreateHull(Shape shape)
        {
            var points = shape.GetAllPoints();

            hullAPI.Hull2DAsync((geometryHull) =>
            {
                var hull = geometryHull.ToUnityMesh();
                triangulationAPI.Triangulate2DAsync((geometryMesh) =>
                {
                    var mesh = geometryMesh.ToUnityMesh();
                    var pointIndices = new int[points.Length];
                    for (var i = 0; i < points.Length; i++)
                    {
                        pointIndices[i] = i;
                    }
                    var pointMesh = new Mesh();
                    pointMesh.vertices = points;
                    pointMesh.SetIndices(pointIndices, MeshTopology.Points, 0);
                    Points.GetComponent<MeshFilter>().mesh = pointMesh;
                    Triangles.GetComponent<MeshFilter>().mesh = mesh;
                    Lines.GetComponent<MeshFilter>().mesh = hull;

                }, new Triangulation2DParameters() { Boundary = hull.vertices, Side = Side.Back });
            }, new Hull2DParameters() { Points = points, Concavity = 30 });

            Camera.main.backgroundColor = new Color(55f / 255f, 189f / 255f, 175 / 255f);
        }

        /// <summary>
        /// Given the dataName and algorithmName the geometry is updated with the corresponding shape and method
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="algorithmName"></param>
        private void UpdateGeometry(string dataName, string algorithmName)
        {
            Triangles.GetComponent<MeshFilter>().mesh = new Mesh();
            Lines.GetComponent<MeshFilter>().mesh = new Mesh();
            Points.GetComponent<MeshFilter>().mesh = new Mesh();

            var shape = Data.Data.Get(shapeTypes[dataName]);
            switch (algorithmName)
            {
                case "Triangulation":
                    CreateTriangulation(shape);
                    break;
                case "Hull":
                    CreateHull(shape);
                    break;
                case "Voronoi":
                    break;
            }

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
}

