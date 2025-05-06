using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acorn : Nut
{
    public float duration = 8.0f;

    protected override void Pickup()
    {
        FindObjectOfType<GameManager>().PickupAcorn(this);
    }
}
