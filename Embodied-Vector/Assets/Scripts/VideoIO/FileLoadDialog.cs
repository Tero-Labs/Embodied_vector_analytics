using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using UnityEngine.Video;

public class FileLoadDialog : MonoBehaviour
{
    // https://github.com/yasirkula/UnitySimpleFileBrowser
    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    public GameObject videoplayer;

    void Start()
    {
        
    }

    public void DialogShow()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Videos", ".mp4", ".avi"),
                            new FileBrowser.Filter("Files", ".txt", ".csv"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        // FileBrowser.SetDefaultFilter(".mp4");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("assets", Application.dataPath, null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //						   () => { Debug.Log( "Canceled" ); },
        //						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        // Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
            {
                Debug.Log(FileBrowser.Result[i]);
                if (FileBrowser.Result[i].EndsWith(".mp4"))
                {
                    GameObject temp = Instantiate(videoplayer, new Vector3(0, 0, -2f), Quaternion.identity 
                        /*transform.GetComponent<Paintable>().canvas_radial.transform*/);

                    temp.transform.GetChild(0).GetComponent<VideoPlayer>().url = FileBrowser.Result[i].ToString();
                    temp.transform.GetChild(0).GetComponent<VideoPlayer>().Play();
                    GameObject slider = temp.transform.GetComponent<VideoPlayerChildrenAccess>().slider;
                    /*temp.transform.GetComponent<VideoPlayerChildrenAccess>().canvas.renderMode = RenderMode.ScreenSpaceOverlay;*/                  

                    // load the annotate file as well
                    int trim_pos = FileBrowser.Result[i].IndexOf(".");                    
                    slider.GetComponent<VideoController>().loadAnnotation(FileBrowser.Result[i].Substring(0, trim_pos) + ".json");

                    if (FileBrowser.Result[i].Contains("airplane"))
                    {
                        slider.GetComponent<VideoController>().loadRoutes(FileBrowser.Result[i].Substring(0, trim_pos) + "_routes.json");
                        temp.transform.GetComponent<VideoPlayerChildrenAccess>().node_radius.isOn = false;
                        temp.transform.GetComponent<VideoPlayerChildrenAccess>().site_specific.isOn = true;
                    }
                        

                    slider.GetComponent<VideoController>().paintable = transform.gameObject;
                }
                else if(FileBrowser.Result[i].EndsWith(".jpg") || FileBrowser.Result[i].EndsWith(".png"))
                {
                    transform.GetComponent<Paintable>().createImageIcon(FileBrowser.Result[i]);
                }
                else if (FileBrowser.Result[i].EndsWith(".txt") || FileBrowser.Result[i].EndsWith(".csv"))
                {
                    int trim_pos = FileBrowser.Result[i].LastIndexOf("\\");                     
                    string filepath = FileBrowser.Result[i].Substring(trim_pos + 1);

                    int trim_pos_2 = filepath.IndexOf(".");
                    // second argument basically dpicts the length of the array
                    List<Dictionary<string, object>> data = CSVReader.Read(FileBrowser.Result[i].Substring(trim_pos+1, trim_pos_2));

                    StartCoroutine(CreateGraphfromFile(data));
                }

                yield return null;
            }
                

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            /*string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);*/
        }
    }

    IEnumerator CreateGraphfromFile(List<Dictionary<string, object>> data)
    {
        GameObject tempgraph = Instantiate(transform.GetComponent<Paintable>().GraphElement);
        tempgraph.GetComponent<GraphElementScript>().abstraction_layer = "graph";
        tempgraph.GetComponent<GraphElementScript>().paintable = transform.gameObject;
        Paintable.graph_count++;
        tempgraph.name = "graph_" + Paintable.graph_count.ToString();
        tempgraph.tag = "graph";
        tempgraph.transform.parent = transform.GetComponent<Paintable>().Objects_parent.transform;
        tempgraph.GetComponent<GraphElementScript>().graph_name = "G" + Paintable.graph_count.ToString();
        tempgraph.GetComponent<GraphElementScript>().nodeMaps = new Dictionary<string, Transform>();

        GameObject tempnodeparent = tempgraph.transform.GetChild(0).gameObject;
        GameObject tempedgeparent = tempgraph.transform.GetChild(1).gameObject;

        foreach (var line in data)
        {
            //int edge_start, edge_end;

            List<string> nodes = new List<string>();

            foreach (var col in line)
            {
                nodes.Add(col.Value.ToString());

                /*if (col.Key == "start_node")
                    edge_start = (int)col.Value;

                else
                    edge_end = (int)col.Value;

                print(col.Key + " " + col.Value); */

                if (tempgraph.GetComponent<GraphElementScript>().nodeMaps.ContainsKey(col.Value.ToString()))
                {
                    continue;
                }
                else
                {
                    GameObject child = transform.GetComponent<Paintable>().createImageIcon("Assets/Icons/" + "stick_figure_icon.png", (int)col.Value);
                    yield return null;
                    child.transform.parent = tempnodeparent.transform;
                    tempgraph.GetComponent<GraphElementScript>().nodeMaps.Add(col.Value.ToString(), child.transform);
                }                    
            }

            tempgraph.GetComponent<GraphElementScript>().EdgeCreation("edge", nodes.ToArray(), 1);
            yield return null;
        }

        //tempgraph.GetComponent<GraphElementScript>().Graph_init();
        tempgraph.GetComponent<GraphElementScript>().ChangeLayout("circular");
    }
}
