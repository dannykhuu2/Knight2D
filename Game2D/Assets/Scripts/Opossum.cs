using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Opossum : Enemy
{
    public float rayLengthXO = 2.0f;    
    public float rayLengthYO = 2.0f;
   
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentHealth > 0) // while still alive.
        {
            if (isAggro == false)
            {
                Move();
            }
            else if (isAggro == true)
            {
                MoveAggro(Knight.Player, rayLengthXO, rayLengthYO);
            }
        }
        else // play death anim.
        {
            if (DeathAnimSet == false)
            {
                DeathAnimTimer = AnimTimer;
                DeathAnimSet = true;
            }
            DeathAnimation();
        }
    }

    void OnTriggerStay2D(Collider2D collision) 
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
