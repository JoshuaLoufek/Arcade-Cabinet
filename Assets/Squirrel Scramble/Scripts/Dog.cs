using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : MonoBehaviour
{
    public Movement movement { get; private set; }
    AnimatedSprite animatedSprite;
    GameManager gameManager;

    public DogScared scared;
    public DogWander wander;
    public DogHouse house;
    public DogChase chase;

    public DogBehavior initalBehavior;
    public Transform target;
    
    public int points = 200;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        animatedSprite = GetComponentInChildren<AnimatedSprite>();
        gameManager = FindObjectOfType<GameManager>();

        scared = GetComponent<DogScared>();
        wander = GetComponent<DogWander>();
        house = GetComponent<DogHouse>();
        chase = GetComponent<DogChase>();
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        gameObject.SetActive(true);
        movement.ResetState();

        scared.Disable();
        chase.Disable();
        wander.Disable();
        house.Disable();

        // TODO: FIX
        //if (initalBehavior != null) initalBehavior.Enable();
        wander.Enable();
    }

    // Triggered whenever the dog and squirrel collide
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Squirrel squirrel = collision.gameObject.GetComponent<Squirrel>();
        if (squirrel != null)
        {
            if (scared.enabled) // The squirrel wins!
            {
                gameManager.DogDefeated(this);
            }
            else // The dog wins!
            {
                gameManager.SquirrelCaught();
            }
        }
    }
}
