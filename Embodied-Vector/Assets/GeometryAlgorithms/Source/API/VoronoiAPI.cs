using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Voronoi2D;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Voronoi3D;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine;
using System;

namespace Jobberwocky.GeometryAlgorithms.Source.API
{
    public class VoronoiAPI : ThreadingAPI
    {
        /// <summary>
        /// Creates a voronoi diagriam of the given input points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>A Unity mesh of a voronoi diagram</returns>
        public Mesh Voronoi2D(Voronoi2DParameters parameters)
        {
            return Voronoi2DRaw(parameters).ToUnityMesh();
        }

        /// <summary>
        /// Creates a 2D voronoi diagram
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>VoronoiDiagramMesh</returns>
        public Geometry Voronoi2DRaw(Voronoi2DParameters parameters)
        {
            var voronoi2DWrapper = new Voronoi2DWrapper();
            return voronoi2DWrapper.Voronoi2D(parameters);
        }

        /// <summary>
        /// Creates a 2D voronoi diagram using threading
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        public void Voronoi2DAsync(Action<Geometry> callback, Voronoi2DParameters parameters = null)
        {
            StartWorker((IParameters param, Action<Geometry> callbackResult) =>
            {
                var wrapper = new Voronoi2DWrapper();
                var geometry = wrapper.Voronoi2D((Voronoi2DParameters)param);

                return new ThreadingResult(callbackResult, geometry);
            }, parameters, callback);
        }

        /// <summary>
        /// Creates a 3D voronoi diagram of the given input points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Unity mesh</returns>
        public Mesh Voronoi3D(Voronoi3DParameters parameters)
        {
            return Voronoi3DRaw(parameters).ToUnityMesh();
        }

        /// <summary>
        /// Creates a 3D voronoi diagram of the given input points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Voronoi3DRaw(Voronoi3DParameters parameters)
        {
            var voronoi3DWrapper = new Voronoi3DWrapper();
            return voronoi3DWrapper.Voronoi3D(parameters);
        }

        /// <summary>
        /// Creates a 3D voronoi diagram using threading
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="points"></param>
        /// <param name="parameters"></param>
        public void Voronoi3DAsync(Action<Geometry> callback, Voronoi3DParameters parameters)
        {
            StartWorker((IParameters param, Action<Geometry> callbackResult) =>
            {
                var wrapper = new Voronoi3DWrapper();
                var geometry = wrapper.Voronoi3D((Voronoi3DParameters)param);

                return new ThreadingResult(callbackResult, geometry);
            }, parameters, callback);
        }
    }
}
