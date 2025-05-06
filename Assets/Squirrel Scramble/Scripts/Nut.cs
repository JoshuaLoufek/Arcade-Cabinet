using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nut : MonoBehaviour
{
    public int score = 10;

    protected virtual void Pickup()
    {
        FindObjectOfType<GameManager>().PickupNut(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Squirrel squirrel = collision.gameObject.GetComponent<Squirrel>();
        if (squirrel != null)
        {
            Pickup();
        }
    }
}
