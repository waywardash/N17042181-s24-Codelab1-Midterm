using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager instance;

    // Text display for UI
    public TextMeshProUGUI display;

    // Current score
    private int score = 0;

    // Property for accessing and setting score
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            Debug.Log("score changed!");

            // Update high score if current score surpasses it
            if (score > HighScore)
            {
                HighScore = score;
            }
        }
    }

    // High score
    private int highScore = 0;

    // Key for saving high score in PlayerPrefs
    const string KEY_HIGH_SCORE = "High Score";

    // Property for accessing and setting high score
    int HighScore
    {
        get
        {
            // Retrieve high score from file
            if (File.Exists(DATA_FULL_HS_FILE_PATH))
            {
                string fileContents = File.ReadAllText(DATA_FULL_HS_FILE_PATH);
                highScore = Int32.Parse(fileContents);
            }
            return highScore;
        }
        set
        {
            // Set high score and save it to file
            highScore = value;
            Debug.Log("New High Score!!!");
            string fileContent = "" + highScore;

            // Ensure directory exists for saving file
            if (!Directory.Exists(Application.dataPath + DATA_DIR))
            {
                Directory.CreateDirectory(Application.dataPath + DATA_DIR);
            }

            // Write high score to file
            File.WriteAllText(DATA_FULL_HS_FILE_PATH, fileContent);
        }
    }

    // File paths for saving high score
    const string FILE_DIR = "/DATA/";
    const string DATA_FILE = "highScores.txt";
    const string DATA_DIR = "/Data/";
    const string DATA_HS_FILE = "hs.txt";

    string FILE_FULL_PATH;
    string DATA_FULL_HS_FILE_PATH;

    // Target score for level progression
    public int targetScore = 3;

    // Current level number
    int levelNum = 1;

    // Timer and max time for game duration
    float timer = 0;
    public int maxTime = 10;
    bool isInGame = true;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        // Ensure only one instance of GameManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set file paths
        FILE_FULL_PATH = Application.dataPath + FILE_DIR + DATA_FILE;
        DATA_FULL_HS_FILE_PATH = Application.dataPath + DATA_DIR + DATA_HS_FILE;
    }

    // Update is called once per frame
    void Update()
    {
        // Delete high score key if space key is pressed (debug)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteKey(KEY_HIGH_SCORE);
        }

        // Update UI display with level, score, high score, and time
        if (isInGame)
        {
            display.text = "Level: " + levelNum + "\nScore: " + score + "\nHigh Score: " + HighScore + "\nTime:" + (maxTime - (int)timer);
        }
        else
        {
            display.text = "GAME OVER\nFINAL SCORE: " + score +
                           "\nHigh Scores:\n" + GetHighScoresString();
        }

        // Increment timer
        timer += Time.deltaTime;

        // Load end scene when game time is up
        if (timer >= maxTime && isInGame)
        {
            isInGame = false;
            SceneManager.LoadScene("EndScene");
            SetHighScore();
        }

        // Check if target score is reached to progress to next level
        if (score == targetScore)
        {
            levelNum++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            targetScore = Mathf.RoundToInt(targetScore + targetScore * 1.5f);
        }
    }

    // Check if current score is a high score
    bool IsHighScore(int score)
    {
        List<int> highScores = HighScores;
        for (int i = 0; i < highScores.Count; i++)
        {
            if (highScores[i] < score)
            {
                return true;
            }
        }
        return false;
    }

    // Set high score if current score is a high score
    void SetHighScore()
    {
        if (IsHighScore(score))
        {
            int highScoreSlot = -1;
            List<int> highScores = HighScores;

            // Find the appropriate slot for the new high score
            for (int i = 0; i < highScores.Count; i++)
            {
                if (score > highScores[i])
                {
                    highScoreSlot = i;
                    break;
                }
            }

            // Insert new high score into the list
            highScores.Insert(highScoreSlot, score);

            // Ensure only the top 5 high scores are retained
            highScores = highScores.GetRange(0, 5);

            // Convert high scores to string for writing to file
            string scoreBoardText = "";
            foreach (var highScore in highScores)
            {
                scoreBoardText += highScore + "\n";
            }

            // Write high scores to file
            File.WriteAllText(FILE_FULL_PATH, scoreBoardText);
        }
    }

    // Get high scores from file
    string GetHighScoresString()
    {
        List<int> highScores = HighScores;
        string scoreBoardText = "";
        foreach (var highScore in highScores)
        {
            scoreBoardText += highScore + "\n";
        }
        return scoreBoardText;
    }

    // Get high scores as a list of integers
    List<int> HighScores
    {
        get
        {
            List<int> highScores = new List<int>();
            if (File.Exists(FILE_FULL_PATH))
            {
                string highScoresString = File.ReadAllText(FILE_FULL_PATH).Trim();
                string[] highScoreArray = highScoresString.Split("\n");
                for (int i = 0; i < highScoreArray.Length; i++)
                {
                    int currentScore = Int32.Parse(highScoreArray[i]);
                    highScores.Add(currentScore);
                }
            }
            else
            {
                // Initialize with default high scores if file does not exist
                highScores.Add(3);
                highScores.Add(2);
                highScores.Add(1);
                highScores.Add(0);
            }
            return highScores;
        }
    }
}
