using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour
{
    // Each route should have a set of paths that the object needs to follow in order to complete the route.
    [SerializeField] private Transform[] paths;
}
