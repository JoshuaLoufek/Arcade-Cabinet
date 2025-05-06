using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogWander : DogBehavior
{
    // Don't forget about your inherited global variables (dog, duration)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();
        // Check if:
            // 1. a node was collided with
            // 2. this behavior is enabled (important because this can still trigger if disabled)
            // 3. the overriding behavior isn't enabled (being scared overrides wandering around)
        if (node != null && this.enabled) //&& this.enabled && !this.dog.scared.enabled)
        {
            Debug.Log("A node was found and Wander is enabled.");
            // Create a copy of the list of possible directions and grab the opposite direction the dog is currently traveling.
            List<Vector2> directionsCopy = new List<Vector2>(node.possibleDirections);
            Vector2 oppositeDirection = -this.dog.movement.direction;

            // If there is more than one direction to choose from and the opposite direction is possible
            if (directionsCopy.Count > 1 && directionsCopy.Contains(oppositeDirection)) {
                directionsCopy.Remove(oppositeDirection);
            }

            // Choose a random direction from the list of remaining possible directions
            int index = Random.Range(0, directionsCopy.Count);

            // Set the direction the dog will travel in next
            this.dog.movement.SetDirection(directionsCopy[index]);
        }
    }
}
