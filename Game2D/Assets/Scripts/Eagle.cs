using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eagle : Enemy
{
    public float floatHeight = 3.0f;
    public float liftForce = 5.0f;
    public float damping =0.2f;
    public float rayLengthXEagle = 2.0f;
    public float rayLengthYEagle = 8.0f;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentHealth > 0) // while still alive, can move.
        {
            if (isAggro == false)
            {
                Vector3 vector = new Vector2(0, -1);
                Vector3 vectorDir = new Vector2(direction, 0);
                RaycastHit2D hit = Physics2D.Raycast(transform.position + (vectorDir * 0.4f), -Vector2.up, 8.0f, LayerMask.GetMask("Tile", "Obstacle"));
                if (hit.collider != null)
                {
                    float distance = Mathf.Abs(hit.point.y - transform.position.y);
                    float heightError = floatHeight - distance;
                    float force = liftForce * heightError - rigidbody2D.velocity.y * damping;
                    rigidbody2D.AddForce(Vector3.up * force);
                }
                else
                {
                    direction = -direction;
                    timer = changeTime;
                }
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    direction = -direction;
                    timer = changeTime;
                }
                Vector2 position = rigidbody2D.position;
                position.x = position.x + Time.deltaTime * speed * direction;
                animator.SetFloat("LookX", direction);
                rigidbody2D.MovePosition(position);
            }
            else
            {
                MoveAggro(Knight.Player, rayLengthXEagle, rayLengthYEagle);
            }
        }
        else
        {
            if (DeathAnimSet == false)
            {
                DeathAnimTimer = AnimTimer;
                DeathAnimSet = true;
            }
            DeathAnimation();
        }
 
    }
    void OnTriggerStay2D(Collider2D collision) // Use trigger or not?
    {
        Knight player = collision.gameObject.GetComponent<Knight>();
        if (player != null)
        {
            if (player.attack == false)
            {
                player.changeHealth(damage);
            }
        }

    }
}  
