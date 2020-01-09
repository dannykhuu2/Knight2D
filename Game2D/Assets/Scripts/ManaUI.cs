using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaUI : MonoBehaviour
{
    public static ManaUI ManaBar { get; private set; }

    public Image ManaMask;
    float StartSize;

    // Start is called before the first frame update
    private void Awake()
    {
        ManaBar = this;
    }

    void Start()
    {
        StartSize = ManaMask.rectTransform.rect.width; // starting width.
    }

    public void SetValue(float value)
    {
        ManaMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, StartSize * value); // % change.
    }
   
}
