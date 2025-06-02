using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// https://www.youtube.com/watch?v=11ofnLOE8pw
// https://www.youtube.com/watch?v=aVwxzDHniEw
// https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

public enum EnemyShipState
{
    Entering,
    Settling,
    Resting,
    Attacking,
    Returning
}

public class BezierFollow : MonoBehaviour
{
    [SerializeField] private Route entranceRoute;
    [SerializeField] private Route attackRoute;
    [SerializeField] private Route returnRoute;

    public Transform restingLocation;

    private EnemyShipState state;
    public float speed = 1f;

    private bool coroutineAllowed; // Stops a second coroutine from starting when one is already in progress
    private bool pathInProgress; // Stops the route from starting multiple paths

    private void Awake()
    {
        state = EnemyShipState.Entering;
        coroutineAllowed = true;
        pathInProgress = false;
    }

    private void Update()
    {
        if (coroutineAllowed)
        {
            Route route = DetermineRoute(state);

            StartCoroutine(GoByTheRoute(route));
            // StartCoroutine(Old_GoByThePath(nextPath));
        }
    }

    private Route DetermineRoute(EnemyShipState currentState)
    {
        Route route;

        switch (state)
        {
            case EnemyShipState.Entering:
                route = entranceRoute;
                break;
            case EnemyShipState.Attacking:
                route = attackRoute;
                break;
            case EnemyShipState.Returning:
                route = returnRoute;
                break;
            default:
                route = entranceRoute;
                break;
        }

        return route;
    }

    private IEnumerator GoByTheRoute(Route route)
    {
        coroutineAllowed = false;
        int nextPath = 0;
        
        while (nextPath < route.paths.Length)
        {
            if (!pathInProgress)
            {
                StartCoroutine(TravelThePath(route.paths[nextPath]));
                nextPath += 1;
            }

            yield return new WaitForEndOfFrame();
        }

        coroutineAllowed = true;
    }

    // Each time the object finishes a path on the route this function is called so that the object can navigate along the path.
    private IEnumerator TravelThePath(Transform path)
    {
        pathInProgress = true;
        float t = 0f;

        Vector2 p0 = path.GetChild(0).position;
        Vector2 p1 = path.GetChild(1).position;
        Vector2 p2 = path.GetChild(2).position;
        Vector2 p3 = path.GetChild(3).position;

        while (t < 1f)
        {
            // Finds the new t value based on how far the object should move
            t = CalculateTFromDistance(p0, p1, p2, p3, t, speed);

            // Now cacluate the new position from this new T value and move the object to the new position 
            transform.position = CalculatePosition(p0, p1, p2, p3, t);

            // Rotate the object based on the current direction travelling in
            RotateObject(p0, p1, p2, p3, t);

            yield return new WaitForEndOfFrame();
        }

        pathInProgress = false;
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

        // Set rotation to the intended angle with an offset
        if (velocityVector.y <= 0)
        {
            transform.rotation = Quaternion.Euler(0,0, -intendedAngle);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, -intendedAngle + 180f);
        }
    }
}
