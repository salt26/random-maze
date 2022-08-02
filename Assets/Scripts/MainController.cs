using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    public static MainController mc;

    public int MazeColumns
    {
        get;
        private set;
    }

    public int MazeRows
    {
        get;
        private set;
    }

    public float InitialTime
    {
        get;
        private set;
    }

    public int SceneIndex
    {
        get;
        private set;
    }

    public enum Theme { None, Sunset, Desert, Illusion };
    public enum Level { None, Easy, Normal, Hard };

    public bool isBGMOff = false;
    public bool isSoundOff = false;
    public GameObject bgm;
    public Button bgmButton;
    public Button startButton;
    public Button themeSunsetButton;
    public Button themeDesertButton;
    public Button themeIllusionButton;
    public Button levelEasyButton;
    public Button levelNormalButton;
    public Button levelHardButton;
    public Text sizeText;
    public Image backgroundImage;
    public Sprite sunsetSprite;
    public Sprite desertSprite;
    public Sprite illusionSprite;
    public Theme theme = Theme.None;
    public Level level = Level.None;

    private bool hasGameStart = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (mc != null)
        {
            isBGMOff = mc.isBGMOff;
            isSoundOff = mc.isSoundOff;
            Destroy(mc.gameObject);
        }
        mc = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        theme = Theme.None;
        level = Level.None;
        UpdateLevel();
        UpdateStart();

        if (isBGMOff)
        {
            bgm.GetComponent<AudioSource>().volume = 0f;
            bgm.GetComponent<Text>().text = "<color=#0098E6>◆</color> BGM (Off)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.9f, 0.9f, 0.9f),
                disabledColor = new Color(0.9f, 0.9f, 0.9f),
                highlightedColor = new Color(0f, 0.5929411f, 0.9f),
                selectedColor = new Color(0f, 0.5929411f, 0.9f),
                pressedColor = new Color(0f, 0.6588235f, 1f)
            };

        }
        else
        {
            bgm.GetComponent<AudioSource>().volume = 1f;
            bgm.GetComponent<Text>().text = "<color=#00E64E>◆</color> BGM (On)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.9f, 0.9f, 0.9f),
                disabledColor = new Color(0.9f, 0.9f, 0.9f),
                highlightedColor = new Color(0f, 0.9f, 0.3070588f),
                selectedColor = new Color(0f, 0.9f, 0.3070588f),
                pressedColor = new Color(0f, 1f, 0.3411765f)
            };
        }
    }

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1))
    private void Update()
    {
        if (SceneManager.GetActiveScene().name.Equals("Main") && !Cursor.visible)
        {
            Cursor.visible = true;
        }
    }
#endif

    public void SunsetTheme()
    {
        if (hasGameStart) return;
        theme = Theme.Sunset;
        SceneIndex = (int)theme;    // 1
        UpdateStart();
        UpdateTheme();
    }

    public void DesertTheme()
    {
        if (hasGameStart) return;
        theme = Theme.Desert;
        SceneIndex = (int)theme;    // 2
        UpdateStart();
        UpdateTheme();
    }

    public void IllusionTheme()
    {
        if (hasGameStart) return;
        theme = Theme.Illusion;
        SceneIndex = (int)theme;    // 3
        UpdateStart();
        UpdateTheme();
    }

    public void EasyLevel()
    {
        if (hasGameStart) return;
        level = Level.Easy;
        MazeColumns = 12;
        MazeRows = 12;
        InitialTime = 305;
        UpdateLevel();
        UpdateStart();
    }

    public void NormalLevel()
    {
        if (hasGameStart) return;
        level = Level.Normal;
        MazeColumns = 18;
        MazeRows = 18;
        InitialTime = 350;
        UpdateLevel();
        UpdateStart();
    }

    public void HardLevel()
    {
        if (hasGameStart) return;
        level = Level.Hard;
        MazeColumns = 24;
        MazeRows = 24;
        InitialTime = 395;
        UpdateLevel();
        UpdateStart();
    }

    private void UpdateStart()
    {
        if (hasGameStart || theme == Theme.None || level == Level.None)
            startButton.interactable = false;
        else
            startButton.interactable = true;
    }

    private void UpdateTheme()
    {
        themeSunsetButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        themeDesertButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        themeIllusionButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        backgroundImage.color = Color.white;

        switch (theme)
        {
            case Theme.Sunset:
                themeSunsetButton.GetComponentInChildren<Text>().color = themeSunsetButton.colors.highlightedColor;
                backgroundImage.sprite = sunsetSprite;
                break;
            case Theme.Desert:
                themeDesertButton.GetComponentInChildren<Text>().color = themeDesertButton.colors.highlightedColor;
                backgroundImage.sprite = desertSprite;
                break;
            case Theme.Illusion:
                themeIllusionButton.GetComponentInChildren<Text>().color = themeIllusionButton.colors.highlightedColor;
                backgroundImage.color = new Color(0.9f, 0.9f, 0.9f);
                backgroundImage.sprite = illusionSprite;
                break;
        }
    }

    private void UpdateLevel()
    {
        levelEasyButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        levelNormalButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        levelHardButton.GetComponentInChildren<Text>().color = new Color(0.9f, 0.9f, 0.9f);
        sizeText.text = MazeColumns + " X " + MazeRows;

        switch (level)
        {
            case Level.None:
                sizeText.color = Color.white;
                sizeText.text = "";
                break;
            case Level.Easy:
                levelEasyButton.GetComponentInChildren<Text>().color = levelEasyButton.colors.highlightedColor;
                sizeText.color = levelEasyButton.colors.pressedColor;
                break;
            case Level.Normal:
                levelNormalButton.GetComponentInChildren<Text>().color = levelNormalButton.colors.highlightedColor;
                sizeText.color = levelNormalButton.colors.pressedColor;
                break;
            case Level.Hard:
                levelHardButton.GetComponentInChildren<Text>().color = levelHardButton.colors.highlightedColor;
                sizeText.color = levelHardButton.colors.pressedColor;
                break;
        }
    }

    public void StartGame()
    {
        if (hasGameStart || theme == Theme.None || level == Level.None) return;
        hasGameStart = true;
        SceneManager.LoadScene(SceneIndex);
        UpdateStart();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BGMButton()
    {
        if (!isBGMOff)
        {
            isBGMOff = true;
            bgm.GetComponent<AudioSource>().volume = 0f;
            bgm.GetComponent<Text>().text = "<color=#0098E6>◆</color> BGM (Off)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.9f, 0.9f, 0.9f),
                disabledColor = new Color(0.9f, 0.9f, 0.9f),
                highlightedColor = new Color(0f, 0.5929411f, 0.9f),
                selectedColor = new Color(0f, 0.5929411f, 0.9f),
                pressedColor = new Color(0f, 0.6588235f, 1f)
            };
        }
        else
        {
            isBGMOff = false;
            bgm.GetComponent<AudioSource>().volume = 1f;
            bgm.GetComponent<Text>().text = "<color=#00E64E>◆</color> BGM (On)";
            bgmButton.colors = new ColorBlock
            {
                colorMultiplier = 1f,
                fadeDuration = 0.1f,
                normalColor = new Color(0.9f, 0.9f, 0.9f),
                disabledColor = new Color(0.9f, 0.9f, 0.9f),
                highlightedColor = new Color(0f, 0.9f, 0.3070588f),
                selectedColor = new Color(0f, 0.9f, 0.3070588f),
                pressedColor = new Color(0f, 1f, 0.3411765f)
            };
        }
    }
}
