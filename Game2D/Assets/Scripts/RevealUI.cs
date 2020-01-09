using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RevealUI : MonoBehaviour
{
    // Start is called before the first frame update
    public static RevealUI RevealBar { get; private set; }

    public Image RevealCD;
    float StartSize;

    // Start is called before the first frame update
    void Start()
    {
        StartSize = RevealCD.rectTransform.rect.width; // starting size.
    }

    private void Awake()
    {
        RevealBar= this;
    }

    public void SetValue(float value)
    {
        RevealCD.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value * StartSize);
    }
}
