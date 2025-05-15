using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public Rigidbody2D myRB { get; private set; }

    public float speed = 8.0f;
    public float speedMultiplier = 1f;
    
    public LayerMask obstacleLayer;

    public Vector2 initialDirection;
    public Vector2 direction { get; private set; } // The direction the object is currently moving.
    public Vector2 nextDirection { get; private set; } // The direction the player can queue for when they reach the next junction.
    public Vector3 startPosition;

    private void Awake()
    {
        myRB = GetComponent<Rigidbody2D>();
        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        speedMultiplier = 1f;
        direction = initialDirection;
        nextDirection = Vector2.zero;
        transform.position = startPosition;
        myRB.isKinematic = false;
        enabled = true;
    }

    private void Update()
    {
        // Every update this entity should attempt to move in its intended "nextDirection"
        if (nextDirection != Vector2.zero) {
            SetDirection(nextDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = myRB.position;
        Vector2 translation = direction * speed * speedMultiplier * Time.fixedDeltaTime;
        myRB.MovePosition(position + translation);
    }
    
    public void SetDirection(Vector2 newDirection, bool force = false)
    {
        if (force || !Occupied(newDirection))
        {
            direction = newDirection;
            nextDirection = Vector2.zero;
        }
        else // case when the attempted direction is blocked
        {
            nextDirection = newDirection; // Set the intended direction as the "nextDirection" the object will attempt to move.
        }
    }

    public bool Occupied(Vector2 newDirection)
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0.0f, newDirection, 1.5f, obstacleLayer);
        return hit.collider != null;
    }
}
