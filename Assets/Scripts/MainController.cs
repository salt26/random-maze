using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private bool hasGameStart = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (mc != null)
        {
            Destroy(mc.gameObject);
        }
        mc = this;
        DontDestroyOnLoad(this);
    }

    public void EasyStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 12;
        MazeRows = 12;
        //InitialTime = 245;
        InitialTime = 305;
        SceneManager.LoadScene(1);
    }

    public void NormalStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 18;
        MazeRows = 18;
        //InitialTime = 275;
        InitialTime = 365;
        SceneManager.LoadScene(1);
    }

    public void HardStart()
    {
        if (hasGameStart) return;
        hasGameStart = true;
        MazeColumns = 24;
        MazeRows = 24;
        //InitialTime = 305;
        InitialTime = 425;
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
