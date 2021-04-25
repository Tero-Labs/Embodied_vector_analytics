using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FunctionTextInputMenu : MonoBehaviour
{
    public GameObject parent;
    public int str_index;
    public string str_arg;
    public InputField mainInputField;

    // Start is called before the first frame update
    void Start()
    {
        mainInputField.onValueChanged.AddListener(delegate { LockInput(mainInputField); });
    }

    public void setArgument(GameObject parent, int str_index)
    {
        this.parent = parent;
        this.str_index = str_index;
    }

    // Update is called once per frame
    void Update()
    {
        if (mainInputField != null && mainInputField.isFocused)
        {
            Paintable.click_on_inputfield = true;
        }
    }

    // Checks if there is anything entered into the input field.
    void LockInput(InputField input)
    {        
        str_arg = input.text;
    }
}
