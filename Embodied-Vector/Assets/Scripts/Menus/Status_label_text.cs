using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Status_label_text : MonoBehaviour
{
    public TMP_Text tmptextlabel;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(transform.gameObject, 0.5f);
    }

    public void ChangeLabel(string status)
    {
        tmptextlabel.text = status;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
