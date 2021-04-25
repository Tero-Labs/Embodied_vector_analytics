using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required when Using UI elements.

public class NodeMenuScript : MonoBehaviour
{
    public InputField mainInputField;
    public Button delete;
    public Button copy;

    Vector3 target_pos;

    public GameObject menu_parent;

    // Start is called before the first frame update
    void Start()
    {
        target_pos = new Vector3(10f, 10f, 0);

        //https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.InputField-onEndEdit.html
        //Adds a listener that invokes the "LockInput" method when the player finishes editing the main input field.
        //Passes the main input field into the method when "LockInput" is invoked
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
        delete.onClick.AddListener(delegate { Delete(delete); });
        copy.onClick.AddListener(delegate { Copy(copy); });

        StartCoroutine(SetupParent());
    }

    IEnumerator SetupParent()
    {
        mainInputField.text = menu_parent.GetComponent<iconicElementScript>().icon_name;
        yield return null;
    }

    void Delete(Button delete)
    {
        StartCoroutine(Delete());
    }

    IEnumerator Delete()
    {
        GameObject graph = null;
        if (menu_parent.transform.parent.tag == "node_parent")
            graph = menu_parent.transform.parent.parent.gameObject;
        Destroy(menu_parent);

        yield return null;

        if (graph != null)
            graph.GetComponent<GraphElementScript>().nodes_init();

        Destroy(transform.gameObject);
    }


    void Copy(Button copy)
    {
        
        GameObject cp = Instantiate(menu_parent, menu_parent.transform.position + target_pos, Quaternion.identity, menu_parent.transform.parent);
        cp.GetComponent<iconicElementScript>().edge_position = menu_parent.GetComponent<iconicElementScript>().edge_position + target_pos;
        Paintable.totalLines++;
        cp.GetComponent<iconicElementScript>().icon_number = Paintable.totalLines;
        cp.GetComponent<iconicElementScript>().icon_name = "iconic_" + Paintable.totalLines.ToString();

        // needs a unique name in the object hierarchy
        cp.name = "iconic_" + Paintable.totalLines.ToString();

        target_pos += new Vector3(10f, 10f, 0);
    }


    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {
        Paintable.click_on_inputfield = true;

        if (input.text.Length > 0)
        {            
            menu_parent.GetComponent<iconicElementScript>().icon_name = input.text;
            //ToDo:show_name_in_a_text
            //menu_parent.name = input.text;
            //Debug.Log("name" + input.text + "has been updated for"+ menu_parent.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
            Paintable.click_on_inputfield = true;
    }
}
