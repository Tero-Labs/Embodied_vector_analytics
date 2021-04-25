#define Graph_And_Chart_PRO
using UnityEngine;
using ChartAndGraph;
using System.Collections.Generic;
using System;

public partial class LargeDataFeed
{
    public int RealtimeDownSampleCount = 10;
    int currentDownSampleCount = 0;
    DoubleVector3 DownSampleSum = new DoubleVector3();
    partial void OnDataLoaded()
    {
        RestartDownSampleCount();
    }
    public void AppendRealtimeWithDownSampling(double x, double y, double slideTime = 0f)
    {
        if (graph == null)
            return;
        bool show = false;
        if (mData.Count == 0)
            show = true;
        else
        {
            double viewX = mData[mData.Count - 1].x;
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - graph.DataSource.HorizontalViewSize;
            if (viewX >= pageStartThreshold && viewX <= pageEndThreshold)
                show = true;
        }
        var v = new DoubleVector2(x, y);
        mData.Add(v);
        if (show)
        {
            if (currentDownSampleCount >= RealtimeDownSampleCount)
            {
                RestartDownSampleCount();
                graph.DataSource.AddPointToCategoryRealtime("Player 1", x, y, slideTime);
            }
            DownSampleSum += v.ToDoubleVector3();
            currentDownSampleCount++;
            var avarage = DownSampleSum * 1.0 / (double)currentDownSampleCount;
            if(currentDownSampleCount != 1)
                graph.DataSource.UpdateLastPointInCategoryRealtime("Player 1", avarage.x, avarage.y, slideTime);
        }
        else
            RestartDownSampleCount();
    }
    void RestartDownSampleCount()
    {
        currentDownSampleCount = 0;
        DownSampleSum = new DoubleVector3();
    }
    public void AppendPointRealtime(double x, double y, double slideTime = 0f)
    {
        if (graph == null)
            return;
        bool show = false;
        if (mData.Count == 0)
            show = true;
        else
        {
            double viewX = mData[mData.Count - 1].x;
            double pageStartThreshold = currentPagePosition - mCurrentPageSizeFactor;
            double pageEndThreshold = currentPagePosition + mCurrentPageSizeFactor - graph.DataSource.HorizontalViewSize;
            if (viewX >= pageStartThreshold && viewX <= pageEndThreshold)
                show = true;
        }
        mData.Add(new DoubleVector2(x, y));
        if (show)
            graph.DataSource.AddPointToCategoryRealtime("Player 1", x, y, slideTime);
    }

}

