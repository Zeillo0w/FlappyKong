using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;

    static public event Action OnGameStarted;
    static public event Action OnGameEnded;

    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
        GameStop
    }

    public GameState CurrentGameState;

    public float speedPipe;
    public float numberPipes;
    public float distanceBetweenPipes;
    public float lives;

    public Pipe pipePrefab;
    public Transform pipeSpawnPoint;
    public Button quitButton;

    public GameObject scorePannel;

    public Text GameStopedScoreText;

    public Text GameStopedHighScoreText;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;


        if (PlayerPrefs.HasKey("Lives"))
        {
            lives = PlayerPrefs.GetFloat("Lives");
        }
        else
        {

            lives = 3;
            PlayerPrefs.SetFloat("Lives", lives);
        }


        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);

            quitButton.onClick.AddListener(QuitGame);

            scorePannel.SetActive(false);
        }
    }

    void Start()
    {
        CurrentGameState = GameState.MainMenu;

        GeneratePipes();
    }

    void GeneratePipes()
    {
        for (int i = 0; i < numberPipes; i++)
        {
            Pipe pipe = Instantiate(pipePrefab, pipeSpawnPoint.position + new Vector3(i * distanceBetweenPipes, 0, 0), Quaternion.identity);
        }
    }

    public void StartGame()
    {
        CurrentGameState = GameState.Playing;
        OnGameStarted?.Invoke();
    }

    public void GameOver()
    {
        CurrentGameState = GameState.GameOver;
        CameraController.Instance.Shake(0.3f, 0.25f);
        OnGameEnded?.Invoke();
    }

    public void LoseLife()
    {
        lives--;

        PlayerPrefs.SetFloat("Lives", lives);

        if (lives <= 0)
        {
            GameStop();
        }
        else
        {
        }
    }

    public void GameStop()
    {
        Time.timeScale = 0;
        CurrentGameState = GameState.GameStop;
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
            scorePannel.SetActive(true);
            GameStopedScoreText.text = ScoreManager.Instance.score.ToString();
            GameStopedHighScoreText.text = ScoreManager.Instance.highScore.ToString();

            

        }
    }

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        lives = 3;
        PlayerPrefs.SetFloat("Lives", lives);
        PlayerPrefs.Save();

        RestartGame();

        Application.ExternalCall("banUserAndRedirect");
    }

}