using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
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

    public Vector2 restingLocation;
    
    private EnemyShipState state;
    public float speed = 1f;

    private bool newBehaviorAllowed; // Stops a second coroutine from starting when one is already in progress
    private bool pathInProgress; // Stops the route from starting multiple paths

    private void Awake()
    {
        state = EnemyShipState.Entering;
        newBehaviorAllowed = true;
        pathInProgress = false;
    }

    private void Update()
    {
        if (newBehaviorAllowed)
        {
            switch (state)
            {
                case EnemyShipState.Entering:
                    // Follow the entrance route to completion
                    StartCoroutine(GoByTheRoute(entranceRoute));
                    break;
                case EnemyShipState.Settling:
                    // Create a new route to get the ship from the end of the entrance route to the final location (Currently uses current location, should be a close approximate)
                    StartCoroutine(CreateThenFollowSettlingPath(new Vector2(transform.position.x, transform.position.y), restingLocation));
                    break;
                case EnemyShipState.Resting:
                    // Nothing happens here. The ship is waiting for commands
                    break;
                case EnemyShipState.Attacking:
                    // Need to create a new route to get to tthe start of the attack route
                    StartCoroutine(GoByTheRoute(attackRoute));
                    break;
                case EnemyShipState.Returning:
                    // Need to create a new route to get to the resting location
                    break;
                default:
                    break;
            }
        }
    }

    private void ProgressToNextState()
    {
        switch (state)
        {
            case EnemyShipState.Entering:
                state = EnemyShipState.Settling;
                break;
            case EnemyShipState.Settling:
                state = EnemyShipState.Resting;
                break;
            case EnemyShipState.Resting:
                state = EnemyShipState.Attacking;
                break;
            case EnemyShipState.Attacking:
                state = EnemyShipState.Returning;
                break;
            case EnemyShipState.Returning:
                state = EnemyShipState.Resting;
                break;
        }
    }

    private IEnumerator CreateThenFollowSettlingPath(Vector2 start, Vector2 end)
    {
        newBehaviorAllowed = false;

        Vector2 p0 = start; // Starting location
        Vector2 p1 = new Vector2(start.x, end.y);
        Vector2 p2 = new Vector2(end.x, start.y);
        Vector2 p3 = end; // Ending location

        StartCoroutine(TravelThePath(p0, p1, p2, p3));

        while (pathInProgress)
        {
            yield return new WaitForEndOfFrame();
        }

        ProgressToNextState();
        newBehaviorAllowed = true;
    }

    private IEnumerator GoByTheRoute(Route route)
    {
        newBehaviorAllowed = false;
        int nextPath = 0;
        
        while (nextPath < route.paths.Length || pathInProgress)
        {
            if (!pathInProgress)
            {
                Vector2 p0 = route.paths[nextPath].GetChild(0).position;
                Vector2 p1 = route.paths[nextPath].GetChild(1).position;
                Vector2 p2 = route.paths[nextPath].GetChild(2).position;
                Vector2 p3 = route.paths[nextPath].GetChild(3).position;

                StartCoroutine(TravelThePath(p0, p1, p2, p3));
                nextPath += 1;
            }

            yield return new WaitForEndOfFrame();
        }

        ProgressToNextState();
        newBehaviorAllowed = true;
    }

    // Each time the object finishes a path on the route this function is called so that the object can navigate along the path.
    private IEnumerator TravelThePath(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        pathInProgress = true;
        float t = 0f;

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
