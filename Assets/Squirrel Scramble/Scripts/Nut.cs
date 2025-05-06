using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nut : MonoBehaviour
{
    [SerializeField] int score;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Squirrel squirrel = collision.gameObject.GetComponent<Squirrel>();
        if (squirrel != null)
        {
            // give points
            squirrel.pickupPoints(score);
            // disable pellet
            this.gameObject.SetActive(false);
        }
    }
}
