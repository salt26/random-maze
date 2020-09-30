using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceSlider : MonoBehaviour
{
    public RectTransform fillRectX;
    public RectTransform fillRectY;
    public RectTransform tickY;

    private float xValue;
    private float yValue;

    // Start is called before the first frame update
    void Start()
    {
        xValue = 0f;
        yValue = 0f;
    }

    public void SetValues(Vector2 v)
    {
        xValue = Mathf.Clamp(v.x, 0f, 1f);
        yValue = Mathf.Clamp(v.y, 0f, 1f);
        UpdateSlider();
    }

    void UpdateSlider()
    {
        fillRectX.anchorMax = new Vector2(xValue / 2f, 1f);
        fillRectY.anchorMax = new Vector2((xValue + yValue) / 2f, 1f);
        tickY.anchorMin = new Vector2(0.49f + xValue / 2f, 0f);
        tickY.anchorMax = new Vector2(0.51f + xValue / 2f, 1f);
    }
}
