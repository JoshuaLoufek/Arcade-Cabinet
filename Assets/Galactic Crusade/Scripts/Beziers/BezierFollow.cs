using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// https://www.youtube.com/watch?v=11ofnLOE8pw
// https://www.youtube.com/watch?v=aVwxzDHniEw
// https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

public class BezierFollow : MonoBehaviour
{
    [SerializeField] private Transform[] routes; // represents the routes that the object will follow along

    private int routeToGo; // Holds the index of the next route the follow

    private float tParam; // The "t" (time) parameter for the current route

    private Vector2 objectPosition; // The current position of the object travelling on the curve

    public float speed = 0.01f;

    private bool coroutineAllowed; // Stops a second coroutine from starting when one is already in progress

    // Initializes the object
    private void Start()
    {
        routeToGo = 0;
        tParam = 0f;
        coroutineAllowed = true;
    }

    private void Update()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(GoByTheRoute(routeToGo));
        }
    }

    private IEnumerator GoByTheRoute(int routeNumber)
    {
        coroutineAllowed = false;

        Vector2 p0 = routes[routeNumber].GetChild(0).position;
        Vector2 p1 = routes[routeNumber].GetChild(1).position;
        Vector2 p2 = routes[routeNumber].GetChild(2).position;
        Vector2 p3 = routes[routeNumber].GetChild(3).position;

        while (tParam < 1)
        {
            // Finds the new t value based on how far the object should move
            tParam = CalculateTFromDistance(p0, p1, p2, p3, tParam, speed);

            // Now cacluate the new position from this new T value
            objectPosition = CalculatePosition(p0, p1, p2, p3, tParam);

            // Move the object to the new position 
            transform.position = objectPosition;

            // Rotate the object based on the current direction travelling in
            RotateObject(p0, p1, p2, p3, tParam);

            yield return new WaitForEndOfFrame();
        }

        tParam = 0f;
        routeToGo += 1;
        if (routeToGo > routes.Length - 1) routeToGo = 0;
        coroutineAllowed = true;
    }

    Vector2 CalculatePosition(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float time)
    {
        Vector2 position = 
            p0 * Mathf.Pow(1 - time, 3) + 
            p1 * 3 * Mathf.Pow(1 - time, 2) * time +
            p2 * 3 * (1 - time) * Mathf.Pow(time, 2) +
            p3 * Mathf.Pow(time, 3);

        return position;
    }

    float CalculateTFromDistance(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float oldT, float distance)
    {
        // These are the derivative vectors that determine velocity 
        Vector2 v1 = (-3 * p0) + (9 * p1) - (9 * p2) + (3 * p3);
        Vector2 v2 = (6 * p0) - (12 * p1) + (6 * p2);
        Vector2 v3 = (-3 * p0) + (3 * p1);

        // Determines the new T position from the distance to travel, the current T position, and the velocity vectors
        float newT = oldT + ((distance * 0.01f) / (oldT * oldT * v1 + oldT * v2 + v3).magnitude);

        return newT;
    }

    void RotateObject(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float oldT)
    {
        // These are the derivative vectors that determine velocity 
        Vector2 v1 = (-3 * p0) + (9 * p1) - (9 * p2) + (3 * p3);
        Vector2 v2 = (6 * p0) - (12 * p1) + (6 * p2);
        Vector2 v3 = (-3 * p0) + (3 * p1);

        Vector2 velocityVector = (oldT * oldT * v1 + oldT * v2 + v3);

        float intendedAngle = Mathf.Atan(velocityVector.x / velocityVector.y) * Mathf.Rad2Deg;

        // Want to rotate a certain number of degrees from the current angle to get to the intended agnle
        if ((velocityVector.x >= 0 && velocityVector.y <= 0) || (velocityVector.x <= 0 && velocityVector.y <= 0))
        {
            transform.rotation = Quaternion.Euler(0,0, -intendedAngle);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, -intendedAngle + 180f);
        }
        
    }
}
