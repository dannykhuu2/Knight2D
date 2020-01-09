using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : Enemy
{
    public float rayLengthXF = 2.0f;
    public float rayLengthYF = 6.0f;
    bool isIdle;
    float directionJump;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Debug.Log(isAggro);
        if (currentHealth > 0)
        {
            if (isAggro == false)
            {
                if (idleTimer < 0)
                {
                    isIdle = false;
                    Vector2 vector = new Vector2(direction, 0);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, vector, 2.0f, LayerMask.GetMask("Tile","Obstacle"));
                    RaycastHit2D hitY = Physics2D.Raycast(transform.position + (Vector3.up * 0.8f), Vector2.down, 4.0f, LayerMask.GetMask("Tile","Obstacle"));
                    Debug.DrawRay(transform.position, Vector2.down, Color.blue);

                    timer -= Time.deltaTime;

                    if (hit.collider != null || hitY.collider == null)
                    {
                        direction = -direction;
                        timer = changeTime;
                    }
                    if (timer < 0)
                    {
                        direction = -direction;
                        timer = changeTime;
                        idleTimer = changeTimeIdle;
                        isIdle = true;

                    }
                    if (idleTimer < 0)
                    {
                        Vector2 position = rigidbody2D.position;
                        position.x = position.x + Time.deltaTime * speed * direction;
                        animator.SetFloat("LookX", direction);
                        rigidbody2D.MovePosition(position);
                    }

                }
                else
                {
                    idleTimer -= Time.deltaTime;
                }
                animator.SetBool("Idle", isIdle);
            }
            else
            {
                MoveAggro(Knight.Player, rayLengthXF, rayLengthYF);
                directionJump = Mathf.Sign(Knight.Player.transform.position.x - transform.position.x);
                animator.SetBool("Idle", false);
                animator.SetFloat("LookX", directionJump);

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
