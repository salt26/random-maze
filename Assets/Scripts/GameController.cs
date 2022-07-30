using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

public class GameController : MonoBehaviour
{
    public static GameController gc;

    public float edgeLength;
    public Vector3 edgeBasePosition;
    public Vector3 edgeBaseRotation;
    public bool useColorMatching;
    public GameObject[] corners;
    public GameObject[] edges;
    public GameObject colliderCorner;
    public GameObject colliderEdge;
    public GameObject colliderExit;
    public GameObject menu;
    public GameObject playerPrefab;
    public List<AudioClip> footsteps;
    public Button menuButton;
    public Text timeText;
    public Text menuButtonText;
    public Text soundButtonText;
    public DistanceSlider progressSlider;
    public AudioClip exitClip;
    public AudioClip timeoutClip;
    public Camera mainCamera;
    public CinemachineVirtualCamera virtualCamera;
    public GameObject touchInput;

    public float initialTime = 300f;
    public Vector3 initialPlayerPosition;

    public bool mazeFromFile = false;

    public int mazeColumns = 15;
    public int mazeRows = 15;
    public int mazeInnerColumns = 5;
    public int mazeInnerRows = 5;
    public float mazeDropProbability = 0.02f;   // When this is 0.0f, only one way exists

    private float time;
    private int[,] m_Maze;
    private bool hasExited = false;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
    private bool hasPressedShift = false;
#endif
    private bool isTimeout = false;
    private bool isMenuShowed = false;
    private Vector2 distanceMaxValue;
    private Vector2 distanceCurrentValue;

    private SwatMovement player;

    void Awake()
    {
        if (gc != null)
        {
            Destroy(gc.gameObject);
        }
        gc = this;

        GameObject p = Instantiate(playerPrefab, initialPlayerPosition, /*Quaternion.Euler(0f, 180f, 0f)*/ Quaternion.identity);
        player = p.GetComponent<SwatMovement>();
        player.mainCamera = mainCamera;
        virtualCamera.Follow = GameObject.FindGameObjectWithTag("Jaw").GetComponent<Transform>();
    }

    void Start()
    {
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
        Cursor.visible = false;
        touchInput.SetActive(false);
        virtualCamera.AddCinemachineComponent<CinemachinePOV>();
        virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_MaxSpeed = 250f;
        virtualCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_MaxSpeed = 250f;
#else
        touchInput.SetActive(true);
#endif
        MazeGenerator m_MazeGenerator = new MazeGenerator();
        bool fileExist = false;

        // simple setting
        m_MazeGenerator.setRatio(.75f);

        if (MainController.mc != null)
        {
            mazeColumns = MainController.mc.MazeColumns;
            mazeRows = MainController.mc.MazeRows;
            initialTime = MainController.mc.InitialTime;
        }

        mazeColumns = Mathf.Clamp(mazeColumns, 5, 26);
        mazeRows = Mathf.Clamp(mazeRows, 5, 26);
        mazeInnerColumns = Mathf.Clamp(mazeInnerColumns, 0, mazeColumns);
        mazeInnerRows = Mathf.Clamp(mazeInnerRows, 0, mazeRows);
        mazeDropProbability = Mathf.Clamp(mazeDropProbability, 0f, 1f);

        if (mazeFromFile)
        {
            // There is no exit collider.
            /*
            string path = "Assets/Resources/Map.txt";

            try
            {
                StreamReader reader = new StreamReader(path);
                string line;
                int cols = System.Convert.ToInt32(reader.ReadLine());
                int rows = System.Convert.ToInt32(reader.ReadLine());
                m_Maze = new int[2 * cols, 2 * rows];
                for (int i = 0; i < 2 * cols; i++)
                {
                    line = reader.ReadLine();
                    for (int j = 0; j < 2 * rows; j++)
                    {
                        m_Maze[i, j] = line[j] == '#' ? 1 : 0;
                    }
                }
                reader.Close();
                fileExist = true;
            }
            catch (FileNotFoundException)
            {

            }
            */
        }
        if (!fileExist)
        {
            m_Maze = m_MazeGenerator.FromDimensions(mazeColumns, mazeRows, mazeInnerColumns, mazeInnerRows, mazeDropProbability);
            // make two entrances
            m_Maze[0, 1] = 0;
            m_Maze[2 * mazeColumns, 2 * mazeRows - 1] = 0;

            //Debug.Log(m_MazeGenerator.ConvertToString(m_Maze));
        }

        if (useColorMatching)
        {
            int[,] mazeColor = new int[m_Maze.GetLength(0), m_Maze.GetLength(1)];
            for (int i = 0; i < m_Maze.GetLength(0); i += 2)
            {
                for (int j = 0; j < m_Maze.GetLength(1); j += 2)
                {
                    if (m_Maze[i, j] == 0)
                    {
                        mazeColor[i, j] = -1;
                        continue;
                    }
                    // i % 2 * 2 + j % 2 == 0
                    int r = Random.Range(0, corners.Length);
                    Instantiate(corners[r],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f * Random.Range(0, 4) + edgeBaseRotation.y, edgeBaseRotation.z));
                    mazeColor[i, j] = r;
                }
            }

            for (int i = 0; i < m_Maze.GetLength(0); i += 2)
            {
                for (int j = 1; j < m_Maze.GetLength(1); j += 2)
                {
                    if (m_Maze[i, j] == 0) continue;
                    // i % 2 * 2 + j % 2 == 1
                    int r = Random.Range(0, edges.Length);
                    if (j + 1 < m_Maze.GetLength(1) && mazeColor[i, j - 1] == mazeColor[i, j + 1])
                        r = mazeColor[i, j - 1];
                    Instantiate(edges[r],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f + 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                }
            }

            for (int i = 1; i < m_Maze.GetLength(0); i += 2)
            {
                for (int j = 0; j < m_Maze.GetLength(1); j += 2)
                {
                    if (m_Maze[i, j] == 0) continue;
                    // i % 2 * 2 + j % 2 == 2
                    int r = Random.Range(0, edges.Length);
                    if (i + 1 < m_Maze.GetLength(0) && mazeColor[i - 1, j] == mazeColor[i + 1, j])
                        r = mazeColor[i - 1, j];
                    Instantiate(edges[r],
                        new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                        Quaternion.Euler(edgeBaseRotation.x, 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                }
            }
        }
        else
        {
            for (int i = 0; i < m_Maze.GetLength(0); i++)
            {
                for (int j = 0; j < m_Maze.GetLength(1); j++)
                {
                    if (m_Maze[i, j] == 0) continue;
                    int type = i % 2 * 2 + j % 2;
                    switch (type)
                    {
                        case 0:
                            Instantiate(corners[Random.Range(0, corners.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f * Random.Range(0, 4) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        case 1:
                            Instantiate(edges[Random.Range(0, edges.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 90f + 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        case 2:
                            Instantiate(edges[Random.Range(0, edges.Length)],
                                new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position + edgeBasePosition,
                                Quaternion.Euler(edgeBaseRotation.x, 180f * Random.Range(0, 2) + edgeBaseRotation.y, edgeBaseRotation.z));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // make exit collider
        Instantiate(colliderExit,
            new Vector3(edgeLength * (2 * mazeRows - 1) / 2f, 0f, edgeLength * mazeColumns) + transform.position,
            Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));

        GameObject p = Instantiate(playerPrefab, 
            new Vector3(edgeLength * (mazeRows - 0.25f), 0f, edgeLength * (mazeColumns + 0.5f)),
            Quaternion.Euler(0, 216f, 0));
        p.GetComponent<SwatMovement>().enabled = false;
        p.GetComponent<Animator>().SetInteger("AnimationState", 4);
        p.GetComponent<Animator>().SetBool("IsSprinting", false);
        p.GetComponent<Animator>().SetBool("NeedTurnLeft", false);
        p.GetComponent<Animator>().SetBool("NeedTurnRight", false);

        for (int i = -1; i > -9; i--)
        {
            CreateColliders(i, 8);
        }

        for (int j = -1; j > -9; j--)
        {
            CreateColliders(8, j);
        }

        for (int i = m_Maze.GetLength(0) + 7; i > -9; i--)
        {
            CreateColliders(i, -8);
            CreateColliders(i, m_Maze.GetLength(1) + 7);
        }

        for (int j = m_Maze.GetLength(1) + 7; j > -9; j--)
        {
            CreateColliders(-8, j);
            CreateColliders(m_Maze.GetLength(0) + 7, j);
        }

        time = initialTime;
        distanceMaxValue = new Vector2(edgeLength * (mazeRows - 1), edgeLength * (mazeColumns - 1));
        distanceCurrentValue = new Vector2(0f, 0f);
        // distanceMinValue = new Vector2(0f, 0f);

        if (MainController.mc != null)
        {
            if (MainController.mc.isSoundOff)
            {
                GetComponent<AudioSource>().volume = 0f;
                player.GetComponent<AudioSource>().volume = 0f;
                //soundButtonText.text = "<color=#E69900>◆</color> Sound (Off)";
                soundButtonText.text = "Sound (Off) <color=#E69900>◆</color>";
            }
            else
            {
                GetComponent<AudioSource>().volume = 1f;
                player.GetComponent<AudioSource>().volume = 1f;
                //soundButtonText.text = "<color=#E69900>◆</color> Sound (On)";
                soundButtonText.text = "Sound (On) <color=#E69900>◆</color>";
            }
        }
    }

    void CreateColliders(int i, int j)
    {
        int type = Mathf.Abs(i) % 2 * 2 + Mathf.Abs(j) % 2;
        switch (type)
        {
            case 0:
                Instantiate(colliderCorner,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 90f * Random.Range(0, 4), 0));
                break;
            case 1:
                Instantiate(colliderEdge,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));
                break;
            case 2:
                Instantiate(colliderEdge,
                    new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                    Quaternion.Euler(0, 180f * Random.Range(0, 2), 0));
                break;
            default:
                break;
        }
    }

    public void SetExited()
    {
        if (hasExited) return;
        hasExited = true;

        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = exitClip;
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().Play();

        if (!isMenuShowed)
        {
            MenuButton();
        }
    }
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
    public void SetShiftPressed()
    {
        hasPressedShift = true;
    }
#endif

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MenuButton();
        }

#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
        if (isMenuShowed && Input.GetKeyDown(KeyCode.O))
        {
            SoundButton();
            MenuButton();
        }
        if (isMenuShowed && Input.GetKeyDown(KeyCode.M))
        {
            MoveButton();
            MenuButton();
        }
        if (isMenuShowed && Input.GetKeyDown(KeyCode.N))
        {
            NewButton();
            MenuButton();
        }
        if (isMenuShowed && Input.GetKeyDown(KeyCode.Q))
        {
            QuitButton();
            MenuButton();
        }
#endif
    }

    void FixedUpdate()
    {
        if (!hasExited)
        {
            time -= Time.fixedDeltaTime;
            if (!isTimeout && time <= 0f)
            {
                isTimeout = true;
                GetComponent<AudioSource>().clip = timeoutClip;
                GetComponent<AudioSource>().loop = true;
                GetComponent<AudioSource>().Play();
            }
        }

        int sign = (int)Mathf.Sign(time);
        int hour = Mathf.Abs((int)time / 3600);
        int minute2 = Mathf.Abs((int)time % 3600 / 600);
        int minute1 = Mathf.Abs((int)time / 60 % 10);
        int second2 = Mathf.Abs((int)time % 60 / 10);
        int second1 = Mathf.Abs((int)time % 10);
        int secondDot1 = Mathf.Abs((int)(time * 10) % 10);
        int secondDot2 = Mathf.Abs((int)(time * 100) % 10);

        if (sign <= 0)
        {
            timeText.text = "<color=#FF1100>-";
        }
        else
        {
            if (hasExited) timeText.text = "<color=#00FF57>";
            else timeText.text = "";
        }

        if (hour > 0)
            timeText.text += hour + ":" + minute2 + "" + minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";
        else if (minute2 > 0)
            timeText.text += minute2 + "" + minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";
        else
            timeText.text += minute1 + ":" + second2 + "" + second1 + "." + secondDot1 + "" + secondDot2 + "\n";

        if (sign <= 0 || hasExited)
            timeText.text += "</color>";

        if (time <= 0f)
        {
            timeText.text += "Time out!";
        }
        else if (hasExited)
        {
            timeText.text += "Congraturations!";
        }
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
        else if (!hasPressedShift)
        {
            timeText.text += "Press 'Left Shift' or 'Right Click' to dash.";
        }
#endif
        else
        {
            if (mazeColumns == 12 && mazeRows == 12)
            {
                timeText.text += "Level: Easy";
            }
            else if (mazeColumns == 18 && mazeRows == 18)
            {
                timeText.text += "Level: Normal";
            }
            else if (mazeColumns == 24 && mazeRows == 24)
            {
                timeText.text += "Level: Hard";
            }
        }

        distanceCurrentValue = new Vector2(
            player.GetComponent<Transform>().position.x / distanceMaxValue.x,
            player.GetComponent<Transform>().position.z / distanceMaxValue.y
        );
        progressSlider.SetValues(distanceCurrentValue);
    }

    public void MenuButton()
    {
        if (isMenuShowed)
        {
            isMenuShowed = false;
            menu.SetActive(false);
            //menuButton.interactable = false;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
            menuButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.11f, 0.98f);
            menuButtonText.text = "<color=#E6C700>◆</color> Menu";
#else
            menuButtonText.text = "Menu <color=#E6C700>◆</color>";
#endif
        }
        else
        {
            isMenuShowed = true;
            menu.SetActive(true);
            //menuButton.interactable = true;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
            menuButton.GetComponent<RectTransform>().anchorMax = new Vector2(0.15f, 0.98f);
            menuButtonText.text = "<color=#E6C700>◆</color> Hide Menu";
#else
            menuButtonText.text = "Hide Menu <color=#E6C700>◆</color>";
#endif
        }
    }

    public void SoundButton()
    {
        if (!MainController.mc.isSoundOff)
        {
            MainController.mc.isSoundOff = true;
            GetComponent<AudioSource>().volume = 0f;
            player.GetComponent<AudioSource>().volume = 0f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
            soundButtonText.text = "<color=#E69900>◆</color> Sound (Off)";
#else
            soundButtonText.text = "Sound (Off) <color=#E69900>◆</color>";
#endif
        }
        else
        {
            MainController.mc.isSoundOff = false;
            GetComponent<AudioSource>().volume = 1f;
            player.GetComponent<AudioSource>().volume = 1f;
#if !((UNITY_ANDROID || UNITY_IOS || UNITY_WP8 || UNITY_WP8_1) && !UNITY_EDITOR)
            soundButtonText.text = "<color=#E69900>◆</color> Sound (On)";
#else
            soundButtonText.text = "Sound (On) <color=#E69900>◆</color>";
#endif
        }
    }

    public void MoveButton()
    {
        player.GetComponent<Transform>().position = initialPlayerPosition;
    }

    public void NewButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitButton()
    {
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }
}
