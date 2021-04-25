using UnityEngine;
using System;
using System.Collections.Generic;
using Jobberwocky.GeometryAlgorithms.Source.Parameters;
using Jobberwocky.GeometryAlgorithms.Source.Core;

namespace Jobberwocky.GeometryAlgorithms.Source.API
{
    public abstract class ThreadingAPI
    {
        protected static List<ThreadingResult> ThreadingResultQueue = new List<ThreadingResult>();

        /// <summary>
        /// Activate the available callbacks
        /// </summary>
        public void ActivateCallbacks()
        {
            for (var i = 0; i < ThreadingResultQueue.Count; i++)
            {
                lock (ThreadingResultQueue)
                {
                    var result = ThreadingResultQueue[i];

                    result.Callback(result.Output);

                    ThreadingResultQueue.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Starts the threading worker given the provided method
        /// </summary>
        /// <param name="method"></param>
        /// <param name="points"></param>
        /// <param name="parameters"></param>
        /// <param name="callback"></param>
        protected static void StartWorker(Func<IParameters, Action<Geometry>, ThreadingResult> method, IParameters parameters, Action<Geometry> callback)
        {
            method.BeginInvoke(parameters, callback, WorkerCompleted, method);
        }

        /// <summary>
        /// Method that is called when the threading process is finished. This will call the provided callback function
        /// </summary>
        /// <param name="method"></param>
        protected static void WorkerCompleted(IAsyncResult method)
        {
            var target = (Func<IParameters, Action<Geometry>, ThreadingResult>)method.AsyncState;

            var threadingResult = target.EndInvoke(method);

            ThreadingResultQueue.Add(threadingResult);
        }

        /// <summary>
        /// Object that is used to store the result when threading is used
        /// </summary>
        protected class ThreadingResult
        {
            public Action<Geometry> Callback;
            public Geometry Output;

            public ThreadingResult(Action<Geometry> callback, Geometry output)
            {
                Callback = callback;
                Output = output;
            }
        }
    }
}
