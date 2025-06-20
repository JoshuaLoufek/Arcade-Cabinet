using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SpaceshipController : MonoBehaviour
{
    Rigidbody2D myRB;
    SpriteRenderer shipSprite;
    public Sprite left;
    public Sprite center;
    public Sprite right;
    
    public float moveSpeed = 10f;
    bool isMoving = false;
    public Vector2 shipMovement;
    [HideInInspector] public bool shipFiring;

    PlayerInput playerInput;
    InputAction fireValue;

    public Bullet bullet;
    public float fireRate = 0.5f;
    float attackTimer;

    private void Awake()
    {
        myRB = GetComponent<Rigidbody2D>();
        shipSprite = GetComponent<SpriteRenderer>();
        playerInput = GetComponent<PlayerInput>();
        fireValue = playerInput.actions["Fire"];
        attackTimer = fireRate;
    }

    void FixedUpdate()
    {
        MoveShip();
        UpdateShipSprite();
        Attack();
    }

    private void Attack()
    {
        if (fireValue.IsPressed() && attackTimer >= fireRate) // The ship can attack if the attack button is pressed and the ship is ready to fire.
        {
            // Fire the ship's laser gun
            Bullet newBullet = Instantiate(bullet);
            newBullet.InitializeBullet(this.transform);
            attackTimer = 0f;
        }
        else // Check if the ship is ready to fire. Progress the timer if it's not ready yet.
        {
            if (attackTimer <= fireRate) attackTimer += Time.deltaTime;
        }
    }

    public void MoveShip()
    {
        if (isMoving)
        {
            myRB.velocity = shipMovement * moveSpeed;
        } 
        else
        {
            myRB.velocity = Vector2.zero;
        }
    }

    public void UpdateShipSprite()
    {
        if (shipMovement.x > 0 && isMoving) shipSprite.sprite = right;
        else if (shipMovement.x < 0 && isMoving) shipSprite.sprite = left;
        else shipSprite.sprite = center;
    }

    // I want 8 directional movement at a constant speed
    public void OnMove(InputValue moveValue)
    {
        // Gets a normalized version of the player's movement vector.
        Vector2 moveDirection = moveValue.Get<Vector2>().normalized;
        float xMovementValue, yMovementValue;

        // Exit Condition: The player's movement returned to the resting state.
        if (moveDirection == Vector2.zero)
        {
            isMoving = false;
            return;
        }

        // Translate the x into a usable value.
        if (Mathf.Abs(moveDirection.x) >= 0.5f)
        {
            if (moveDirection.x > 0) { xMovementValue = 1; }
            else { xMovementValue = -1; }
        }
        else { xMovementValue = 0; }

        // Translate the y into a usable value.
        if (Mathf.Abs(moveDirection.y) >= 0.5f)
        {
            if (moveDirection.y > 0) { yMovementValue = 1; }
            else { yMovementValue = -1; }
        }
        else { yMovementValue = 0; }

        // Update the flag to indicate if the ship is moving or not.
        if (xMovementValue == 0 && yMovementValue == 0) { isMoving = false; }
        else { isMoving = true; }

        // For diagonal movement we need to decrease how far is moved on the x and y axis.
        if (Mathf.Abs(xMovementValue) == 1 && Mathf.Abs(yMovementValue) == 1)
        {   // 0.7071 is an approximation of sqrt(1/2) which is derived from the pythagorean theorem where c=1 and a=b. c=1 because we want the player to move at "1 speed" in the diagonal c direction
            xMovementValue = 0.7071f * xMovementValue;
            yMovementValue = 0.7071f * yMovementValue;
        }

        // now set up the ship movement vector
        shipMovement.x = xMovementValue;
        shipMovement.y = yMovementValue;
    }
}
