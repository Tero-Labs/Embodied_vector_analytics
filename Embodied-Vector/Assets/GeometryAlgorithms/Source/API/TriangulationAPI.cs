using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Triangulation2D;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Triangulation3D;
using UnityEngine;
using System;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;

namespace Jobberwocky.GeometryAlgorithms.Source.API
{
    public class TriangulationAPI : ThreadingAPI
    {
        /// <summary>
        /// Creates a 2D triangulation of the given input points + parameters.
        /// The points input should not include the boundary and holes points.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Triangulate2DRaw(Triangulation2DParameters parameters)
        {
            var triWrapper2D = new Triangulation2DWrapper();
            return triWrapper2D.Triangulate2D(parameters);
        }

        /// <summary>
        /// Creates a 2D triangulation of the given input points + parameters.
        /// The points input should not include the boundary and holes points.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Triangulated Unity mesh</returns>
        public Mesh Triangulate2D(Triangulation2DParameters parameters)
        {
            // Create triangulation
            Geometry geometry = Triangulate2DRaw(parameters);

            return geometry.ToUnityMesh();
        }

        /// <summary>
        /// Creates a 2D triangulation using threading
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        public void Triangulate2DAsync(Action<Geometry> callback, Triangulation2DParameters parameters = null)
        {
            StartWorker((IParameters param, Action<Geometry> callbackResult) =>
                {
                    var triWrapper = new Triangulation2DWrapper();
                    var geometry = triWrapper.Triangulate2D((Triangulation2DParameters)param);

                    return new ThreadingResult(callbackResult, geometry);
                }, parameters, callback);
        }

        /// <summary>
        /// Creates a 3D triangulation of the given input points.
        /// Note that this method assumes that the 3D shape is convex and without holes.
        /// This means that concave shapes are triangulated to a convex shape and holes are removed.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Unity mesh</returns>
        public Mesh Triangulate3D(Triangulation3DParameters parameters)
        {
            var geometry = Triangulate3DRaw(parameters);

            return geometry.ToUnityMesh();
        }

        /// <summary>
        /// Creates a 3D triangulation of the given input points.
        /// Note that this method assumes that the 3D shape is convex and without holes.
        /// This means that concave shapes are triangulated to a convex shape and holes are removed.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Triangulate3DRaw(Triangulation3DParameters parameters)
        {
            var triWrapper3D = new Triangulation3DWrapper();
            return triWrapper3D.Triangulate3D(parameters);
        }

        /// <summary>
        /// Creates a 3D convex triangulation using threading
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="points"></param>
        /// <param name="parameters"></param>
        public void Triangulation3DThreading(Action<Geometry> callback, Triangulation3DParameters parameters)
        {
            StartWorker(
                (IParameters param, Action<Geometry> callbackResult) =>
                {
                    var triWrapper = new Triangulation3DWrapper();
                    var geometry = triWrapper.Triangulate3D(parameters);

                    return new ThreadingResult(callbackResult, geometry);
                }, parameters, callback);
        }
    }
}
