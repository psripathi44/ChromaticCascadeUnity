using UnityEngine;
using TMPro;
using System.Collections;

namespace ChromaticCascade.UI
{
    /// <summary>
    /// Animates score display with count-up effect
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshProUGUI scoreText;
        
        [Header("Animation")]
        [SerializeField] private float countUpDuration = 0.5f;
        [SerializeField] private bool useCountUpAnimation = true;
        
        [Header("Flash Effect")]
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private Color flashColor = Color.yellow;
        
        private int currentDisplayedScore = 0;
        private int targetScore = 0;
        private Coroutine countUpCoroutine;
        private Color originalColor;
        
        private void Awake()
        {
            if (scoreText == null)
                scoreText = GetComponent<TextMeshProUGUI>();
            
            if (scoreText != null)
                originalColor = scoreText.color;
        }
        
        /// <summary>
        /// Update score with animation
        /// </summary>
        public void UpdateScore(int newScore, bool animate = true)
        {
            targetScore = newScore;
            
            if (useCountUpAnimation && animate && newScore > currentDisplayedScore)
            {
                if (countUpCoroutine != null)
                    StopCoroutine(countUpCoroutine);
                
                countUpCoroutine = StartCoroutine(CountUpAnimation());
            }
            else
            {
                currentDisplayedScore = targetScore;
                UpdateScoreText();
            }
        }
        
        /// <summary>
        /// Count up animation from current to target score
        /// </summary>
        private IEnumerator CountUpAnimation()
        {
            int startScore = currentDisplayedScore;
            float elapsed = 0f;
            
            while (elapsed < countUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / countUpDuration;
                
                currentDisplayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                UpdateScoreText();
                
                yield return null;
            }
            
            currentDisplayedScore = targetScore;
            UpdateScoreText();
        }
        
        /// <summary>
        /// Update the text display
        /// </summary>
        private void UpdateScoreText()
        {
            if (scoreText != null)
            {
                scoreText.text = currentDisplayedScore.ToString("N0");
            }
        }
        
        /// <summary>
        /// Flash the score text (e.g., on combo)
        /// </summary>
        public void Flash()
        {
            if (scoreText != null)
            {
                StartCoroutine(FlashAnimation());
            }
        }
        
        /// <summary>
        /// Flash animation coroutine
        /// </summary>
        private IEnumerator FlashAnimation()
        {
            scoreText.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            scoreText.color = originalColor;
        }
    }
}
