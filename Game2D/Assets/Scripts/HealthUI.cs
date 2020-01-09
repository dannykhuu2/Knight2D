using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI HealthBar { get; private set; }

    public Image HealthMask;
    float StartSize;

    // Start is called before the first frame update
    void Awake()
    {
        HealthBar = this;
    }

    private void Start()
    {
        StartSize = HealthMask.rectTransform.rect.width; // original width of the mask.
    }

    public void SetValue(float value)
    {
        HealthMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, StartSize * value); // sets horizontal bar with startsize * value (a fraction between 0 - 1).
    }

}
