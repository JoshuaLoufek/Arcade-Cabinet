using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int health;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(float damage)
    {
        health -= (int)damage;
        if (health < 0) Destroy(this.gameObject);
    }
}
