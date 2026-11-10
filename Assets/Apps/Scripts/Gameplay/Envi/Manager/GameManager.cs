
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Players")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Level Settings")]
    [SerializeField] private int firstLevelBuildIndex = 1;

    private int currentLevel;
    private bool levelCompleted;

    public Transform Player1 => player1;
    public Transform Player2 => player2;

    public int CurrentLevel => currentLevel;
    public bool LevelCompleted => levelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        UpdateCurrentLevel();
    }

    private void Start()
    {
        FindPlayers();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        levelCompleted = false;

        UpdateCurrentLevel();
        FindPlayers();
    }

    private void UpdateCurrentLevel()
    {
        currentLevel = SceneManager.GetActiveScene().buildIndex -
                       firstLevelBuildIndex + 1;

        if (currentLevel < 1)
            currentLevel = 0;
    }

    public void FindPlayers()
    {
        GameObject p1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject p2 = GameObject.FindGameObjectWithTag("Player2");

        player1 = p1 != null ? p1.transform : null;
        player2 = p2 != null ? p2.transform : null;

        if (player1 == null)
            Debug.LogWarning("Player1 was not found!");

        if (player2 == null)
            Debug.LogWarning("Player2 was not found!");
    }

    public void SetPlayers(Transform p1, Transform p2)
    {
        player1 = p1;
        player2 = p2;
    }

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        Debug.Log("Level " + currentLevel + " completed!");
    }

    public void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("All levels completed!");
            return;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadLevel(int buildIndex)
    {
        if (buildIndex < 0 ||
            buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError("Invalid scene build index: " + buildIndex);
            return;
        }

        SceneManager.LoadScene(buildIndex);
    }
}
