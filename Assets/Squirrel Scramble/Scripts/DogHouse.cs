using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogHouse : DogBehavior
{
    public Transform insideHouse;
    public Transform outsideHouse;

    private void OnEnable()
    {
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        if (this.gameObject.activeSelf) { 
            StartCoroutine(LeaveHouse());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.enabled && collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            this.dog.movement.SetDirection(-this.dog.movement.direction, true);
        }
    }

    private IEnumerator LeaveHouse()
    {
        this.dog.movement.SetDirection(Vector2.up, true); // force upwards movement
        this.dog.movement.myRB.isKinematic = true; // turns off physics and collision
        this.dog.movement.enabled = false; // turns off the movement script to allow this function to move the dog exclusively

        Vector3 position = this.transform.position;
        float duration = 0.5f;
        float elapsed = 0.0f;

        // Moves the dog to the inside position over the course of duration
        while (elapsed < duration)
        {
            Vector3 newPosition = Vector3.Lerp(position, this.insideHouse.position, elapsed / duration);
            newPosition.z = position.z;
            this.dog.transform.position = newPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0.0f;

        // Moves the dog to the outside position over the course of duration
        while (elapsed < duration)
        {
            Vector3 newPosition = Vector3.Lerp(this.insideHouse.position, this.outsideHouse.position, elapsed / duration);
            newPosition.z = position.z;
            this.dog.transform.position = newPosition;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Choose whether to go left or right at random
        if (Random.Range(0, 1) > 0) this.dog.movement.SetDirection(Vector2.right, true);
        else this.dog.movement.SetDirection(Vector2.left, true);

        this.dog.movement.myRB.isKinematic = false; // reenables physics and collisions
        this.dog.movement.enabled = true; // allows the movement script to take control again
        this.dog.wander.Enable(); // set the initial behavior to wandering
    }
}
