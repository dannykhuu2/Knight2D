using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    //Default values subject to change
    public int maxHealth = 100;
    public float speed = 2.0f;
    public float changeTime = 2.0f;
    public int damage = -10;
    public float changeTimeIdle = 2.0f;
    public float range = 8.0f;
    public Canvas HealthSystem;

    protected int currentHealth;
    protected float timer;
    protected int direction =1;
    protected float idleTimer;
    protected bool isAggro;
    protected bool isGroundTrigger;

    protected Rigidbody2D rigidbody2D;    
    protected Animator animator;
    protected float DeathAnimTimer;
    protected float AnimTimer = 0.667f;
    protected bool DeathAnimSet;

    HealthUI HealthBar;
    public Slider healthFill;
    

    public void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        timer = changeTime; 
        idleTimer = changeTimeIdle;
        isGroundTrigger = false;
        DeathAnimSet = false;
        if (HealthSystem != null)
        {
            HealthBar = GetComponent<HealthUI>();
            HealthSystem.enabled = false;
        }
    }
    public void Move()
    {
        
        if (idleTimer < 0 && isAggro == false)
        {

            Vector3 vector = new Vector2(direction, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3.down * 0.4f), vector, 2.0f, LayerMask.GetMask("Tile","Obstacle"));
            RaycastHit2D hitY = Physics2D.Raycast(transform.position + (vector * 0.4f), Vector2.down, 2.0f, LayerMask.GetMask("Tile","Obstacle"));
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

            }
            if (idleTimer < 0)
            {
                Vector2 position = rigidbody2D.position;
                position.x = position.x + Time.deltaTime * speed * direction;
                animator.SetFloat("LookX", direction);
                rigidbody2D.MovePosition(position);
            }
        }
        else if(idleTimer > 0 && isAggro == false)
        {
            idleTimer -= Time.deltaTime;
        }
    }

    public void changeHealth(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth + damage, 0, maxHealth);
        if (HealthSystem != null)
        {
            HealthSystem.enabled = true; // enable the health bar when mob takes damage.
            HealthBar.SetValue(currentHealth / (float)maxHealth);
        }
        else
        {
            healthFill.value = currentHealth / (float)maxHealth;
        }
        if (currentHealth == 0 && HealthSystem != null)
        {
            animator.SetBool("Death", true);
        }
        else if(currentHealth == 0 && HealthSystem == null) //Idk if this will work at all to be honest
        {
            StartCoroutine("ColorChangeLonger");
            animator.SetBool("Death", true);
        }
    }

    public void MoveAggro(Knight player, float rayLengthX, float rayLengthY)
    {
        float difference = Vector2.Distance(transform.position, player.transform.position);
        //Direction aggro might be larger than 1? idk
        float directionAggro = Mathf.Sign(player.transform.position.x - transform.position.x);
        if (difference <= range)
        {    
            Vector3 vector = new Vector2(directionAggro, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3.down * 0.4f), vector, rayLengthX, LayerMask.GetMask("Tile","Obstacle"));
            RaycastHit2D hitY = Physics2D.Raycast(transform.position + (vector * 0.4f), Vector2.down, rayLengthY, LayerMask.GetMask("Tile","Obstacle"));
            if (hit.collider != null || hitY.collider == null)
            {
                isAggro = false;
                direction = -direction;
                timer = changeTime;
            }
            if(isAggro == true)
            {
                float step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), step);
                animator.SetFloat("LookX", directionAggro); //This still does not take into account the enemy walking into walls 
            }
  
        }
        else
        {
            isAggro = false;
        }

    }

    public void SetAggro()
    {
        isAggro = true;
    }

    public void DeathAnimation()
    {
        rigidbody2D.velocity = Vector3.zero; // stop all movement.
        Collider2D collider = GetComponent<BoxCollider2D>(); // disable collider so death animation doesnt deal dmg.
        collider.enabled = false;
        DeathAnimTimer -= Time.fixedDeltaTime;// continue animation while timer duration lasts.
        if (DeathAnimTimer < 0)
        {
            Destroy(gameObject);
        }
    }

}