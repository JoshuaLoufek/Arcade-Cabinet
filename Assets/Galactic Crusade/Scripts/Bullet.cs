using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D rb;

    public float projectileSpeed = 5f;
    private Vector2 direction;

    public float lifespan = 3f;
    public float lifeTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        direction = new Vector2(0, 1); // set the direction to always be up
        lifeTimer = 0f;
    }

    public void InitializeBullet(Transform origin)
    {
        this.transform.position = origin.position;
    }

    private void FixedUpdate()
    {
        MoveBullet();
        TrackBulletLifetime();
    }
    
    private void MoveBullet()
    {
        rb.velocity = direction * projectileSpeed;
    }

    private void TrackBulletLifetime()
    {
        if (lifeTimer > lifespan) Destroy(this.gameObject);
        else lifeTimer += Time.deltaTime;
    }

    // Triggered when this bullet collides with something
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet collision detected");

    }
}
