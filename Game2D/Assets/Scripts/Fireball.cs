using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    Rigidbody2D rigidbody2D;
    Animator animate;
    GameObject FballCaster;
    // Distance from caster 
    Vector2 position;

    // Timer for hit animation;
    float hitTimer;
    const float hitTime = 0.600f;
    bool hit;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animate = GetComponent<Animator>();
        animate.SetBool("Hit", false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        CheckRange();

        // if hit display hit animation.
        if (FballCaster != null)
        {
            if (hit == true)
            {
                hitTimer -= Time.fixedDeltaTime;
                if (hitTimer < 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        else  // caster has died , destroy their cast.
        {
            Destroy(gameObject);
        }

    }

    // Move
    public void Launch(Vector2 direction, float force, Vector2 initialposition, GameObject caster) // for knight.
    {
        FballCaster = caster;
        position = initialposition;
        animate.SetFloat("Look Direction", direction.x);
        rigidbody2D.AddForce(direction * force);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    { 
        if (FballCaster.gameObject.layer != 10) // own fireball cannot deal damage to own caster.
        {
            if (collision.gameObject.layer == 10) // monster. 
            {
                HitSetUp();
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                enemy.changeHealth(-50);
            }
        } 
    }

    void HitSetUp()
    {
        hitTimer = hitTime;
        hit = true;
        rigidbody2D.isKinematic = true; // remove physics.
        rigidbody2D.velocity = Vector3.zero;
        animate.SetBool("Hit", true);
        Collider2D collider = GetComponent<BoxCollider2D>();
        collider.enabled = false; // disable colliderr.
    }

    void CheckRange()
    {
        // Range of fireball 
        float differencex = Mathf.Abs(rigidbody2D.position.x - position.x);
        if (FballCaster.gameObject.name.Equals("Knight"))
        {
            if (differencex > 10.0f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (differencex > 20.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    // Destroy fireball upon hitting a surface.
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (FballCaster != null) // if enemy exists.
        {
            if (FballCaster.gameObject.layer == 8)
            {
                if (collision.gameObject.layer != 8) // if not colliding with knight.
                {
                    HitSetUp(); // hitting an obstacle.
                }
            }
        }

        if (FballCaster.gameObject.layer == 10) // if casted by the boss, then check if it hits knight.
        {
            if (collision.gameObject.layer == 8) // knight.
            {
                Knight player = collision.gameObject.GetComponent<Knight>();
                player.changeHealth(-20);
            }
            HitSetUp();
        }
    }
}
