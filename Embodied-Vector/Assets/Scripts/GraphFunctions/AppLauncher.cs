using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;


public class AppLauncher : MonoBehaviour
{
    Process process = null;
    StreamWriter messageStream;

    void Start()
    {
        try
        {
            var processInfo = new ProcessStartInfo("python.exe", Application.dataPath + "\\NetMQExample\\Scripts\\Graphserver.py --" );
            //processInfo.Arguments = "--hypergraph_to_graph";
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = true;

            process = Process.Start(processInfo);
            
            UnityEngine.Debug.Log("Successfully launched app");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Unable to launch app: " + e.Message);
        }
    }


    void DataReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        // Handle it
    }


    void ErrorReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        UnityEngine.Debug.LogError(eventArgs.Data);
    }


    void OnApplicationQuit()
    {
        if (process != null && !process.HasExited )
        {
            process.Kill();
        }
    }
}