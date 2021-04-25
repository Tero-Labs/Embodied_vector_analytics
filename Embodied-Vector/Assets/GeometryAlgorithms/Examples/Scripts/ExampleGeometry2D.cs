using Jobberwocky.GeometryAlgorithms.Examples.Data;
using Jobberwocky.GeometryAlgorithms.Source.API;
using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jobberwocky.GeometryAlgorithms.Examples
{
    public class ExampleGeometry2D : ExampleGeometryAlgorithms
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
            new Dropdown.OptionData("Dude"),
            new Dropdown.OptionData("Bird"),
            new Dropdown.OptionData("Tank"),
            new Dropdown.OptionData("Circle"),
        };

        // Assign shape to each data string
        private Dictionary<string, ShapeType> shapeTypes = new Dictionary<string, ShapeType>()
        {
            { "Dude", ShapeType.Dude },
            { "Bird", ShapeType.Bird },
            { "Tank", ShapeType.Tank },
            { "Circle", ShapeType.Circle },
        };

        // Use this for initialization
        void Start()
        {
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

            UpdateGeometry("Dude", "Triangulation");
        }

        /// <summary>
        /// Creates the triangulation for a certain shape
        /// </summary>
        /// <param name="shape"></param>
        private void CreateTriangulation(Shape shape)
        {
            var parameters = new Triangulation2DParameters();
            parameters.Points = shape.Points;
            parameters.Boundary = shape.Boundary;
            parameters.Holes = shape.Holes;
            parameters.Side = Side.Back;
            parameters.Delaunay = true;

            var triangulationAPI = new TriangulationAPI();
            var mesh = triangulationAPI.Triangulate2D(parameters);
            Triangles.GetComponent<MeshFilter>().mesh = mesh;

            var scaleWireframe = Mathf.Abs(shape.CameraPoint.z / 350f);
            var scaleBoundary = Mathf.Abs(shape.CameraPoint.z / 250f);

            CreateWireframe(mesh, scaleWireframe, cylinderMesh, wireframeMaterial, Lines);
            CreateBoundaries(shape, scaleBoundary, cylinderMesh, boundaryMaterial, Boundary);

            Camera.main.backgroundColor = new Color(238f / 255f, 89f / 255f, 108f / 255f);

        }

        /// <summary>
        /// Creates the hull for a certain shape
        /// </summary>
        /// <param name="shape"></param>
        private void CreateHull(Shape shape)
        {
            var points = shape.GetAllPoints();

            var hullAPI = new HullAPI();
            var hull = hullAPI.Hull2D(new Hull2DParameters() { Points = points, Concavity = 30 });

            var triangulationAPI = new TriangulationAPI();
            var mesh = triangulationAPI.Triangulate2D(new Triangulation2DParameters() { Boundary = hull.vertices, Side = Side.Back });
            Triangles.GetComponent<MeshFilter>().mesh = mesh;

            var scalePoints = Mathf.Abs(shape.CameraPoint.z / 100f);
            var scaleBoundary = Mathf.Abs(shape.CameraPoint.z / 100f);

            CreatePointSpheres(points, scalePoints, sphereMesh, pointMaterial, Points);
            CreateLineCylinders(hull.vertices, scaleBoundary, cylinderMesh, boundaryMaterial, Boundary);

            Camera.main.backgroundColor = new Color(55f /255f, 189f / 255f, 175 / 255f);
        }

        /// <summary>
        /// Given the dataName and algorithmName the geometry is updated with the corresponding shape and method
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="algorithmName"></param>
        private void UpdateGeometry(string dataName, string algorithmName)
        {
            Triangles.GetComponent<MeshFilter>().mesh = new Mesh();

            Destroy(Points);
            Destroy(Lines);
            Destroy(Boundary);

            Points = new GameObject("Points");
            Lines = new GameObject("Lines");
            Boundary = new GameObject("Boundary");

            Points.transform.parent = GeometryObject.transform;
            Lines.transform.parent = GeometryObject.transform;
            Boundary.transform.parent = GeometryObject.transform;

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
    }
}

