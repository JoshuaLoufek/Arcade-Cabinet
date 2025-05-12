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
        StartCoroutine(LeaveHouse());
    }

    private IEnumerator LeaveHouse()
    {
        this.dog.movement.SetDirection(Vector2.up, true); // force upwards movement
        this.dog.movement.myRB.isKinematic = true; // turns off physics and collision
        this.dog.movement.enabled = false;

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

        // go left or right
        if (Random.Range(0, 2) > 0) this.dog.movement.SetDirection(Vector2.right, true);
        else this.dog.movement.SetDirection(Vector2.left, true);

        this.dog.movement.myRB.isKinematic = false;
        this.dog.movement.enabled = true;
        this.dog.wander.enabled = true;
    }
}
