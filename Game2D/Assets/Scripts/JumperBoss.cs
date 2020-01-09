using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumperBoss : Enemy
{
    //Make a move randomizer, so that it picks between 1-3, where each is a different set of moves for a chain of patterns
    //Give the rocks like 1 second of immunity, so they dont break on the walls?
    //Make it so that, if the position of the rocks are outside of the box, don't spawn them, this might change cuz moving boss rooms 
    //also random numbers 

    //Movement of Jumper and Phase 1 timers
    Renderer renderer;
    bool awake;
    public static bool isGrounded;
    bool inRange;
    int numJumps;
    float timeIdle;
    float timeCoolDown = 2.0f;
    float radius = 13.0f;
    float restTimer;
    float rest = 5.0f;
    Vector3 originalPos;

    //Abilities (Rock Fall)
    public GameObject rockPrefab;
    float stationary = 3.0f;
    float rockCoolDown = 6.0f;
    float rockTimer;
    float shakeAmt = 200.0f;

    //Camoflauge 
    float camoTimer;
    float camoCoolDown = 8.0f;
    float camoDuration = 5.0f;
    float camoDurationTimer;
    bool isCamo;

    //Health UI
    public Canvas bossHealth;

    //Gate
    public GameObject gateTiles;
    bool firstAwake;

    //Audio
    public AudioClip BossBGM;
    public AudioSource AudioPlayer;
    public AudioClip NormalBGM;
    bool played;
    bool audioreset;

    // Victory screen.
    public Canvas VictoryScreen;
    public AudioClip VictoryBGM;
    public Canvas PlayerUI;
    bool victoryBGMset;

    void Start()
    {
        base.Start();
        renderer = GetComponent<Renderer>();
        played = false;
        audioreset = false;
        awake = false;
        inRange = false;
        isGrounded = true;
        timeIdle = 0;
        numJumps = 0;
        restTimer = 1.0f;
        rockTimer = rockCoolDown;
        camoTimer = 4.0f;
        isCamo = false;
        firstAwake = false;
        bossHealth.enabled = false;
        originalPos = transform.position;
        victoryBGMset = false;

    }

    // Update is called once per frame  
    void FixedUpdate()  
    {
        VictoryScreen.enabled = false; // hide victory screen.

        if(Knight.Player.Dead == true && currentHealth > 0)
        {
            Reset();
            ResetAudio();
        }
        if(currentHealth > 0 && Knight.Player.Dead == false)
        {
            inRange = Physics2D.OverlapCircle(transform.position, radius, LayerMask.GetMask("Knight"));
            if (inRange == true && firstAwake == false)
            {
                Knight.Player.respawnpoint = new Vector3(278f, -12f, 0f); 
                awake = true;
                bossHealth.enabled = true;
                gateTiles.SetActive(true);
                firstAwake = true;
                if (played == false)
                {
                    AudioPlayer.clip = BossBGM;
                    AudioPlayer.Play();
                    played = true;
                    audioreset = false; // disable audio reset. as boss bgm starts.
                }
            }
            //Find a direction for which the monster will jump for the x axis
            //For some reason, the rockTimer decides to stop counting down at times, usually when jumping?
            if (restTimer < 0 && awake == true)
            {
                if (isGrounded == true)
                {
                    if (rockTimer < 0)
                    {
                        StartCoroutine("ColorChange");
                        RockFallAttack();
                        restTimer = stationary;
                        rockTimer = rockCoolDown;
                    }
                    else
                    {
                        float directionAggro = Mathf.Sign(Knight.Player.transform.position.x - transform.position.x);
                        timeIdle -= Time.deltaTime;
                        if (timeIdle < 0)
                        {
                            animator.SetBool("isJump", true);
                            rigidbody2D.AddForce(new Vector2(250.0f * directionAggro, 500.0f)); //Make this so it actually jumps onto you
                            numJumps++;
                            timeIdle = timeCoolDown;
                        }
                    }

                    if (camoTimer < 0)
                    {
                        StartCoroutine("Fade");
                        camoDurationTimer = camoDuration;
                        isCamo = true;
                        camoTimer = camoCoolDown;
                    }
                    if (isCamo == true)
                    {
                        camoDurationTimer -= Time.deltaTime;
                    }
                    else if (isCamo == false)
                    {
                        camoTimer -= Time.deltaTime;
                    }
                    if (camoDurationTimer < 0 && isCamo == true)
                    {
                        StartCoroutine("UnFade");
                        isCamo = false;
                    }
                }
                if (numJumps == 4)
                {
                    restTimer = rest;
                    numJumps = 0;
                }
            }
            else
            {
                if (awake == true)
                {
                    restTimer -= Time.deltaTime;
                    rockTimer -= Time.deltaTime;
                }

            }
        }
        else if(currentHealth <= 0 && isGrounded == true)
        {
            if (DeathAnimSet == false)
            {
                Debug.Log("Death Anim Played");
                DeathAnimSet = true;
                DeathAnimTimer = 1.2f;
            }
            DeathAnimation();
           // ResetAudio();
            VictoryScreen.enabled = true; // enable victory screen .
            PlayerUI.sortingOrder = -10; // push behind victory screen.
            if (victoryBGMset == false) // ensure it only executes once.
            {
                AudioPlayer.clip = VictoryBGM; // change BGM to victory bgm.
                AudioPlayer.Play();
                victoryBGMset = true;
            }

        }


    }
    //Might want to use a shape for this stuff instead, see how it goes 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == 11)
        {
            animator.SetBool("isJump", false);
            isGrounded = true;
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Knight player = collision.GetComponent<Knight>();
        if (player != null)
        {
            player.changeHealth(-15);
        }
    }

    private void RockFallAttack()
    {
        Vector2 subtractVector = new Vector2(-20.0f, 0);
        Vector2 addVector =  new Vector2(8.0f, 0);
        Vector2 addVectorY = new Vector2(0, 10.0f);
        GameObject[] rockspawn = new GameObject[5];
        Vector2 initial = rigidbody2D.position + subtractVector;
        initial = initial + addVectorY;
        initial = initial + addVector;
        for(int i = 0; i  < 4; i++)
        {
            rockspawn[i] = Instantiate(rockPrefab, initial, Quaternion.identity);
            initial += addVector;
 
        }
    }
    private void Reset()
    {
        rigidbody2D.position = originalPos;
        awake = false;
        inRange = false;
        isGrounded = true;
        timeIdle = 0;
        numJumps = 0;
        restTimer = 1.0f;
        rockTimer = rockCoolDown;
        camoTimer = 4.0f;
        isCamo = false;
        firstAwake = false;
        currentHealth = maxHealth;
        healthFill.value = currentHealth / (float)maxHealth;
        bossHealth.enabled = false;
        gateTiles.SetActive(false);
       // Knight.Player.respawnpoint = new Vector3(278f, -12f, 0f);
    }
    void ResetAudio()
    {
        if (audioreset == false)
        {
            AudioPlayer.clip = NormalBGM; // reset audio clip.
            AudioPlayer.Play();
            audioreset = true;
            played = false; 
        }
    }
    IEnumerator Fade()
    {
        for (float f = 1.0f; f >= 0; f -= 0.1f)
        {
            Color c = renderer.material.color;
            c.a = f;
            renderer.material.color = c;
            yield return new WaitForSeconds(.1f);
        }
    }

    IEnumerator UnFade()
    {
        for(float f = 0; f <= 1.0f; f+= 0.1f)
        {
            Color c = renderer.material.color;
            c.a = f;
            renderer.material.color = c;
            yield return new WaitForSeconds(.1f);
        }
    }
    IEnumerator ColorChange()
    {
        Color defaultColor = renderer.material.color;
        Color newColor = Color.gray;
        for (int f = 0; f <= 20; f++)
        {
            if(f % 2 == 0)
            {
                renderer.material.color = defaultColor;
                Debug.Log(newColor);
            }
            else
            {
   
                renderer.material.color = newColor;
            }
            yield return new WaitForSeconds(.05f);
        }
    }
    IEnumerator ColorChangeLonger()
    {
        Color defaultColor = renderer.material.color;
        Color newColor = Color.gray;
        for (int f = 0; f <= 30; f++)
        {
            if (f % 2 == 0)
            {
                renderer.material.color = defaultColor;
                Debug.Log(newColor);
            }
            else
            {

                renderer.material.color = newColor;
            }
            yield return new WaitForSeconds(10.0f);
        }
    }

}   

