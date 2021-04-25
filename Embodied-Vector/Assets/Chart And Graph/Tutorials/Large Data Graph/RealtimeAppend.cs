#define Graph_And_Chart_PRO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeAppend : MonoBehaviour
{
    float time = 1f;
    double x = -1 , y = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time -= Time.deltaTime;
        if(time <= 0f)
        {
            time = 1f;
            var feed = GetComponent<LargeDataFeed>();
            if (feed != null)
            {
                if (x == -1)
                {
                    var v = feed.GetLastPoint();
                    x = v.x;
                    y = v.y;
                }

                y += UnityEngine.Random.value * 10f - 5f;
                x += UnityEngine.Random.value;
                feed.AppendRealtimeWithDownSampling(x, y, 1f);
            }
        }
    }
}
