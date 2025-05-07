using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogChase : DogBehavior
{
    private void OnDisable()
    {
        this.dog.wander.Enable();
    }

    private void OnEnable()
    {
        Debug.Log("Chase mode was enabled");
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
            // Generic chase script:
            // check the directions from the current node.
            // Go in the direction that will bring the dog closest to the squirrel (target)
            ChaseNormal(node);
        }
    }
    
    private void ChaseNormal(Node node)
    {
        Vector2 direction = Vector2.zero;
        float minDistance = float.MaxValue;

        // Create a copy of the list of possible directions and grab the opposite direction the dog is currently traveling.
        List<Vector2> possibleDirections = ListWithoutOppositeDirection(node);

        foreach (Vector2 possibleDirection in possibleDirections)
        {
            Vector3 newPosition = this.transform.position + new Vector3(possibleDirection.x, possibleDirection.y, 0.0f);
            float distance = (this.dog.target.position - newPosition).sqrMagnitude;

            if (distance < minDistance)
            {
                direction = possibleDirection;
                minDistance = distance;
            }

            this.dog.movement.SetDirection(direction);
        }
    }


}

// Smell Dog: Go towards the node that the squirrel has visited most recently. Chase is triggered when it gets to a node that has a strong smell.
    // should also have a raycast at each node to determine if it sees the squirrel.
    // otherwise it might end up at the last node the squirrel visited and get confused because the squirrel hasn't visited a new node yet.
// Sight Dog: Go towards the node the dog saw the squirrel
// Sound Dog: Standard chase, but chase is triggered when the dog hears the squirrel (squirrel gets in close proximity to this dog)