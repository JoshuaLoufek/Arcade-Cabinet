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
    int xMovement = 0;
    int yMovement = 0;
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

    // Update is called once per frame
    void FixedUpdate()
    {
        MoveShip();
        UpdateShipSprite();
        Attack();
    }

    private void Attack()
    {
        if (fireValue.IsPressed() && attackTimer >= fireRate) 
        {
            // Fire the ship's laser gun
            Bullet newBullet = Instantiate(bullet);
            newBullet.InitializeBullet(this.transform);
            attackTimer = 0f;
        }
        else
        {
            // Check if the ship is ready to fire, then increase the timer if it's not ready
            if (attackTimer <= fireRate) attackTimer += Time.deltaTime;
        }
    }

    public void MoveShip()
    {
        if (isMoving)
        {
            myRB.velocity = shipMovement * moveSpeed;
        } else
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

    public void OnMove(InputValue moveValue)
    {
        Vector2 moveDirection = moveValue.Get<Vector2>().normalized;
        
        // Exit Condition: The player's movement returned to the resting state
        if (moveDirection == Vector2.zero)
        {
            isMoving = false;
            return;
        }

        // I want 8 directional movement at a constant speed

        // first lets sort out the x dirction
        if (Mathf.Abs(moveDirection.x) >= 0.5f)
        {
            if (moveDirection.x > 0) { xMovement = 1; }
            else { xMovement = -1; }
        }
        else { xMovement = 0; }

        // now lets get the y diretion
        if (Mathf.Abs(moveDirection.y) >= 0.5f)
        {
            if (moveDirection.y > 0) { yMovement = 1; }
            else { yMovement = -1; }
        }
        else { yMovement = 0; }

        // now to tell the update script if the ship is moving or not
        if (xMovement == 0 && yMovement == 0)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        // now set up the ship movement vector
        shipMovement.x = xMovement;
        shipMovement.y = yMovement;

        Debug.Log(moveDirection);
        Debug.Log("X: " + xMovement + "\nY: " + yMovement);

        // Move the ship
        myRB.velocity = shipMovement * moveSpeed;
    }
}
