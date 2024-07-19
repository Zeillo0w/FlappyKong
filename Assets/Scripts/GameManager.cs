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

    public GameObject scorePanel;

    public Text gameStoppedScoreText;
    public Text gameStoppedHighScoreText;

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
            lives = 5;
            PlayerPrefs.SetFloat("Lives", lives);
        }

        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(false);
            quitButton.onClick.AddListener(QuitGame);
            scorePanel.SetActive(false);
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
    }

    public void GameStop()
    {
        Time.timeScale = 0;
        CurrentGameState = GameState.GameStop;
        
        if (quitButton != null)
        {
            quitButton.gameObject.SetActive(true);
            scorePanel.SetActive(true);
            gameStoppedScoreText.text = ScoreManager.Instance.score.ToString();
            gameStoppedHighScoreText.text = ScoreManager.Instance.highScore.ToString();

            // Envoie du meilleur score à la base de données via JavaScript (index.html)
            string jsCode = @"
                var highScore = " + ScoreManager.Instance.highScore.ToString() + @";
                var xhr = new XMLHttpRequest();
                xhr.open('POST', '../save_score.php', true);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                xhr.onreadystatechange = function() {
                    if (xhr.readyState === XMLHttpRequest.DONE) {
                        if (xhr.status === 200) {
                            console.log('Score envoyé avec succès à la base de données.');
                        } else {
                            console.error('Erreur lors de l\'envoi du score à la base de données:', xhr.statusText);
                        }
                    }
                };
                xhr.send('highScore=' + highScore);
            ";

            // Exécute le code JavaScript depuis Unity WebGL
            Application.ExternalEval(jsCode);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        lives = 5;
        PlayerPrefs.SetFloat("Lives", lives);
        PlayerPrefs.Save();

        RestartGame();

        // Appel à la fonction JavaScript banUserAndRedirect définie dans index.html
        Application.ExternalCall("banUserAndRedirect");
    }
}