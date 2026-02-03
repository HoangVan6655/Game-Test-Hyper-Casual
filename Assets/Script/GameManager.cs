using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private static bool hasSessionStarted = false; 

    [Header("--- SYSTEM REFERENCES ---")]
    public SimpleSquareBuilder mapBuilder;
    public LaneController leftLane;
    public LaneController rightLane;
    public HoleBuilder holeBuilder;

    [Header("--- UI & CONTROLS ---")]
    public TextMeshProUGUI timerText; 
    public Slider progressSlider;     
    public GameObject victoryPanel;   
    public GameObject failPanel;      
    public Button nextLevelButton;
    public Button restartButton;

    [Header("--- GAME CONFIG ---")]
    public List<Color> gamePalette = new List<Color>() { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan };

    private int currentLevel = 0; 
    private float currentTime;
    private bool isGameActive = false;
    private int totalScoreRequired = 0; 
    private int currentScore = 0;       

    void Awake() 
    { 
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeGameSession();
        SetupUI();
        StartLevel();
    }

    void InitializeGameSession()
    {
        if (!hasSessionStarted) 
        { 
            currentLevel = 0; 
            PlayerPrefs.SetInt("SavedLevel", 0); 
            hasSessionStarted = true; 
        }
        else 
        { 
            currentLevel = PlayerPrefs.GetInt("SavedLevel", 0); 
        }
    }

    void SetupUI()
    {
        if (nextLevelButton) nextLevelButton.onClick.AddListener(OnNextLevelClick);
        if (restartButton) restartButton.onClick.AddListener(OnRestartClick);
        if (victoryPanel) victoryPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.Max(0, Mathf.FloorToInt(currentTime)) + "s";
        
        if (currentTime <= 0) EndGame(false);
    }

    void StartLevel()
    {
        currentTime = 30f + (currentLevel * 10); 
        isGameActive = true;
        currentScore = 0;

        switch (currentLevel)
        {
            case 0: SetupLevel_Square(); break;
            case 1: SetupLevel_Rectangle(); break;
            default: SetupLevel_Triangle(); break;
        }

        SetupCamera();
    }

    #region LEVEL SETUP LOGIC
    void SetupLevel_Square()
    {
        mapBuilder.BuildSquare();
        AlignAndBuildLanes(); 

        int groupSize = 20; 
        Color c1 = GetRandomColor();
        Color c2 = GetRandomColor(c1);

        SetupLane(leftLane, groupSize, groupSize, new List<Color> { c1 });
        SetupLane(rightLane, groupSize, groupSize, new List<Color> { c2 });

        holeBuilder.BuildMultipleHoles(new List<Color>{c1, c2}, new List<int>{groupSize, groupSize});
        SetupSlider(groupSize * 2);
    }

    void SetupLevel_Rectangle()
    {
        mapBuilder.BuildRectangle();
        AlignAndBuildLanes();

        int groupSize = 15; 
        List<Color> colors = GetUniqueColors(4);

        SetupLane(leftLane, groupSize, groupSize * 2, new List<Color> { colors[0], colors[1] });
        SetupLane(rightLane, groupSize, groupSize * 2, new List<Color> { colors[2], colors[3] });
        
        holeBuilder.BuildMultipleHoles(colors, new List<int>{groupSize, groupSize, groupSize, groupSize});
        SetupSlider(groupSize * 4);
    }

    void SetupLevel_Triangle()
    {
        mapBuilder.BuildTriangle();
        AlignAndBuildLanes();

        int groupSize = 20;
        List<Color> colors = GetUniqueColors(4);

        SetupLane(leftLane, groupSize, groupSize * 2, new List<Color> { colors[0], colors[1] });
        SetupLane(rightLane, groupSize, groupSize * 2, new List<Color> { colors[2], colors[3] });

        holeBuilder.BuildMultipleHoles(colors, new List<int>{groupSize, groupSize, groupSize, groupSize});
        SetupSlider(groupSize * 4);
    }
    #endregion

    #region HELPER METHODS
    void AlignAndBuildLanes()
    {
        if (mapBuilder == null) return;

        Vector3 origin = mapBuilder.transform.position;
        float w = (currentLevel == 1) ? mapBuilder.rectWidth : mapBuilder.sideLength;
        float h = (currentLevel == 1) ? mapBuilder.rectHeight : mapBuilder.sideLength;
        if (currentLevel >= 2) { w += 5; h += 5; } 

        float centerX = origin.x + (w * mapBuilder.spacing / 2f);
        float bottomZ = origin.z - (h * mapBuilder.spacing);
        float laneZ = bottomZ - 0f; // Gap = 0
        float xSpacing = 2.5f; 

        UpdateLaneTransform(leftLane, new Vector3(centerX - xSpacing, origin.y, laneZ));
        UpdateLaneTransform(rightLane, new Vector3(centerX + xSpacing, origin.y, laneZ));
    }

    void UpdateLaneTransform(LaneController lane, Vector3 pos)
    {
        if (lane == null) return;
        lane.transform.position = pos;
        lane.transform.rotation = Quaternion.identity;
        if(lane.laneBuilder)
        {
            lane.laneBuilder.length = 1;
            lane.laneBuilder.BuildLane();
        }
    }

    void SetupCamera()
    {
        if (Camera.main == null || mapBuilder == null) return;

        float width = (currentLevel == 1) ? mapBuilder.rectWidth : mapBuilder.sideLength;
        if(currentLevel >= 2) width += 5;
        
        float centerX = mapBuilder.transform.position.x + (width * mapBuilder.spacing / 2f);

        Camera.main.transform.position = new Vector3(centerX, 40f, 4f);
        Camera.main.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void SetupLane(LaneController lane, int grpSize, int total, List<Color> cols)
    {
        if (lane == null) return;
        lane.groupSize = grpSize;
        lane.minionCount = total;
        lane.SpawnMinionsSequence(cols);
    }

    void SetupSlider(int total)
    {
        totalScoreRequired = total;
        if (progressSlider)
        {
            progressSlider.maxValue = total;
            progressSlider.value = 0;
        }
    }
    #endregion

    #region GAMEPLAY LOGIC
    public void AddScore()
    {
        if (!isGameActive) return;
        currentScore++;
        if (progressSlider) progressSlider.value = currentScore;
        if (currentScore >= totalScoreRequired) EndGame(true);
    }

    void EndGame(bool isWin)
    {
        isGameActive = false;
        if (isWin)
        {
            if (currentLevel >= 2 && nextLevelButton)
            {
                var txt = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt) txt.text = "Main Menu";
            }
            if (victoryPanel) victoryPanel.SetActive(true);
        }
        else
        {
            if (failPanel) failPanel.SetActive(true);
        }
    }

    public void OnRestartClick() 
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void OnNextLevelClick() 
    { 
        if (currentLevel >= 2)
        {
            PlayerPrefs.SetInt("SavedLevel", 0); 
            SceneManager.LoadScene("Menu");
        }
        else
        {
            currentLevel++; 
            PlayerPrefs.SetInt("SavedLevel", currentLevel); 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        }
    }
    #endregion

    #region UTILS
    List<Color> GetUniqueColors(int count)
    {
        List<Color> result = new List<Color>();
        while(result.Count < count) 
        {
            Color c = GetRandomColor();
            bool dup = false;
            foreach(Color e in result) if(IsSameColor(e, c)) dup = true;
            if(!dup) result.Add(c);
        }
        return result;
    }

    Color GetRandomColor(Color exclude = default)
    {
        Color c = gamePalette[Random.Range(0, gamePalette.Count)];
        if(exclude != default) 
        {
            int safe = 10;
            while(IsSameColor(c, exclude) && safe > 0) { c = gamePalette[Random.Range(0, gamePalette.Count)]; safe--; }
        }
        return c;
    }

    bool IsSameColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.05f && Mathf.Abs(c1.g - c2.g) < 0.05f && Mathf.Abs(c1.b - c2.b) < 0.05f;
    }
    #endregion
}