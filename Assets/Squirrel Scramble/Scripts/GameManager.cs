using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public Dog[] dogs;
    public Squirrel squirrel;
    public Transform nuts;

    public int lives { get; private set; }
    public int pouch { get; private set; } // Points currently carried by the squirrel
    public int stockpile { get; private set; } // Points safely brought back to the tree
    public int dogMultiplier { get; private set; } = 1;


    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (this.lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        setPouch(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        foreach (Transform nut in this.nuts) {
            nut.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < dogs.Length; i++) {
            this.dogs[i].ResetState();
        }

        this.squirrel.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        for (int i = 0; i < dogs.Length; i++) {
            this.dogs[i].gameObject.SetActive(false);
        }

        this.squirrel.gameObject.SetActive(false);
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void DogDefeated(Dog dog)
    {
        addToPouch(dog.points * dogMultiplier);
        dogMultiplier++;
    }

    private void ResetMultiplier()
    {
        dogMultiplier = 1;
    }

    public void SquirrelCaught()
    {
        this.squirrel.gameObject.SetActive(false);

        SetLives(this.lives - 1);

        if (this.lives > 0) {
            Invoke(nameof(ResetState), 3.0f);
        } else {
            GameOver();
        }

    }

    public void PickupNut(Nut nut)
    {
        nut.gameObject.SetActive(false); // disable the nut so it can't be picked up again

        addToPouch(nut.score); // Add the nut's value to the pouch

        // TODO: Check if the squirrel has pickup up all the nuts?
        // Maybe that should be done when the squirrel returns to home base or runs out of time.
    }

    public void PickupAcorn(Acorn acorn)
    {
        // TODO: Trigger dogs to be defeatable
        
        PickupNut(acorn);
        CancelInvoke();
        Invoke(nameof(ResetMultiplier), acorn.duration);
    }

    private bool HasRemainingNuts()
    {
        foreach (Transform nut in nuts)
        {
            if (nut.gameObject.activeSelf) return true;
        }
        return false;
    }

    // SCORE FUNCTIONS *******************************************************

    private void addToPouch(int score)
    {
        setPouch(pouch + score);
    }

    // Takes everything from the pouch and puts it in the stockpile
    private void setPouch(int score)
    {
        this.pouch = score;
    }

    public void depositPouch()
    {
        setStockpile(stockpile + pouch);
        pouch = 0;
    }

    private void setStockpile(int score)
    {
        stockpile = score;
    }
}
