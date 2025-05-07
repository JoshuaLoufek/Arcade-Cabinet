using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogWander : DogBehavior
{
    // Don't forget about your inherited global variables (dog, duration)

    // Whenever wandering is over, begin the chase sequence.
    private void OnDisable()
    {
        this.dog.chase.Enable();
    }

    private void OnEnable()
    {
        Debug.Log("Wander mode was enabled");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();
        // Check if:
            // 1. a node was collided with
            // 2. this behavior is enabled (important because this can still trigger if disabled)
            // 3. the overriding behavior isn't enabled (being scared overrides wandering around)
        if (node != null && this.enabled && !this.dog.scared.enabled)
        {
            // Create a copy of the list without the direction the dog just travelled
            List<Vector2> possibleDirections = ListWithoutOppositeDirection(node);

            // Choose a random direction from the list of remaining possible directions
            int index = Random.Range(0, possibleDirections.Count);

            // Set the direction the dog will travel in next
            this.dog.movement.SetDirection(possibleDirections[index]);
        }
    }
}
