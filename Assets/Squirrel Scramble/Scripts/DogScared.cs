using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogScared : DogBehavior
{
    public SpriteRenderer sprite;

    public bool defeated { get; private set; }

    public override void Awake()
    {
        dog = GetComponent<Dog>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        defeated = false;
    }

    public override void Enable(float duration)
    {
        base.Enable(duration);
        sprite.color = Color.blue;

        Invoke(nameof(Flash), duration / 2.0f); // starts to flash halfway through the scared behavior
    }

    public override void Disable()
    {
        base.Disable();
        sprite.color = Color.white;
    }

    private void Flash()
    {
        if (!defeated)
        {
            sprite.color = Color.red;
            sprite.GetComponent<AnimatedSprite>().Restart();
        }
    }

    private void OnEnable()
    {
        this.dog.movement.speedMultiplier = 0.5f;
        this.defeated = false;
    }

    private void OnDisable()
    {
        this.dog.movement.speedMultiplier = 1.0f;
        this.defeated = false;
    }

    // Handles movement for the dog when it hits a node.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();
        // Check if:
        // 1. a node was collided with
        // 2. this behavior is enabled (important because this can still trigger if disabled)
        if (node != null && this.enabled)
        {
            ScaredNormal(node);
        }
    }

    private void ScaredNormal(Node node)
    {
        Vector2 direction = Vector2.zero;
        float maxDistance = float.MinValue;

        // Create a copy of the list of possible directions and grab the opposite direction the dog is currently traveling.
        List<Vector2> possibleDirections = ListWithoutOppositeDirection(node);

        foreach (Vector2 possibleDirection in possibleDirections)
        {
            Vector3 newPosition = this.transform.position + new Vector3(possibleDirection.x, possibleDirection.y, 0.0f);
            float distance = (this.dog.target.position - newPosition).sqrMagnitude;

            if (distance > maxDistance)
            {
                direction = possibleDirection;
                maxDistance = distance;
            }

            this.dog.movement.SetDirection(direction);
        }
    }

    // Handles what happens when the dog is defeated.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Squirrel squirrel = collision.gameObject.GetComponent<Squirrel>();
        if (squirrel != null)
        {
            if(enabled)
            {
                DogDefeated();
            }
        }
    }

    private void DogDefeated()
    {
        this.defeated = true;

        Vector3 position = this.dog.house.insideHouse.position;
        position.z = this.dog.transform.position.z;
        this.dog.transform.position = position;

        this.dog.house.Enable(this.duration); // tell the dog to wait in the dog house for a length equalling a full scared timer.

        this.sprite.color = Color.white;
    }
}
