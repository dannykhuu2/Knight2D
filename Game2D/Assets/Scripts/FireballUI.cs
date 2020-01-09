using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireballUI : MonoBehaviour
{
    public static FireballUI FireballBar { get; private set; }

    public Image FireballCD;
    float StartSize;

    // Start is called before the first frame update
    void Start()
    {
        StartSize = FireballCD.rectTransform.rect.width; // starting size.
    }

    private void Awake()
    {
        FireballBar = this;
    }

    public void SetValue(float value)
    {
        FireballCD.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value * StartSize);
    }
}
