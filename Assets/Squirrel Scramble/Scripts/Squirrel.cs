using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Squirrel : MonoBehaviour
{
    Movement movement;
    public int score;
    public int haul;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    public void OnMove(InputValue moveValue)
    {
        Vector2 moveDirection = moveValue.Get<Vector2>().normalized;

        // Exit Condition: The player's movement returned to the resting state
        if (moveDirection == Vector2.zero) return;

        // Perform horizontal movement if the x value is larger than the y value
        if (Mathf.Abs(moveDirection.x) >= Mathf.Abs(moveDirection.y))
        {
            if (moveDirection.x > 0) movement.SetDirection(Vector2.right);
            else movement.SetDirection(Vector2.left);
        } // Otherwise perform vertical movement
        else
        {
            if (moveDirection.y > 0) movement.SetDirection(Vector2.up);
            else movement.SetDirection(Vector2.down);
        }
    }

    public void pickupPoints(int points)
    {
        haul += points;
    }

    public void depositPoints()
    {
        score += haul;
        haul = 0;
    }
}
