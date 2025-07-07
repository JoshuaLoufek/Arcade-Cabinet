using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

// https://www.youtube.com/watch?v=11ofnLOE8pw
// https://www.youtube.com/watch?v=aVwxzDHniEw
// https://www.youtube.com/watch?v=jvPPXbo87ds
// https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve

// https://math.stackexchange.com/questions/877725/retrieve-the-initial-cubic-b%c3%a9zier-curve-subdivided-in-two-b%c3%a9zier-curves/879213#879213
// Bezier curves with "C2 Continuity" means that they meet smoothly (C0), have the same tangent direction (C1), and share the same curvature at the point they join (C2)
// This principle will help me generate dynamic bezier curves that properly flow together.
// https://stackoverflow.com/questions/12295773/joining-two-b%C3%A9zier-curves-smoothly-c2-continuous
// C0 continuity: P3 = Q0
// C1 continuity: P2 - P3 = Q0 - Q1
// C2 continuity: P1 - 2 * P2 + P3 = Q0 - 2 * Q1 + Q2

public enum EnemyShipState
{
    Entering, // Static Route
    Settling, // Dynamic Path
    Resting, // Static Location
    Preparing, // Dynamic Path
    Attacking, // Static Route
    Returning // Dynamic Path
}

public class SpaceEnemyLogic : MonoBehaviour
{
    // INFORMATION INTRINSIC TO THIS SHIP

    // Each enemy prefab contains a list of available entrance and attack routes that it can follow. 
    [SerializeField] public List<Route> entranceRoutes;
    [SerializeField] public List<Route> attackRoutes;

    // Once the routes are chosen from the list they are assigned to these variables here.
    private Route entranceRoute;
    private Route attackRoute;

    // Control variables for the enemy state machine
    private EnemyShipState state;
    private bool newBehaviorAllowed; // Stops a new behvaior coroutine from starting when one is already in progress
    private bool pathInProgress; // Stops the route from starting a new path while one is actively being followed
    
    // Qualties of the ship
    public float speed = 1f;

    // INFORMATION RECEIVED FROM THE ENEMY SHIP MANAGER
    private Vector2 restingLocation;
    private bool attackSignal;

    // Outdated variables that will be replaced with the lists of routes above -------------------

    


    private void Awake()
    {
        state = EnemyShipState.Entering;
        newBehaviorAllowed = true;
        pathInProgress = false;
    }

    // This is the enemy state machine. Eventually the enemy manager will control the preparing and attacking behavior.
    // Every frame the ship checks if it currently has an active behavior. If it doesn't, it attempts to start the next one in the sequence.
    private void FixedUpdate()
    {
        if (newBehaviorAllowed) // Every frame the ship tries to start a new behavior. However, if a behavior is already active then a new one can't be started.
        {
            switch (state) // When a behavior ends a new state is chosen. This switch statement uses that new state to activate the subsequent behavior.
            {
                case EnemyShipState.Entering:
                    // Follow the entrance route to completion
                    StartCoroutine(FollowTheRoute(entranceRoute));
                    break;
                case EnemyShipState.Settling:
                    // Create a new route to get the ship from the end of the entrance route to the final location (Currently uses current location, should be a close approximate)
                    StartCoroutine(FollowDynamicSettlingPath(new Vector2(transform.position.x, transform.position.y), restingLocation));
                    break;
                case EnemyShipState.Resting:
                    // Nothing happens here. The ship is waiting for commands. (Maybe rotate ship if settling and/or preparing sequence change)
                    StartCoroutine(RestingState());
                    break;
                case EnemyShipState.Preparing:
                    // Create a path from the resting location to the start of the attack route
                    StartCoroutine(FollowDynamicPreparingPath(new Vector2(transform.position.x, transform.position.y), attackRoute.paths[0].GetChild(0).position));
                    break;
                case EnemyShipState.Attacking:
                    // Follow the attack route to completion
                    StartCoroutine(FollowTheRoute(attackRoute));
                    break;
                case EnemyShipState.Returning:
                    // Need to create a new route to get to the resting location
                    StartCoroutine(FollowDynamicReturningPath(new Vector2(restingLocation.x, restingLocation.y + 10f), restingLocation));
                    break;
                default:
                    break;
            }
        }
    }

    // This function is generally called at the end of a behavior to update the model's state to the next one in order.
    private void ProgressToNextState()
    {
        switch (state)
        {
            // After entering the ship must settle
            case EnemyShipState.Entering:
                state = EnemyShipState.Settling;
                break;
            // After settling the ship is in it's resting location
            case EnemyShipState.Settling:
                state = EnemyShipState.Resting;
                break;
            // After resting the ship must prepare to attack
            case EnemyShipState.Resting:
                state = EnemyShipState.Preparing;
                break;
            // Once the ship is done preparing the attack will commence
            case EnemyShipState.Preparing:
                state = EnemyShipState.Attacking;
                break;
            // After attacking the ship begins returning to the resting state
            case EnemyShipState.Attacking:
                state = EnemyShipState.Returning;
                break;
            // Upon returning the ship is in the resting state
            case EnemyShipState.Returning:
                state = EnemyShipState.Resting;
                break;
        }
    }

    private IEnumerator RestingState()
    {
        newBehaviorAllowed = false;

        // TEMP CODE: Wait in the resting position for three seconds.
        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        // In the future this will act as a lock to keep the ship in resting mode until it recieves commands the Enemy Ship Manager.

        ProgressToNextState(); // Updates the ship to follow a next state in the process.
        newBehaviorAllowed = true; // Frees up the ship to start a new behavior
    }

    // Control function for the Settling behavior to get the ship from the end of the Entrance Route to the assigned Resting Location
    private IEnumerator FollowDynamicSettlingPath(Vector2 start, Vector2 end)
    {
        newBehaviorAllowed = false;  // Disables new behaviors until this behavior is finished.

        Vector2 p0 = start; // Starting location
        Vector2 p1 = new Vector2(start.x, end.y); // These values form an "S" shape to guide the ship home in a smooth and logical fashion.
        Vector2 p2 = new Vector2(end.x, start.y);
        Vector2 p3 = end; // Ending location

        StartCoroutine(TravelThePath(p0, p1, p2, p3));

        while (pathInProgress)
        {
            yield return new WaitForEndOfFrame();
        }

        // Now the Resting Behavior is ready to begin in full
        ProgressToNextState(); // Updates the ship to follow a next state in the process.
        newBehaviorAllowed = true; // Frees up the ship to start a new behavior
    }

    // Control function for the Preparing behvaior to get the ship from the Resting Location to the start of the Attacking Route
    private IEnumerator FollowDynamicPreparingPath(Vector2 start, Vector2 end)
    {
        newBehaviorAllowed = false; // Disables new behaviors until this behavior is finished.

        Vector2 p0 = start; // Starting location
        Vector2 p1 = new Vector2((start.x + end.x) / 2f, start.y + 4f); // These values form a loop up and then down to get to the start of the Attacking Route.
        Vector2 p2 = new Vector2(end.x, end.y + 4f); // These might need to have some adjustments or an intermediary path if the start and end x-values are too similar.
        Vector2 p3 = end; // Ending location

        StartCoroutine(TravelThePath(p0, p1, p2, p3));

        while (pathInProgress)
        {
            yield return new WaitForEndOfFrame();
        }

        // Now the attack sequence is ready to begin in full
        ProgressToNextState(); // Updates the ship to follow a next state in the process.
        newBehaviorAllowed = true; // Frees up the ship to start a new behavior
    }

    private IEnumerator FollowDynamicReturningPath(Vector2 start, Vector2 end)
    {
        newBehaviorAllowed = false;

        float thirdDistance = (start.y - end.y) / 3;
        float y1 = start.y - thirdDistance;
        float y2 = end.y + thirdDistance;

        Vector2 p0 = start; // Starting location
        Vector2 p1 = new Vector2(start.x, y1); // This is a straight line down from above the screen into the resting point.
        Vector2 p2 = new Vector2(end.x, y2);
        Vector2 p3 = end; // Ending location
        
        StartCoroutine(TravelThePath(p0, p1, p2, p3));

        while (pathInProgress)
        {
            yield return new WaitForEndOfFrame();
        }

        // From here the ship goes into the resting state
        ProgressToNextState(); // Updates the ship to follow a next state in the process.
        newBehaviorAllowed = true; // Frees up the ship to start a new behavior
    }

    // Used by the Entering and Attacking behaviors because they have predefined routes that they follow.
    private IEnumerator FollowTheRoute(Route route)
    {
        newBehaviorAllowed = false;
        int nextPath = 0;
        
        // Continue the Route behavior while there are still upcoming paths to take or a path is currently in progress 
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
        } // Exit when all paths have been exhausted and the last path is finished

        ProgressToNextState();
        newBehaviorAllowed = true;
    }

    // This function is called to have the ship travel along a Bezier Curve path
    private IEnumerator TravelThePath(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        pathInProgress = true;
        float t = 0f;

        // Velocity is not consistent on a bezier curve. Can't just increase the T value at a constant rate and expect the ship to move at a constant speed.
        // This while loop simulates constant move speed along a bezier curve. Uses the distance we expect the enemy to move each frame and approximating the T value from that.
        while (t < 1f)
        {
            // Finds the new T value based on the distance we expect the enemy to move each frame
            t = CalculateTimeFromDistanceTravelled(p0, p1, p2, p3, t, speed);

            // Cacluates the new position from the new T value and moves the object there 
            transform.position = CalculatePositionAtTime(p0, p1, p2, p3, t);

            // Rotate the object based on the current direction travelling in
            RotateObject(p0, p1, p2, p3, t);

            yield return new WaitForEndOfFrame();
        }

        pathInProgress = false;
    }

    // START - TravelThePath Helper Functions
    float CalculateTimeFromDistanceTravelled(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float oldT, float distance)
    {
        // These are the derivative vectors that determine velocity 
        Vector2 v1 = (-3 * p0) + (9 * p1) - (9 * p2) + (3 * p3);
        Vector2 v2 = (6 * p0) - (12 * p1) + (6 * p2);
        Vector2 v3 = (-3 * p0) + (3 * p1);

        // Determines the new T position from the distance to travel, the current T position, and the velocity vectors
        float newT = oldT + ((distance * 0.01f) / ((oldT * oldT * v1) + (oldT * v2) + (v3)).magnitude);

        // catches the edge case scenario where newT ends up being set to infinity or negative infinity. We want to slightly increment t to nudge it along the right path and away from the position that is producing extreme values.
        if (newT == Mathf.Infinity || newT == Mathf.NegativeInfinity) newT = oldT + 0.01f; 
        
        // This function can never return a t-value outside of the bounds of [0,1]
        if (newT > 1f) return 1f;
        else if (newT < 0f) return 0f;
        else return newT;
    }

    Vector2 CalculatePositionAtTime(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float time)
    {
        Vector2 position =
            p0 * Mathf.Pow(1 - time, 3) +
            p1 * 3 * Mathf.Pow(1 - time, 2) * time +
            p2 * 3 * (1 - time) * Mathf.Pow(time, 2) +
            p3 * Mathf.Pow(time, 3);

        return position;
    }

    void RotateObject(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float oldT)
    {
        // These are the derivative vectors that determine velocity 
        Vector2 v1 = (-3 * p0) + (9 * p1) - (9 * p2) + (3 * p3);
        Vector2 v2 = (6 * p0) - (12 * p1) + (6 * p2);
        Vector2 v3 = (-3 * p0) + (3 * p1);

        Vector2 velocityVector = (oldT * oldT * v1 + oldT * v2 + v3);

        float intendedAngle = Mathf.Atan(velocityVector.x / velocityVector.y) * Mathf.Rad2Deg;

        // Exit case to skip rotation whenever the arctangent would return a non existent value
        if (float.IsNaN(intendedAngle) || intendedAngle == Mathf.Infinity || intendedAngle == Mathf.NegativeInfinity) return;

        // Set rotation to the intended angle with an offset
        if (velocityVector.y <= 0)
        {
            transform.rotation = Quaternion.Euler(0,0, -intendedAngle);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, -intendedAngle + 180f);
        } // NOTE - I understand the math behind this, but there was some guesswork involved to set up the if/else statement properly. May wish to revise in the future.
    }
    // END - TravelThePath Helper Functions

    // Getters and Setters for Private Variables
    public void SetEntranceRoute(Route route) { entranceRoute = route; }

    public void SetAttackRoute(Route route) { attackRoute = route; }

    public void SetRestingLocation(Vector2 location) { restingLocation = location; }

    public void SetAttackSignal(bool signal) { attackSignal = signal; }
}
