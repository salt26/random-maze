using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistanceSlider : MonoBehaviour
{
    public RectTransform fillRectX;
    public RectTransform fillRectY;

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
        fillRectX.anchorMax = new Vector2(Mathf.Clamp(xValue, 0f, 1f) / 2f, 1f);
        fillRectY.anchorMax = new Vector2(
            (Mathf.Clamp(xValue, 0f, 1f) + Mathf.Clamp(yValue, 0f, 1f)) / 2f, 1f);
    }
}
