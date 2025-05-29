using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BezierPathClass
{
    // A set of curves make a path
    private List<BezierCurve> mCurves;
    private List<int> mSamples;

    // Constructor for this class
    BezierPathClass()
    {

    }

    virtual public void AddCurve(BezierCurve curve, int samples)
    {
        mCurves.Add(curve);
        mSamples.Add(samples);
    }

    virtual public void Sample(List<Vector2> sampledPath)
    {
        for (int i = 0; i < mCurves.Count; i++) {
            for (float t = 0; t <= 1.0f; t+= 1.0f / mSamples[i])
            {
                sampledPath.Add(mCurves[i].CalculateCurvePoint(t));
            }
        }
    }
}
