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
            if (touch != null)
                return touch.isActive;
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

    ButtonInfo touch = new ButtonInfo();

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
        touch.backgroundCircle = cntrlTmpObj.AddComponent<Image>();
        touch.backgroundCircle.sprite = circleSprite;
        touch.backgroundCircle.rectTransform.anchorMin = new Vector2(0, 0);
        touch.backgroundCircle.rectTransform.anchorMax = new Vector2(0, 0);
        touch.backgroundCircle.rectTransform.sizeDelta = new Vector2(circleSize, circleSize);
        touch.backgroundCircle.rectTransform.pivot = new Vector2(1, 0);
        touch.backgroundCircle.rectTransform.position = new Vector3(Screen.width - marginRight, marginBottom, 0);
        touch.backgroundCircle.color = new Color(1f, 1f, 1f, 0.5f);

        // Navigation button
        cntrlTmpObj = new GameObject("Movement Button");
        cntrlTmpObj.transform.position = Vector3.zero;
        cntrlTmpObj.transform.parent = tmpObj.transform;
        touch.mainButton = cntrlTmpObj.AddComponent<Image>();
        touch.mainButton.sprite = buttonSprite;
        touch.mainButton.rectTransform.anchorMin = new Vector2(0, 0);
        touch.mainButton.rectTransform.anchorMax = new Vector2(0, 0);
        touch.mainButton.rectTransform.sizeDelta = new Vector2(buttonSize, buttonSize);
        touch.mainButton.rectTransform.pivot = new Vector2(1, 0);
        touch.mainButton.rectTransform.position = new Vector3(Screen.width - marginRight - (circleSize - buttonSize) / 2, marginBottom + (circleSize - buttonSize) / 2, 0);
        touch.mainButton.color = new Color(1f, 1f, 1f, 0.8f);

        // Save the default location of the joystick button to be used later for input detection
        touch.defaultArea = new Rect(touch.mainButton.rectTransform.position.x,
            touch.mainButton.rectTransform.position.y,
            touch.mainButton.rectTransform.sizeDelta.x,
            touch.mainButton.rectTransform.sizeDelta.y);

        touch.detectionArea = new Rect(Screen.width * 0.5f,
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
                if(touch.isActive && touch.touchID == touch.fingerId)
                {
                    touch.currentTouchPos = touch.position;
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

        touch.currentTouchPos = Input.mousePosition;
#endif

        // Moving
        if (touch.isActive)
        {
            touch.mainButton.rectTransform.position = new Vector3(
                Mathf.Clamp(touch.currentTouchPos.x + buttonSize / 2,
                    touch.detectionArea.x + buttonSize / 2, touch.detectionArea.x + touch.detectionArea.width),
                Mathf.Clamp(touch.currentTouchPos.y - buttonSize / 2,
                    touch.detectionArea.y, touch.detectionArea.y + touch.detectionArea.height - buttonSize / 2),
                0
            );
            touch.backgroundCircle.rectTransform.position = new Vector3(
                touch.mainButton.rectTransform.position.x + (circleSize - buttonSize) / 2,
                touch.mainButton.rectTransform.position.y - (circleSize - buttonSize) / 2, 0);
        }
        else
        {
            touch.mainButton.rectTransform.position = new Vector3(touch.defaultArea.x, touch.defaultArea.y);
            touch.backgroundCircle.rectTransform.position = new Vector3(Screen.width - marginRight, marginBottom, 0);
        }
    }

    // Here we check if the clicked/tapped position is inside the touch detection area
    void MobileButtonsCheck(Vector2 touchPos, int touchID)
    {
        // Move controller
        if (touch.detectionArea.Contains(new Vector2(touchPos.x, Screen.height - touchPos.y)) && !touch.isActive)
        {
            touch.isActive = true;
            //touch.touchOffset = new Vector2(touchPos.x - touch.defaultArea.x, Screen.height - touchPos.y - touch.defaultArea.y);
            touch.touchOffset = new Vector2(touchPos.x, Screen.height - touchPos.y);
            touch.currentTouchPos = new Vector2(touchPos.x, Screen.height - touchPos.y);
            touch.touchID = touchID;
        }
    }

    // Here we release the previously active button if we release the mouse button/finger from the screen
    void MobileButtonStop(int touchID)
    {
        if (touch.isActive && touch.touchID == touchID)
        {
            touch.isActive = false;
            touch.touchOffset = Vector2.zero;
            touch.touchID = -1;
        }
    }

    public void SetCircleColor(Color color)
    {
        touch.backgroundCircle.color = new Color(color.r, color.g, color.b, 0.5f);
    }
}
