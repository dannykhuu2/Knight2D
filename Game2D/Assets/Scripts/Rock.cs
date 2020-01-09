using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    Rigidbody2D rigidbody2D;
    float invinc = 1.0f;
    float invincTimer;
    float despawnTimer = 8.0f;
    // Start is called before the first frame update
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        invincTimer = invinc;
        rigidbody2D.velocity = Vector3.down * 6.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        despawnTimer -= Time.deltaTime;
        if(invincTimer > 0)
        {
            invincTimer -= Time.deltaTime;
        }
        if(despawnTimer < 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 8)
        {
            Knight player = collision.GetComponent<Knight>();
            if(player != null)
            {
                player.changeHealth(-25);
                Destroy(gameObject);
            }
           
        }
        else if(collision.gameObject.layer == 11 && invincTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

}
