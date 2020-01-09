using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Sign : MonoBehaviour
{
    //Just add a "..." to the end of the line if its not the last line 
    public GameObject scroll;
    public GameObject messageBox;
    public TMP_Text dText;
    public TMP_Text dTextNextBox;
    public string[] dialogLines;
    public int currentLine;

    string finalString;
    float distance;
    float maxDistance = 4.0f;
    bool active;

    // Start is called before the first frame update
    void Start()
    {
        active = false;
        currentLine = 0;
        finalString = "";
        messageBox.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if(gameObject.name.Equals("sign"))
        {
            Debug.Log("current line: " + currentLine);
        }

        if (active == true)
        {
            float distance = Mathf.Abs(Knight.Player.transform.position.x - transform.position.x);
            if (distance > maxDistance)
            {
                DisplayDialog();
            }
        }

    }
    public void DisplayDialog()
    {
        if(gameObject.name.Equals("sign (1)"))
        {
            Debug.Log(currentLine);
        }
        if(currentLine == 0)
        {
            active = true;
            messageBox.SetActive(true);
            scroll.SetActive(false);
            dTextNextBox.text = "Next ... (S)";
        }
        else if(currentLine >= dialogLines.Length)
        {
            messageBox.SetActive(false);
            active = false;
            currentLine = 0;
        }
        if (currentLine == dialogLines.Length -1)
        {
            dTextNextBox.text = "Close ... (S)";
        }
        finalString = textFormat();
        dText.text = finalString;
        if(active != false)
        {
            currentLine++;
        }

    }
    private string textFormat()
    {
        string copy = "";
        string[] words = dialogLines[currentLine].Split('^');
        foreach(var word in words)
        {
            copy += word + "\n";
        }
        return copy;
    }
}
