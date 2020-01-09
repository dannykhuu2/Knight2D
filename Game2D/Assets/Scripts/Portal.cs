using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Portal : MonoBehaviour
{
    Collider2D PortalCollider; // Collider to allow portal to act as an object.
    TilemapRenderer PortalRenderer;
    bool Faded;

    // Gem
    public GameObject Gem;
    SpriteRenderer GemRenderer;
    Collider2D GemCollider;
    bool DisplayGem;

    // Timers.
    float Timer;
    float TeleportTimer = 2.0f;

    // knight collider.
    public Transform KnightCollider;
    float radius = 1.0f;

    // Audio System.
    public AudioSource AudioPlayer;
    public AudioClip MainBGM;

    // Teleport Location
    Vector3 TeleportPos = new Vector3(115f, 9.0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        PortalCollider = GetComponent<TilemapCollider2D>();
        PortalRenderer = GetComponent<TilemapRenderer>();

        GemRenderer = Gem.GetComponent<SpriteRenderer>();
        GemCollider = Gem.GetComponent<BoxCollider2D>();

        Timer = TeleportTimer;
        Faded = false;
        DisplayGem = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (PortalRenderer.enabled) // if the portal is displaying then it is active and boss is dfted.
        {
            if (Faded == false)
            {
                StartCoroutine("FadeIn");
                Faded = true;
            }

            PortalCollider.enabled = true; // enable the collider.

            if (Gem != null) // if gem has not been claimed.
            {
                GemRenderer.enabled = true; // display the gem.
                if (DisplayGem == false)
                {
                    StartCoroutine("GemFadeIn");
                    DisplayGem = true;
                }
            }

            if (Physics2D.OverlapCircle(KnightCollider.position, radius, LayerMask.GetMask("Knight"))) // if touching knight, then countdown timer.
            {
                Timer -= Time.deltaTime;
                if (Timer < 0) // when timer complete tp knight.
                {
                    Knight player = Physics2D.OverlapCircle(KnightCollider.position, radius, LayerMask.GetMask("Knight")).gameObject.GetComponent<Knight>();
                    player.transform.position = TeleportPos;
                    AudioPlayer.clip = MainBGM; // change BGM once map is left.
                    AudioPlayer.Play();
                }
            }
            else
            {
                Timer = TeleportTimer; // reset timer if not touching.
            }
        }
    }

    IEnumerator FadeIn()
    {
        Color color = new Color(1f, 1f, 1f, 0f);
        PortalRenderer.material.color = color;
        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            color = PortalRenderer.material.color;
            color.a = i;
            PortalRenderer.material.color = color;
            yield return new WaitForSeconds(.2f);
        }
        PortalRenderer.material.color = new Color(1f, 1f, 1f, 1f); // fully visible.
    }

    IEnumerator GemFadeIn()
    {
        Color color = new Color(1f, 1f, 1f, 0f);
        GemRenderer.material.color = color;
        for (float i = 0f; i <= 1f; i += 0.1f)
        {
            color = GemRenderer.material.color;
            color.a = i;
            GemRenderer.material.color = color;
            yield return new WaitForSeconds(.2f);
        }
        GemRenderer.material.color = new Color(1f, 1f, 1f, 1f); // fully visible.
        GemCollider.enabled = true; // enable the collider.
    }
}
