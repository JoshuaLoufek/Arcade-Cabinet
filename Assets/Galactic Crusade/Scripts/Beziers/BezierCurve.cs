using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BezierCurve
{
    public Vector2 p0, p1, p2, p3;

    public Vector2 CalculateCurvePoint(float t)
    {
        float tt = t * t;
        float ttt = tt * t;
        float u = 1.0f - t;
        float uu = u * u;
        float uuu = uu * u;

        Vector2 point = (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
        point.x = Mathf.Round(point.x);
        point.y = Mathf.Round(point.y);
        return point;
    }
}
