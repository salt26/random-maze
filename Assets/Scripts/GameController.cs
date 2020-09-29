using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices.ComTypes;

public class GameController : MonoBehaviour
{
    public static GameController gc;

    public float edgeLength;
    public GameObject[] corners;
    public GameObject[] edges;
    public GameObject colliderCorner;
    public GameObject colliderEdge;
    public GameObject colliderExit;
    public Text timeText;
    public AudioClip exitClip;
    public AudioClip timeoutClip;

    public float initialTime = 300f;

    public bool mazeFromFile = false;

    public int mazeColumns = 15;
    public int mazeRows = 15;
    public int mazeInnerColumns = 5;
    public int mazeInnerRows = 5;

    private float time;
    private int[,] m_Maze;
    private bool hasExited = false;
    private bool hasPressedShift = false;
    private bool isTimeout = false;

    void Awake()
    {
        if (gc != null)
        {
            Destroy(gc.gameObject);
        }
        gc = this;
    }

    void Start()
    {
        Cursor.visible = false;
        MazeGenerator m_MazeGenerator = new MazeGenerator();
        // simple setting
        m_MazeGenerator.setRatio(.75f);
        bool fileExist = false;

        mazeColumns = Mathf.Clamp(mazeColumns, 5, 26);
        mazeRows = Mathf.Clamp(mazeRows, 5, 26);
        mazeInnerColumns = Mathf.Clamp(mazeInnerColumns, 0, mazeColumns);
        mazeInnerRows = Mathf.Clamp(mazeInnerRows, 0, mazeRows);

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
                m_Maze = new int[2 * cols - 1, 2 * rows - 1];
                for (int i = 0; i < 2 * cols - 1; i++)
                {
                    line = reader.ReadLine();
                    for (int j = 0; j < 2 * rows - 1; j++)
                    {
                        m_Maze[i, j] = line[j * 2] == '#' ? 1 : 0;
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
            m_Maze = m_MazeGenerator.FromDimensions(mazeColumns, mazeRows, mazeInnerColumns, mazeInnerRows);
            // make two entrances
            m_Maze[0, 1] = 0;
            m_Maze[2 * mazeColumns, 2 * mazeRows - 1] = 0;

            //Debug.Log(m_MazeGenerator.ConvertToString(m_Maze));
        }


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
                            new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position,
                            Quaternion.Euler(0, 90f * Random.Range(0, 4), 0));
                        break;
                    case 1:
                        Instantiate(edges[Random.Range(0, edges.Length)],
                            new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position, 
                            Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));
                        break;
                    case 2:
                        Instantiate(edges[Random.Range(0, edges.Length)],
                            new Vector3(edgeLength * j / 2f, 0f, edgeLength * i / 2f) + transform.position, 
                            Quaternion.Euler(0, 180f * Random.Range(0, 2), 0));
                        break;
                    default:
                        break;
                }
            }
        }

        // make exit collider
        Instantiate(colliderExit,
            new Vector3(edgeLength * (2 * mazeRows - 1) / 2f, 0f, edgeLength * mazeColumns) + transform.position,
            Quaternion.Euler(0, 90f + 180f * Random.Range(0, 2), 0));

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
        hasExited = true;

        GetComponent<AudioSource>().Stop();
        GetComponent<AudioSource>().clip = exitClip;
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().Play();
    }
    public void SetShiftPressed()
    {
        hasPressedShift = true;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
            timeText.text = "<color=#ff0000>-";
        }
        else
        {
            if (hasExited) timeText.text = "<color=#0000ff>";
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

        if (hasExited || time <= 0f)
        {
            timeText.text += "Press 'R' to restart, or 'Esc' to quit.";
        }
        else if (!hasPressedShift)
        {
            timeText.text += "Press 'Left Shift' or 'Left Click' to dash.";
        }
    }
}
