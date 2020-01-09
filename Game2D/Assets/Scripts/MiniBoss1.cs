    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MiniBoss1 : Enemy
{

    // Movement generator.
    Vector3 reference = Vector3.zero;
    float MoveTimer = 3.0f;
    float Timer;
    float moveSpeedX;
    float moveSpeedY;
    float Smoothing = 0.05f;
    Vector3 StartPos;

    // fireball.
    float FBAnimTimer = 0.833f;
    float DelayTimer; // time between each casted fireball.
    float FireballTimer; // time for continuous fireball cast.
    float FireBallCD = 10.0f; // time counting down to next fireball cast.
    float TimerAnimFB;
    bool FireballReady; // check if fireball is ready to be casted.
    float TimerFBCast;
    float FBCastDuration = 10.0f;
    float CastDelay = 2.0f;
    float CastDelayP2 = 1.0f;
    float TimerFBCD;
    bool attackAgain; // To initiate attack after a delay.

    // Targeting.
    float attackRange = 3.0f;
    float attackTimer;
    float attackDelay = 2.5f; // delay between each attacks.
    float attackDelayP2 = 1.5f; // delay during p2.
    bool attack;
    float attackAnimTimer;
    float attackanim = 1.0f;
    float attackanimp2 = 0.833f;
    float chaseSpeed = 3.0f;
    int attackCount; // for phase 2 after 2 normal attacks, trigger spikes.

    // fireball prefab.
    public GameObject fireball;

    // Trigger awake.
    float BattleVicinity = 10.0f;
    bool BattleStart;

    // Phase 2.
    bool Death1;
    float DeathanimTimer1 = 0.867f;
    float DeathV2 = 1.600f;
    float EvolveTime;
    float EvolveTimer;
    float EvolveAnim = 1.625f;
    int Phase2Hp = 200;
    bool Phase2;
    bool Evolving; // in evolving animation.

    // Spike attacks.
    public Collider2D SpikeCollider;
    float SpikeAttackDelay = 1.5f;
    float SpikeAnim = 1.0f;
    bool SpikeAttack;

    // max Health;
    int BossMaxHealth = 500;

    // Portal spawn after death.
    public GameObject Portal;
    TilemapRenderer PortalRenderer;
    Collider2D PortalCollider;

    // gate to prevent running off map.
    public GameObject Gate;
    TilemapRenderer GateRenderer;
    Collider2D GateCollider;

    // boss battle music.
    public AudioClip BossBGM;
    public AudioSource AudioPlayer;
    public AudioClip NormalBGM;
    bool played;
    bool audioreset;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        StartPos = transform.position; // start position to return it there when knight dies.
        FullReset();
        played = false;
        audioreset = false;
        GateRenderer = Gate.GetComponent<TilemapRenderer>();
        GateCollider = Gate.GetComponent<TilemapCollider2D>();
    } 

    // Update is called once per frame
    void FixedUpdate()
    {
        // check if knight is within a vicinity, if so then awaken the boss.
        if (Physics2D.OverlapCircle(rigidbody2D.position, BattleVicinity / 2, LayerMask.GetMask("Knight")) && BattleStart == false)
        {
            BattleStart = true;
            HealthSystem.enabled = true; // display health system upon entering the battle.
            changeHealth(0); // --> brings up UI.
            ChangeMovement(); // start moving upon awake.
        }
        else if (BattleStart == false)
        {
            animator.SetFloat("Look X", -1); 
            rigidbody2D.Sleep();
            GateRenderer.enabled = false; // disable the gate if not awaken yet.
            GateCollider.enabled = false;
            HealthSystem.enabled = false;
        }

        if (BattleStart == true) // if monster is awake.
        {
            // plays the audio when the boss fight begins.
            if (played == false)
            {
                AudioPlayer.clip = BossBGM;
                AudioPlayer.Play();
                played = true;
                audioreset = false; // disable audio reset. as boss bgm starts.
            }

            GateRenderer.enabled = true; // enable the gate to enclose exit.
            GateCollider.enabled = true;

            if (currentHealth > 0) // if it is alive.
            {
                // If knight has been knocked out, restart everything.
                if (Knight.Player.Dead == true)
                {
                    StartCoroutine("ResetTimer");
                }

                animator.SetFloat("Look X", Mathf.Sign(moveSpeedX)); // set look direction.
                if (currentHealth <= Phase2Hp && Phase2 == false) // trigger phase 2 at this health threshold.
                {
                    Evolving = true; // begins transforming.
                    rigidbody2D.velocity = Vector3.zero;
                    BeginPhase2(); // trigger evolution.
                }

                if (Evolving == false) // if in evolving phase don't trigger any attacks or aggro.
                {
                    // moves when fireball is on cd.
                    if (TimerFBCD > 0)
                    {
                        // Check for player within vinicity for aggro during movement frame.
                        Collider2D ColliderChecker = Physics2D.OverlapCircle(rigidbody2D.position, BattleVicinity, LayerMask.GetMask("Knight"));
                        // -------- Aggro ----------
                        if (ColliderChecker != null) // if found and knight is not near a wall collider, chase him.
                        {
                            // retrieve position of knight.
                            Knight knight = ColliderChecker.gameObject.GetComponent<Knight>();
                            Rigidbody2D knightbody = knight.GetComponent<Rigidbody2D>();
                            Vector2 Target = new Vector2(knightbody.position.x, knightbody.position.y + 1.5f);
                            // Approach the knights position, when in a certain range from the knight trigger the attack animation.
                            float step = chaseSpeed * Time.deltaTime;
                            animator.SetBool("Idle", false); // start moving.
                            if (Phase2 == true)
                            {
                                step = step * 1.20f; // move 1.2x faster when evolved.
                            }
                            rigidbody2D.position = Vector2.MoveTowards(rigidbody2D.position, Target, step);
                            animator.SetFloat("Look X", Mathf.Floor(Mathf.Sign(Target.x - rigidbody2D.position.x)));

                            // within a certain distance, trigger the attack.
                            if (Vector2.Distance(rigidbody2D.position, knightbody.position) <= attackRange) // within an X and Y range.
                            {
                                if (attack == false) // if havent attacked yet, trigger the attack and start cd.
                                {
                                    if (attackCount == 2) // if normal atk twice, then check if phase 2.
                                    {
                                        if (Phase2 == true) // can trigger spikes if in phase 2.
                                        {
                                            Spikes();
                                        }
                                        attackCount = 0; // reset
                                    }
                                    else
                                    {
                                        Attack();
                                        attackCount++;
                                    }
                                }
                            }
                        }
                        // if not colliding with anything and moving timer interval ran out, then move.
                        if (Timer < 0 && ColliderChecker == null) // if movement timer runs out, change its movement
                        {
                            ChangeMovement();
                            Timer = MoveTimer;
                        }
                        // Movement timer before changing.
                        Timer -= Time.fixedDeltaTime;
                    }

                    // attacked alrdy, countdown the cd.
                    if (attack == true)
                    {
                        attackAnimTimer -= Time.fixedDeltaTime;
                        attackTimer -= Time.fixedDeltaTime;
                        if (attackAnimTimer < 0) // reset anim.
                        {
                            if (SpikeAttack == true) // if spike attack then reset for that.
                            {
                                animator.SetBool("Spikes", false);
                            }
                            else // otherwise it is normal attack.
                            {
                                animator.SetBool("Attack", false); 
                            }
                        }
                        if (attackTimer < 0) // cd complete reset,
                        {
                            attack = false;
                            if (SpikeAttack == true)
                            {
                                SpikeAttack = false;
                            }
                        }
                    }

                    if (SpikeAttack == true) // condition to use spike attack.
                    {
                        SpikeCollider.enabled = true;
                    }
                    else
                    {
                        SpikeCollider.enabled = false; // disable spike collider unless using spike attack.
                    }


                    // -------- FIREBALL ATTACK ----------
                    // countdown to next cast of fireballs.
                    TimerFBCD -= Time.fixedDeltaTime;

                    // when the cd is complete, fireball attacks will be casted.
                    if (TimerFBCD < 0 && FireballReady == false)
                    {
                        rigidbody2D.velocity = Vector2.zero; // Kill the movement of the character.
                        animator.SetBool("Idle", true); // animation is idle pos.
                        SetUpFireBallAttack(); // setup for fireball chain.
                        TimerFBCast = FBCastDuration; // Duration of this entire attack chain.
                        FireballReady = true; // ready to start attack chain.
                    }

                    // fireball phase starts.
                    if (FireballReady == true) // timer is set ... countdown.
                    {
                        TimerFBCast -= Time.fixedDeltaTime;
                        if (TimerFBCast > 0) // while there is still time to cast fireballs, continue it.
                        {
                            TimerAnimFB -= Time.fixedDeltaTime; // countdown fireball animation.
                            if (TimerAnimFB < 0 && attackAgain == true) // when timer is finished, launch the attack.
                            {
                                FireBallAttack();
                                animator.SetBool("Fireball", false); // not attacking anymore, go idle.
                                if (Phase2 == true) // setting up delay btween fireballs.
                                {
                                    DelayTimer = CastDelayP2;
                                }
                                else
                                {
                                    DelayTimer = CastDelay;
                                }
                                
                                attackAgain = false;
                            }

                            if (TimerFBCast - DelayTimer > TimerAnimFB + 0.35f) // if there is still time left after considering delay, then execute. --> Checking if it is greater than animation timer as it must be factored in during the entire cast anim.
                            {
                                if (DelayTimer > 0) // delay taking place.
                                {
                                    DelayTimer -= Time.fixedDeltaTime;
                                    if (DelayTimer < 0) // timer is complete. then reset attack.
                                    {
                                        SetUpFireBallAttack(); // replay the attack.
                                    }
                                }
                            }
                        }
                        else
                        {
                            animator.SetBool("Fireball",false);
                            animator.SetBool("Idle", false); // set it to move again.
                            ChangeMovement(); // start movement.
                            Timer = MoveTimer; // move timer.
                            TimerFBCD = FireBallCD;
                            FireballReady = false;
                        }

                    }
                }
            }
            else // 0 hp initiate death.
            {
                GateRenderer.enabled = false; // disable the gate.
                GateCollider.enabled = false;
                if (DeathAnimSet == false)
                 {
                    Debug.Log("Death Anim Played");
                    DeathAnimTimer = DeathV2;
                    DeathAnimSet = true;
                }
                DeathAnimation();
                // Spawn portal afer death.
                PortalRenderer = Portal.GetComponent<TilemapRenderer>();
                PortalRenderer.enabled = true;
                PortalRenderer.material.color = new Color(1f, 1f, 1f, 0f);
                Knight.Player.respawnpoint = new Vector3(115f, 9.0f, 0f); // tree as new respawn point.
            }
        }
    }

    void SetUpFireBallAttack()
    {
        animator.SetBool("Fireball", true);
        TimerAnimFB = FBAnimTimer; // Set Timer for animation to cast.
        attackAgain = true; // initial condition to enable first attack.
    }

    void ChangeMovement()
    {
        // initial moveSpeed is randomly generated.
        moveSpeedX = Mathf.Floor(Random.Range(-5.0f, 5.0f)); // random speed between 1 to 5.
        moveSpeedY = Mathf.Floor(Random.Range(-2.0f, 2.0f));
        Vector2 Movement = new Vector2(moveSpeedX, moveSpeedY); // Movement vector.
        if (Phase2 == true) // if phase 2 move 1.5x faster.
        {
            Movement *= 2.0f;
        }
        // Move the character.
        rigidbody2D.velocity = Vector3.SmoothDamp(rigidbody2D.velocity, Movement, ref reference, Smoothing);
        // Timer begins for movement.
        Timer = Mathf.Floor(Random.Range(1.0f, 3.0f)); // movement lasts randomly between 1s to 3s.
        // Set anim to walk.
        animator.SetBool("Idle", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // damg the player upon contact.
        Knight player = collision.GetComponent<Knight>();
        if (player != null)
        {
            if (SpikeAttack == true) // spikes.
            {
                player.changeHealth(-20);
            }
            else if (attack == true) // normal attack. 
            {
                player.changeHealth(-15);
            }
            else // touch damage.
            {
                player.changeHealth(-10);
            }
        }
    }

    void FireBallAttack()
    {
        Vector2 initialposition = rigidbody2D.position;
        Vector2[] LookDirection = new Vector2[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down, new Vector2(1.0f, -1.0f), new Vector2(1.0f, 1.0f), new Vector2(-1.0f, 1.0f), new Vector2(-1.0f, -1.0f) };
        Vector2 spawnposition = initialposition + Vector2.up;
        int numFireballsPhase1 = 4; 
        if (Mathf.Sign(moveSpeedX) < 0) 
        {
            spawnposition += Vector2.right * 0.5f; // shift right if facing left.
        }
        else
        {
            spawnposition += Vector2.left * 0.5f; // shift left if facing right.
        }

        if (Phase2 == true)
        {
            spawnposition += Vector2.down * 0.5f;
;        }

        GameObject[] fireballspawn = new GameObject[8];
        Fireball[] fireballattack = new Fireball[8];
        float force = 300.0f;
        // same angles.
        for (int i = 0; i < 2; i++)
        {
            fireballspawn[i] = Instantiate(fireball, spawnposition, Quaternion.identity);

        }
        // different angles.
        fireballspawn[2] = Instantiate(fireball, spawnposition , Quaternion.Euler(0,0,-90));
        fireballspawn[3] = Instantiate(fireball, spawnposition , Quaternion.Euler(0, 0, 90));

        for (int i = 0; i < numFireballsPhase1; i++)
        {
            fireballattack[i] = fireballspawn[i].GetComponent<Fireball>();
            fireballattack[i].Launch(LookDirection[i], force, initialposition, this.gameObject);
        }

        if (Phase2 == true) // shoots more during phase 2
        {
            // diagonal fireballs during phase2.
            fireballspawn[4] = Instantiate(fireball, spawnposition, Quaternion.Euler(0, 0, -45));
            fireballspawn[5] = Instantiate(fireball, spawnposition, Quaternion.Euler(0, 0, 45));
            fireballspawn[6] = Instantiate(fireball, spawnposition, Quaternion.Euler(0, 0, -45));
            fireballspawn[7] = Instantiate(fireball, spawnposition, Quaternion.Euler(0, 0, 45));

            for (int i = numFireballsPhase1; i < fireballspawn.Length; i++)
            {
                fireballattack[i] = fireballspawn[i].GetComponent<Fireball>();
                fireballattack[i].Launch(LookDirection[i], force, initialposition, this.gameObject);
            }
        }
    }

    void Attack()
    {
        if (Phase2 == true)
        {
            attackTimer = attackDelayP2;
        }
        else
        {
            attackTimer = attackDelay;
        }
        
        attack = true;
        animator.SetBool("Attack", true);
        attackAnimTimer = attackanim;
    }

    void Spikes()
    {
        attackTimer = SpikeAttackDelay;
        attack = true;
        animator.SetBool("Spikes", true);
        attackAnimTimer = SpikeAnim;
        attackCount = 0; // reset attack counter.
        SpikeAttack = true;
    }

    void BeginPhase2()
    {
        Collider2D collider = GetComponent<BoxCollider2D>();
        collider.enabled = false; // disable the collider. 
        if (Death1 == false)
        {
            animator.SetBool("Idle", false);
            DeathAnimTimer = DeathanimTimer1; // set to play first death animation, then trigger evolution.
            animator.SetBool("Death", true);
            Death1 = true; // died. and is now enterin phase 2.
        }

        if (DeathAnimTimer > 0 && Evolving == true) // time is set for death anim.
        {
            DeathAnimTimer -= Time.fixedDeltaTime; // countdown death animation,  then trigger phase 2 anim. 
            if (DeathAnimTimer < 0)
            {
                animator.SetBool("Death", false); // reset death parameter.
                animator.SetTrigger("Evolve"); // evolve. 
                EvolveTime = EvolveAnim;
            }
        }

        if (EvolveTime > 0) // evolve animation timer is set.
        {
            EvolveTime -= Time.fixedDeltaTime;
            if (EvolveTime < 0)
            {
                Phase2 = true; // phase2 begins.
                Evolving = false; // done evolving.
                Reset();  // reset all attacks.
            }
        }
    }

    // initial values. used for evolve.
    private void Reset()
    {
        TimerFBCD = FireBallCD;
        Timer = MoveTimer; // initial movement timer pattern.
        FireballReady = false; // fireball not ready upon awake.
        attack = false; // not attacking upon awake.
        attackTimer = -1;
        attackCount = 0;
        foreach (AnimatorControllerParameter parameter in animator.parameters) // for each parameter, set it to false.
        {
            if (parameter.type.Equals(typeof(bool)))
            {
                animator.SetBool(parameter.name, false);
            }
        } 
        animator.SetBool("Idle", true); // then set idle to true.
    }

    // when knight dies, perform full reset.
    void FullReset()
    {
        Reset();
        Death1 = false; // phase 1.
        Phase2 = false;
        Evolving = false; // currently in evolving phase or not.
        SpikeAttack = false;
        BattleStart = false; // not wake.
        currentHealth = BossMaxHealth; // reset health.
        HealthSystem.enabled = false;
        played = false; // reset boss bgm.
        ResetAudio();
    }

    IEnumerator ResetTimer()
    {
        yield return new WaitForSeconds(3.0f);
        BattleStart = false;
        FullReset();
        rigidbody2D.transform.position = StartPos;
        animator.SetTrigger("Reset");
    }

    void ResetAudio()
    {
        if (audioreset == false)
        {
            AudioPlayer.clip = NormalBGM; // reset audio clip.
            AudioPlayer.Play();
            audioreset = true;
        }
    }

}

