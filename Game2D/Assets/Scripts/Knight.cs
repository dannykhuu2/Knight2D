using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Knight : MonoBehaviour
{
    // Rigid Body 2D.
    Rigidbody2D rigidbody2d;
    public static Knight Player { get; set; }

    // Health and Damage
    public const int maxHealth = 100;
    public const int maxMana = 50;
    Renderer KnightRender;
    public int currentMana;
    public int currentHealth;
    public int health
    {
        get
        {
            return currentHealth;
        }
    }
    public int mana
    {
        get
        {
            return currentMana;
        }
    }

    // respawn locations:
    Vector3 start;
    public Vector3 respawnpoint;

    // Death
    public bool Dead;

    // invinicible from dmg.
    float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;

    // Movement;
    float speed = 5.0f;
    float smoothing = 0.05f;
    Vector3 reference = Vector3.zero;
    //bool isCrouch;

    // Jump height.
    float jumpheight = 400.0f;
    bool isGrounded = false;
    public Transform groundCheckRight;
    public Transform groundCheckLeft;
    float groundRadius = 0.25f;

    // Animator.
    new Animator animation;
    Vector2 lookDirection = new Vector2(1, 0);

    // Fireball attack.
    public GameObject fireball;
    float startcooldown;
    float cooldown = 4.0f;
    bool startTimer;

    // Normal attacks.
    int layercontact; // check the layer that is triggered by contact.
    public bool attack;
    float attackCd = 0.6f;
    float attackTimer;
    int damagedealt;

    // Block invincibility.
    bool isBlock;
    float Blocktime = 1.0f;

    //Hidden Reveal
    public GameObject reveal;
    SpriteRenderer revealRender;
    bool startTimerReveal;
    float startCooldownReveal;
    readonly float cooldownReveal = 12.0f;
    readonly float durationReveal = 5.0f;
    public float durationTimer;

    //Gate
    Collectible collectible;
    public GameObject torches;

    // BGM Control
  /*  public AudioClip MiniBossBGM;
    public AudioSource AudioPlayer;
    public AudioClip MainBGM;
    public AudioClip JumperBGM;
    bool played; // check if a BGM was set.
    bool audioreset; // check if main BGM was played again.
    Vector3 MiniBossLocation = new Vector3(110f, -12f, 0f); */


    // Start is called before the first frame update
    void Start()
    {
        Player = this;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animation = GetComponent<Animator>();
        KnightRender = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        currentMana = maxMana;
        startTimer = false;
        startTimerReveal = false;
        reveal.SetActive(false);
        attack = false;
        Player.Dead = false;
        revealRender = reveal.GetComponent<SpriteRenderer>();
        start = transform.position; // initial position.
        Player.respawnpoint = start; // initial respawn point
        FireballUI.FireballBar.SetValue(startcooldown / (float)cooldown); // initialize the fireball cd ui to be ready.
        RevealUI.RevealBar.SetValue(startCooldownReveal / (float)cooldownReveal);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Movement
        float horizontalMove = Input.GetAxis("Horizontal") * speed;
        Vector2 position = rigidbody2d.position;
        Vector2 movement = new Vector2(horizontalMove, 0);

        // Setting direction for movement.
        if (!Mathf.Approximately(movement.x, 0.0f))
        {
            lookDirection.Set(movement.x, movement.y);
            lookDirection.Normalize();
        }

        // Movement.
        animation.SetFloat("Look X", lookDirection.x);
        animation.SetFloat("Speed", movement.magnitude);

        // Ray cast object.
        Vector2 RayStart = new Vector2(transform.position.x, transform.position.y + 0.1f);
        RaycastHit2D Hit = Physics2D.Raycast(RayStart, Vector2.down, 0.5f, LayerMask.GetMask("Tile"));
        Debug.DrawRay(RayStart, Vector2.down, Color.red);

        // Jumping 
        // Constantly test if the character is touching the ground. 
        if (Mathf.Approximately(lookDirection.x, -1))
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, groundRadius, LayerMask.GetMask("Tile"));
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckRight.position, groundRadius, LayerMask.GetMask("Tile"));
        }

        animation.SetBool("Jump", !isGrounded);

        // Sprint on holding shift
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            speed = 7.0f;
        }
        else
        {
            speed = 5.0f;
        }

        // Delaying attack.
        if (attack)
        {
            if (attackTimer > 0)
            {
                attackTimer -= Time.fixedDeltaTime;
            }
            else
            {
                attack = false;
            }
        }

        // ------ Fireball -----------
        // begin countdown timer if the skill has been casted.
        if (startTimer == true)
        {
            startcooldown -= Time.fixedDeltaTime;
            FireballUI.FireballBar.SetValue(startcooldown / (float)cooldown);
            if (startcooldown <= 0f) // reset when timer is done.
            {
                startTimer = false;
            }
        }

        // Can be casted if not under cd with a mana pool of at least 10.
        if (Input.GetKeyDown(KeyCode.A) && currentMana >= 10 && startTimer == false)
        {
            startTimer = true;
            animation.SetBool("Fireball", true);
            FireBallAttack();
            changeMana(-10);
            startcooldown = cooldown;
        }

        // movement
        Vector3 targetVelocity = new Vector2(horizontalMove, rigidbody2d.velocity.y);

        if (Hit.collider != null)
        {
            float angle = Vector2.Angle(Hit.normal, lookDirection);
            rigidbody2d.gravityScale = 1;
            jumpheight = 400.0f;
            if (!Mathf.Approximately(angle, 90))
            {
                jumpheight = 600f;
                rigidbody2d.gravityScale = 3;
            }
        }

        if (Knight.Player.Dead == false) // can only move when alive.
        {
            rigidbody2d.velocity = Vector3.SmoothDamp(rigidbody2d.velocity, targetVelocity, ref reference, smoothing);
        }
        else
        {
            rigidbody2d.velocity = Vector3.zero;
        }

        // Invincibility;
        if (isInvincible)
        {
            invincibleTimer -= Time.fixedDeltaTime;
            if (isBlock == true)
            {
                rigidbody2d.velocity = Vector2.zero;
            }
            if (invincibleTimer < 0)
            {
                isInvincible = false;
                // Reset block.
                if (isBlock == true)
                {
                    animation.SetBool("Block", false);
                    isBlock = false;
                }
            }
        }
    }

    private void Update()
    {
        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded == true) // && isCrouch == false)
        {
            rigidbody2d.velocity = new Vector2(0f, 0f);
            rigidbody2d.AddForce(new Vector2(0f, jumpheight));
        }

        // Attacking 
        if (Input.GetKeyDown(KeyCode.X) && attack == false)
        {
            NormalAttack(); // Set to attack to true.
            attackTimer = attackCd;
            damagedealt = -25;
            // Checking if the knight is grounded to trigger specific attacks.
            if (isGrounded == true)
            {
                animation.SetTrigger("Attack");
            }
            else
            {
                animation.SetTrigger("JumpAttack");
            }
        }

        // Trigger Death
        if (currentHealth == 0)
        {
            isInvincible = true; // set invincibility frames.
            invincibleTimer = timeInvincible * 2; 
            animation.SetTrigger("Death"); // trigger death.
            StartCoroutine("FadeDeath"); // fade away then respawn else where.
            // reset health and mana values.
            changeHealth(maxHealth);
            changeMana(maxMana);
        }

        // Reveal
        if (startTimerReveal == true)
        {
            startCooldownReveal -= Time.fixedDeltaTime;
            RevealUI.RevealBar.SetValue(startCooldownReveal / (float)cooldownReveal);
            durationTimer -= Time.fixedDeltaTime;
            if (startCooldownReveal <= 0f)
            {
                startTimerReveal = false;
            }
            if (durationTimer <= 0f)
            {
                reveal.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.D) && startTimerReveal == false)
        {
            reveal.SetActive(true);
            StartCoroutine("FadeReveal");
            startTimerReveal = true;
            startCooldownReveal = cooldownReveal;
            durationTimer = durationReveal;

        }

        // Opening Gate (Reactivate when the player walks into the room and awakens the final boss)
        if (Input.GetKeyDown(KeyCode.S))
        {
            RaycastHit2D gateHit = Physics2D.Raycast(transform.position + (Vector3.up * 0.4f), lookDirection, 1.0f, LayerMask.GetMask("Gate"));
            RaycastHit2D signHit = Physics2D.Raycast(transform.position + (Vector3.up * 0.2f), lookDirection, 1.0f, LayerMask.GetMask("Sign"));
            if (gateHit.collider != null)
            {
                GameObject gateTile = gateHit.collider.gameObject;
                if (Collectible.collected == 5) 
                {
                    gateTile.SetActive(false);
                    torches.SetActive(false);
                }
            }
            if (signHit.collider != null)
            {
                Sign sign = signHit.collider.GetComponent<Sign>();
                if(sign != null)
                {
                    sign.DisplayDialog();
                }
            }
        }
        // Crouch
        /*   if (Input.GetKey(KeyCode.LeftControl) && isGrounded == true)
           {
               isCrouch = true;
               rigidbody2d.velocity = Vector3.zero; // stop movement;
               rigidbody2d.angularVelocity = 0;
           }
           else
           {
               isCrouch = false;
           }
           animation.SetBool("Crouch", isCrouch);*/

        // Block
        if (Input.GetKeyDown(KeyCode.Z) && isInvincible == false && isGrounded == true)
        {
            animation.SetBool("Block", true);
            isInvincible = true;
            isBlock = true;
            invincibleTimer = Blocktime;
        }

    }

    // HP
    public void changeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
            {
                return;
            }
            isInvincible = true;
            invincibleTimer = timeInvincible;
            animation.SetTrigger("Hit");
        }
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        HealthUI.HealthBar.SetValue(currentHealth / (float)maxHealth);
    
       
    }

    public void changeMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        ManaUI.ManaBar.SetValue(currentMana / (float)maxMana);
    }

    private void FireBallAttack()
    {
        Vector2 spawnposition = new Vector2(1.5f, 1.0f);
        Vector2 initialposition = rigidbody2d.position;
        // flip the spawn position.
        if (lookDirection.x < 0f)
        {
            spawnposition.x = -spawnposition.x;
        }
        GameObject fireballspawn = Instantiate(fireball, initialposition + spawnposition, Quaternion.identity);
        Fireball fireballattack = fireballspawn.GetComponent<Fireball>();
        fireballattack.Launch(lookDirection, 300, initialposition, this.gameObject);
    }

    // retrieve the layer upon contact.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //   Debug.Log(collision.gameObject.name + collision.gameObject.layer);
        layercontact = collision.gameObject.layer;
        if (layercontact == 10)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (attack == true)
            {
                enemy.changeHealth(damagedealt);
                enemy.SetAggro();
            }
        }
    }

    // Set normal attack var to true.
    public bool NormalAttack()
    {
        attack = true;
        return attack;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.layer);
    }

    IEnumerator FadeDeath()
    {
        Player.Dead = true;
        for (float f = 1f; f >= 0f; f -= 0.1f)
        {
            Color color = KnightRender.material.color;
            color.a = f;
            KnightRender.material.color = color;
            yield return new WaitForSeconds(.1f);
        }
        KnightRender.material.color = new Color(1f, 1f, 1f, 0f); // full transparernt.
        yield return new WaitForSeconds(1.0f);
        StartCoroutine("Respawn");
    }

    IEnumerator Respawn()
    {
        animation.SetTrigger("Respawn");
        rigidbody2d.transform.position = respawnpoint; // teleport to a checkpoint.
        for (float f = 0f; f <= 1f; f += 0.1f) // fade back in the color.
        {
            Color color = KnightRender.material.color;
            color.a = f;
            KnightRender.material.color = color;
            yield return new WaitForSeconds(.1f);
        }
        Player.Dead = false;
        KnightRender.material.color = new Color(1f, 1f, 1f, 1f); // fully visible.
    }

    IEnumerator FadeReveal()
    {
        Color original = revealRender.material.color;
        for (float f = 1.0f; f >= 0; f -= 0.1f)
        {
            Color c = revealRender.material.color;
            c.a = f;
            revealRender.material.color = c;
            yield return new WaitForSeconds(.15f);
        }
        for (float f = 0; f <= 1.0f; f += 0.1f)
        {
            Color c = revealRender.material.color;
            c.a = f;
            revealRender.material.color = c;
            yield return new WaitForSeconds(.2f);
        }
        revealRender.material.color = original;

    }

}