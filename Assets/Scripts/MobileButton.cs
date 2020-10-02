using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileButton : MonoBehaviour
{
    // Mobile controller graphics
    public Sprite circleSprite;
    public Sprite buttonSprite;

    public bool HasPressed {
        get {
            if (buttonTouch != null)
                return buttonTouch.isActive;
            else
                return false;
        }
    }

    // Button components size
    int circleSize = (int)(Screen.height * 0.2f * 1.2f);
    int buttonSize = (int)(Screen.height * 0.2f);

    // How far the joystick should be placed from the side of the screen
    int marginRight = (int)(Screen.height * 0.22f);

    // How far the joystick should be placed from the bottom of the screen
    int marginBottom = (int)(Screen.height * 0.22f);

    Canvas mainCanvas;

    // Mobile movement
    [System.Serializable]
    public class ButtonInfo
    {
        public Image backgroundCircle;
        public Image mainButton;
        public Rect defaultArea;
        public Rect detectionArea;
        public Vector2 touchOffset;
        public Vector2 currentTouchPos;
        public int touchID;
        public bool isActive = false;
    }

    ButtonInfo buttonTouch = new ButtonInfo();

    public static MobileButton instance;

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)
        {
            // There is another instance already present, remove this one
            Destroy(gameObject);
            return;
        }
        // Assign this instance to a static variable so you can access the movement direction directly at MobileDash.instance.isDashing
        instance = this;

        // This function will initialize canvas element along with the joystick button
        GameObject tmpObj = new GameObject("Canvas");
        tmpObj.transform.position = Vector3.zero;
        mainCanvas = tmpObj.AddComponent<Canvas>();
        mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        mainCanvas.pixelPerfect = true;

        // Add Canvas Scaler component
        CanvasScaler canvasScaled = tmpObj.AddComponent<CanvasScaler>();
        canvasScaled.scaleFactor = 1;
        canvasScaled.referencePixelsPerUnit = 100;

        // Add Graphic Raycaster element
        tmpObj.AddComponent<GraphicRaycaster>();

        // Setup navigation background
        GameObject cntrlTmpObj = new GameObject("Dash Circle");
        cntrlTmpObj.transform.position = Vector3.zero;
        cntrlTmpObj.transform.parent = tmpObj.transform;
        buttonTouch.backgroundCircle = cntrlTmpObj.AddComponent<Image>();
        buttonTouch.backgroundCircle.sprite = circleSprite;
        buttonTouch.backgroundCircle.rectTransform.anchorMin = new Vector2(0, 0);
        buttonTouch.backgroundCircle.rectTransform.anchorMax = new Vector2(0, 0);
        buttonTouch.backgroundCircle.rectTransform.sizeDelta = new Vector2(circleSize, circleSize);
        buttonTouch.backgroundCircle.rectTransform.pivot = new Vector2(1, 0);
        buttonTouch.backgroundCircle.rectTransform.position = new Vector3(Screen.width - marginRight, marginBottom, 0);
        buttonTouch.backgroundCircle.color = new Color(1f, 1f, 1f, 0.5f);

        // Navigation button
        cntrlTmpObj = new GameObject("Movement Button");
        cntrlTmpObj.transform.position = Vector3.zero;
        cntrlTmpObj.transform.parent = tmpObj.transform;
        buttonTouch.mainButton = cntrlTmpObj.AddComponent<Image>();
        buttonTouch.mainButton.sprite = buttonSprite;
        buttonTouch.mainButton.rectTransform.anchorMin = new Vector2(0, 0);
        buttonTouch.mainButton.rectTransform.anchorMax = new Vector2(0, 0);
        buttonTouch.mainButton.rectTransform.sizeDelta = new Vector2(buttonSize, buttonSize);
        buttonTouch.mainButton.rectTransform.pivot = new Vector2(1, 0);
        buttonTouch.mainButton.rectTransform.position = new Vector3(Screen.width - marginRight - (circleSize - buttonSize) / 2, marginBottom + (circleSize - buttonSize) / 2, 0);
        buttonTouch.mainButton.color = new Color(1f, 1f, 1f, 0.8f);

        // Save the default location of the joystick button to be used later for input detection
        buttonTouch.defaultArea = new Rect(buttonTouch.mainButton.rectTransform.position.x,
            buttonTouch.mainButton.rectTransform.position.y,
            buttonTouch.mainButton.rectTransform.sizeDelta.x,
            buttonTouch.mainButton.rectTransform.sizeDelta.y);

        buttonTouch.detectionArea = new Rect(Screen.width * 0.5f,
            0f,
            Screen.width * 0.5f,
            Screen.height * 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle button click
#if (UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR
        // Mobile touch input
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                MobileButtonsCheck(new Vector2(touch.position.x, Screen.height - touch.position.y), touch.fingerId);
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if(buttonTouch.isActive && buttonTouch.touchID == touch.fingerId)
                {
                    buttonTouch.currentTouchPos = touch.position;
                }
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                MobileButtonStop(touch.fingerId);
            }
        }
#else
        // Desktop mouse input for editor testing
        if (Input.GetMouseButtonDown(0))
        {
            MobileButtonsCheck(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y), -1);
        }

        if (Input.GetMouseButtonUp(0))
        {
            MobileButtonStop(-1);
        }

        buttonTouch.currentTouchPos = Input.mousePosition;
#endif

        // Moving
        if (buttonTouch.isActive)
        {
            buttonTouch.mainButton.rectTransform.position = new Vector3(
                Mathf.Clamp(buttonTouch.currentTouchPos.x + buttonSize / 2,
                    buttonTouch.detectionArea.x + buttonSize / 2, buttonTouch.detectionArea.x + buttonTouch.detectionArea.width),
                Mathf.Clamp(buttonTouch.currentTouchPos.y - buttonSize / 2,
                    buttonTouch.detectionArea.y, buttonTouch.detectionArea.y + buttonTouch.detectionArea.height - buttonSize / 2),
                0
            );
            buttonTouch.backgroundCircle.rectTransform.position = new Vector3(
                buttonTouch.mainButton.rectTransform.position.x + (circleSize - buttonSize) / 2,
                buttonTouch.mainButton.rectTransform.position.y - (circleSize - buttonSize) / 2, 0);
        }
        else
        {
            buttonTouch.mainButton.rectTransform.position = new Vector3(buttonTouch.defaultArea.x, buttonTouch.defaultArea.y);
            buttonTouch.backgroundCircle.rectTransform.position = new Vector3(Screen.width - marginRight, marginBottom, 0);
        }
    }

    // Here we check if the clicked/tapped position is inside the touch detection area
    void MobileButtonsCheck(Vector2 touchPos, int touchID)
    {
        // Move controller
        if (buttonTouch.detectionArea.Contains(new Vector2(touchPos.x, Screen.height - touchPos.y)) && !buttonTouch.isActive)
        {
            buttonTouch.isActive = true;
            //touch.touchOffset = new Vector2(touchPos.x - touch.defaultArea.x, Screen.height - touchPos.y - touch.defaultArea.y);
            buttonTouch.touchOffset = new Vector2(touchPos.x, Screen.height - touchPos.y);
            buttonTouch.currentTouchPos = new Vector2(touchPos.x, Screen.height - touchPos.y);
            buttonTouch.touchID = touchID;
        }
    }

    // Here we release the previously active button if we release the mouse button/finger from the screen
    void MobileButtonStop(int touchID)
    {
        if (buttonTouch.isActive && buttonTouch.touchID == touchID)
        {
            buttonTouch.isActive = false;
            buttonTouch.touchOffset = Vector2.zero;
            buttonTouch.touchID = -1;
        }
    }

    public void SetCircleColor(Color color)
    {
        buttonTouch.backgroundCircle.color = new Color(color.r, color.g, color.b, 0.5f);
    }
}
