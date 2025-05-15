using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Dog))]
public abstract class DogBehavior : MonoBehaviour
{
    public Dog dog;
    public float duration;
    // TODO: Create a "Chase Style" that the dog uses

    public virtual void Awake()
    {
        this.dog = GetComponent<Dog>();
    }

    public void Enable()
    {
        Enable(this.duration);
    }

    public virtual void Enable(float time)
    {
        this.enabled = true; // toggle this behavior as active
        CancelInvoke(); // destroy any old timers before a new timer is created
        Invoke(nameof(Disable), time); // disablethis new behavior after "time"
    }

    public virtual void Disable()
    {
        this.enabled = false;
        CancelInvoke();
    }

    public List<Vector2> ListWithoutOppositeDirection(Node node)
    {
        // Create a copy of the list of possible directions and grab the opposite direction the dog is currently traveling.
        List<Vector2> directionsCopy = new List<Vector2>(node.possibleDirections);
        Vector2 oppositeDirection = -this.dog.movement.direction;

        // If there is more than one direction to choose from and the opposite direction is possible
        if (directionsCopy.Count > 1 && directionsCopy.Contains(oppositeDirection)) {
            directionsCopy.Remove(oppositeDirection);
        }

        return directionsCopy;
    }
}
