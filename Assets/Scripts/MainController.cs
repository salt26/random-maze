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

    public bool isBGMOff = false;
    public bool isSoundOff = false;
    public GameObject bgm;
    public Button bgmButton;

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

    public void EasyStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 12;
        MazeRows = 12;
        InitialTime = 245;
        SceneManager.LoadScene(1);
    }

    public void NormalStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 18;
        MazeRows = 18;
        InitialTime = 275;
        SceneManager.LoadScene(1);
    }

    public void HardStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 24;
        MazeRows = 24;
        InitialTime = 305;
        SceneManager.LoadScene(1);
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
