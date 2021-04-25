using Jobberwocky.GeometryAlgorithms.Source.Core;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Hull2D;
using Jobberwocky.GeometryAlgorithms.Source.Algorithms.Hull3D;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using UnityEngine;
using System;

namespace Jobberwocky.GeometryAlgorithms.Source.API
{
    /// <summary>
    /// Class for the generation of hulls in 2D and 3D
    /// </summary>
    public class HullAPI : ThreadingAPI
    {
        /// <summary>
        /// Creates a 2D hull by using another thread. The callback function is used when the thread is finished.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="parameters"></param>
        public void Hull2DAsync(Action<Geometry> callback, Hull2DParameters parameters)
        {
            StartWorker(
                (IParameters param, Action<Geometry> callbackResult) =>
                    {
                        var hull2DWrapper = new Hull2DWrapper();
                        var hullGeometry = hull2DWrapper.Hull2D((Hull2DParameters)param);

                        return new ThreadingResult(callbackResult, hullGeometry);
                    }, parameters, callback);
        }

        /// <summary>
        /// Creates a 2D hull of the given input points and the concavity or maximal edge length defined in the parameters object.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Unity mesh</returns>
        public Mesh Hull2D(Hull2DParameters parameters)
        {
            return Hull2DRaw(parameters).ToUnityMesh();
        }

        /// <summary>
        /// Creates a 2D hull of the given input points and the concavity or maximal edge length defined in the parameters object.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Geometry</returns>
        public Geometry Hull2DRaw(Hull2DParameters parameters)
        {
            var hull2DWrapper = new Hull2DWrapper();
            return hull2DWrapper.Hull2D(parameters);
        }

        /// <summary>
        /// Creates a 3D convex hull of the given input points
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>Unity mesh</returns>
        public Mesh ConvexHull3D(Hull3DParameters parameters)
        {
            return ConvexHull3DRaw(parameters).ToUnityMesh();
        }

        /// <summary>
        /// Creates a 3D convex hull of the given input points 
        /// </summary>
        /// <param name="points"></param>
        /// <returns>HullMesh</returns>
        public Geometry ConvexHull3DRaw(Hull3DParameters parameters)
        {
            var hull3DWrapper = new Hull3DWrapper();
            return hull3DWrapper.Hull3D(parameters);
        }

        /// <summary>
        /// Creates a 3D hull by using another thread. The callback function is used when the thread is finished.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="points"></param>
        /// <param name="maxEdgeLength"></param>
        public void ConvexHull3DAsync(Action<Geometry> callback, Hull3DParameters parameters)
        {
            StartWorker(
                (IParameters param, Action<Geometry> callbackResult) =>
                {
                    var hull3DWrapper = new Hull3DWrapper();
                    var hullGeometry = hull3DWrapper.Hull3D((Hull3DParameters)param);

                    return new ThreadingResult(callbackResult, hullGeometry);
                }, parameters, callback);
        }

    }
}
