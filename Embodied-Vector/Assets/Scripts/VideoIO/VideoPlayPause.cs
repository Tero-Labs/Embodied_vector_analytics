using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayPause : MonoBehaviour
{

    public Button perform_action;
    public Sprite play_sprite;
    public Sprite pause_sprite;
    [SerializeField]
    private VideoPlayer videoplayer;
    public bool playFlag;

    // Start is called before the first frame update
    void Start()
    {
        perform_action.onClick.AddListener(delegate { OnClickButton(perform_action); });
        playFlag = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnClickButton(Button perform_action)
    {
        if (playFlag)
        {
            videoplayer.Pause();
            playFlag = false;
            transform.GetComponent<Image>().sprite = play_sprite;
        }
        else
        {
            videoplayer.Play();
            playFlag = true;
            transform.GetComponent<Image>().sprite = pause_sprite;
        }
    }
}
