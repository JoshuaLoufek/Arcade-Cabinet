using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Vector2> possibleDirections { get; private set; }
    public LayerMask obstacleLayer;

    private void Start()
    {
        possibleDirections = new List<Vector2>();
        // Check up, down, left, and right
        CheckDirectionAvailability(Vector2.up);
        CheckDirectionAvailability(Vector2.down);
        CheckDirectionAvailability(Vector2.left);
        CheckDirectionAvailability(Vector2.right);
    }

    private void CheckDirectionAvailability(Vector2 direction)
    {
        // (from this location, cast a skinny ray, __, going the chosen direction, extending 1 unit, that collides with obstacles)
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0.0f, direction, 1f, obstacleLayer);
        // If there was no collision then the direction is valid
        if (hit.collider == null)
        {
            possibleDirections.Add(direction);
        }
    }
}
