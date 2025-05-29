using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierFollow : MonoBehaviour
{
    [SerializeField] private Transform[] routes; // represents the routes that the object will follow along

    private int routeToGo; // Holds the index of the next route the follow

    private float tParam; // The "t" (time) parameter for the current route

    private Vector2 objectPosition; // The current position of the object travelling on the curve

    private float speedModifier; // A modifier that dictates how fast the object travels along the curve

    private bool coroutineAllowed; // Stops a second coroutine from starting when one is already in progress

    // Initializes the object
    private void Start()
    {
        routeToGo = 0;
        tParam = 0f;
        speedModifier = 0.5f;
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
            tParam += Time.deltaTime * speedModifier;

            objectPosition = Mathf.Pow(1 - tParam, 3) * p0 +
                3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                Mathf.Pow(tParam, 3) * p3;

            transform.position = objectPosition;
            yield return new WaitForEndOfFrame();
        }

        tParam = 0f;
        routeToGo += 1;
        if (routeToGo > routes.Length - 1) routeToGo = 0;
        coroutineAllowed = true;
    }
}
