using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ChromaticCascade.UI
{
    /// <summary>
    /// Manages the main gameplay HUD display
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Score Display")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI scoreLabelText;
        
        [Header("Combo Display")]
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Image comboMeterFill;
        [SerializeField] private GameObject comboPanel;
        
        [Header("Next Block Preview")]
        [SerializeField] private Image nextBlockPreview;
        [SerializeField] private TextMeshProUGUI nextBlockLabel;
        
        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        
        private int currentScore = 0;
        private int currentCombo = 0;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeUI();
        }
        
        private void Start()
        {
            // Subscribe to gameplay events
            SubscribeToEvents();
            
            // Initialize displays
            UpdateScore(0);
            UpdateCombo(0, 1f);
        }
        
        private void InitializeUI()
        {
            // Set up button listeners
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
            
            // Hide panels initially
            if (pausePanel != null)
                pausePanel.SetActive(false);
            
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            if (comboPanel != null)
                comboPanel.SetActive(false);
        }
        
        private void SubscribeToEvents()
        {
            // Subscribe to score events (will be implemented in Sprint 2)
            // TODO: ScoreManager.OnScoreChanged += UpdateScore;
            // TODO: ScoreManager.OnComboChanged += UpdateCombo;
        }
        
        /// <summary>
        /// Update the score display
        /// </summary>
        public void UpdateScore(int newScore)
        {
            currentScore = newScore;
            
            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString("N0");
            }
        }
        
        /// <summary>
        /// Update the combo display
        /// </summary>
        public void UpdateCombo(int comboCount, float multiplier)
        {
            currentCombo = comboCount;
            
            if (comboPanel != null)
            {
                comboPanel.SetActive(comboCount > 1);
            }
            
            if (comboText != null)
            {
                comboText.text = $"x{multiplier:F1}";
            }
            
            if (comboMeterFill != null)
            {
                // Placeholder - will animate based on combo decay timer
                comboMeterFill.fillAmount = comboCount > 0 ? 1f : 0f;
            }
        }
        
        /// <summary>
        /// Update next block preview
        /// </summary>
        public void UpdateNextBlockPreview(Sprite blockSprite, Color blockColor)
        {
            if (nextBlockPreview != null)
            {
                nextBlockPreview.sprite = blockSprite;
                nextBlockPreview.color = blockColor;
            }
        }
        
        /// <summary>
        /// Show pause panel
        /// </summary>
        public void ShowPausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                Time.timeScale = 0f; // Pause game
            }
        }
        
        /// <summary>
        /// Hide pause panel
        /// </summary>
        public void HidePausePanel()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                Time.timeScale = 1f; // Resume game
            }
        }
        
        /// <summary>
        /// Show game over panel
        /// </summary>
        public void ShowGameOverPanel(int finalScore, int highScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                
                // Update final score text
                if (finalScoreText != null)
                {
                    finalScoreText.text = $"Score: {finalScore}";
                }
            }
        }
        
        /// <summary>
        /// Handle pause button click
        /// </summary>
        private void OnPauseClicked()
        {
            ShowPausePanel();
        }
        
        /// <summary>
        /// Toggle pause state (for button callbacks)
        /// </summary>
        public void TogglePause()
        {
            if (pausePanel != null && pausePanel.activeSelf)
            {
                HidePausePanel();
            }
            else
            {
                ShowPausePanel();
            }
        }
        
        /// <summary>
        /// Resume game from pause
        /// </summary>
        public void OnResumeClicked()
        {
            HidePausePanel();
        }
        
        /// <summary>
        /// Restart current game
        /// </summary>
        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            // Will be implemented with SceneManager
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            // Will be implemented with SceneManager
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            // TODO: Implement when score manager is ready
        }
    }
}
