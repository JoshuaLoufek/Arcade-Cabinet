using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dog[] dogs;
    public Squirrel squirrel;
    public Transform nuts;

    public int score { get; private set; }
    public int lives { get; private set; }

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
        SetScore(0);
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
        for (int i = 0; i < dogs.Length; i++)
        {
            this.dogs[i].gameObject.SetActive(true);
        }

        this.squirrel.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        for (int i = 0; i < dogs.Length; i++)
        {
            this.dogs[i].gameObject.SetActive(false);
        }

        this.squirrel.gameObject.SetActive(false);
    }

    private void SetScore(int score)
    {
        this.score = score;
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void DogDefeated(Dog dog)
    {
        SetScore(this.score + dog.points);
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
}
