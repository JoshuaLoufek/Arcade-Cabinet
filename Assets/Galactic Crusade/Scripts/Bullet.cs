using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody2D rb;
    public float damage = 1f;

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
        this.transform.position = origin.position + new Vector3(0.0625f, 0f, 0f);
    }

    private void FixedUpdate()
    {
        MoveBullet();
        CheckLifespan();
    }
    
    private void MoveBullet()
    {
        rb.velocity = direction * projectileSpeed;
    }

    private void CheckLifespan()
    {
        if (lifeTimer > lifespan) Destroy(this.gameObject);
        else lifeTimer += Time.deltaTime;
    }

    // Triggered when this bullet collides with something
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageableObject = collision.gameObject.GetComponent<IDamageable>();
        if (damageableObject != null)
        {
            damageableObject.Damage(damage);
        }
    }
}
