using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ChromaticCascade.Core
{
    /// <summary>
    /// Manages game states and state transitions
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        
        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.Menu;
        
        public GameState CurrentState => currentState;
        
        public event System.Action<GameState> OnStateChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Change to a new game state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;
            
            GameState previousState = currentState;
            currentState = newState;
            
            Debug.Log($"State changed: {previousState} -> {newState}");
            
            OnStateChanged?.Invoke(newState);
            
            HandleStateChange(newState);
        }
        
        /// <summary>
        /// Handle state-specific logic
        /// </summary>
        private void HandleStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    break;
                
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
            }
        }
        
        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }
        
        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }
        
        /// <summary>
        /// Trigger game over
        /// </summary>
        public void GameOver()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.GameOver);
            }
        }
        
        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            ChangeState(GameState.Playing);
        }
        
        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
        }
    }
    
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }
}
