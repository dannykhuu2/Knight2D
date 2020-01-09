using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Collectible : MonoBehaviour
{
    //For some reason this collected variable does not keep track, does not add 
    public static int collected;
    public Image GemIcon;
    public Tilemap gateTiles;
    static int yVar = -8;
    Knight controller;
    Vector3Int vectorGate = new Vector3Int(316, yVar, 0);

    private void Start()
    {
        if (GemIcon != null) // hide the gems.
        {
            Color color = new Color(1f, 1f, 1f, 0.25f);
            GemIcon.color = color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        controller  = other.GetComponent<Knight>();
        if(controller != null)
        {
            if(gameObject.layer == 13)
            {
                if (controller.health < Knight.maxHealth)
                {
                    controller.changeHealth(25);
                    Destroy(gameObject);
                }
            }
            else if(gameObject.layer == 14)
            {
                if(controller.mana < Knight.maxMana)
                {
                    controller.changeMana(10);
                    Destroy(gameObject);
                }
            }
            //No idea if this actually works, this is kinda wonky 
            else if(gameObject.layer == 15)
            {
                if (!gameObject.name.Equals("bossGem"))
                {
                    if (controller.durationTimer > 0)
                    {
                        torchDisplay();
                    }
                }
                else
                {
                    torchDisplay();
                }
            }
        }
    }

    void DisplayGem() 
    {
        Color color = new Color(1f, 1f, 1f, 1f);
        GemIcon.color = color;
    }
    void torchDisplay()
    {
        collected++;
        Debug.Log("Y Var : " + yVar);
        Debug.Log(collected);
        DisplayGem();
        if (yVar >= -12)
        {
            Vector3Int vectorGate = new Vector3Int(316, yVar, 0);
            gateTiles.SetTile(vectorGate, null);
            yVar--;
            Destroy(gameObject);
        }
    }
}
