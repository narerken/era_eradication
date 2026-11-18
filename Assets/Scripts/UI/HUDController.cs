using UnityEngine;
using UnityEngine.UI;
using Emberfall.Core;

namespace Emberfall.UI
{

    public class HUDController : MonoBehaviour
    {
        [SerializeField] private Transform heartsContainer;
        [SerializeField] private Text scoreText;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private PauseController pauseController;

        private void OnEnable()
        {
            GameEvents.HeartsChanged += OnHeartsChanged;
            GameEvents.ScoreChanged += OnScoreChanged;
            GameEvents.LevelCompleted += OnLevelCompleted;
            GameEvents.PlayerDied += OnPlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.HeartsChanged -= OnHeartsChanged;
            GameEvents.ScoreChanged -= OnScoreChanged;
            GameEvents.LevelCompleted -= OnLevelCompleted;
            GameEvents.PlayerDied -= OnPlayerDied;
        }

        private void Start()
        {
            levelCompletePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            OnScoreChanged(0);
        }

        private void OnHeartsChanged(int current, int max)
        {
            for (int i = 0; i < heartsContainer.childCount; i++)
                heartsContainer.GetChild(i).gameObject.SetActive(i < current);
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = "SCORE " + score;
        }

        private void OnLevelCompleted() => levelCompletePanel.SetActive(true);
        private void OnPlayerDied() => gameOverPanel.SetActive(true);

        public void OnPausePressed() => pauseController.Pause();
        public void OnRetryPressed() => GameManager.Instance?.ReloadCurrentLevel();
        public void OnMenuFromEndPressed() => GameManager.Instance?.ReturnToMenu();
        public void OnNextLevelPressed()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.CurrentLevel == null) return;
            int nextIndex = gm.CurrentLevel.levelIndex;
            foreach (var lvl in gm.allLevels)
                if (lvl.levelIndex == gm.CurrentLevel.levelIndex + 1) { gm.LoadLevel(lvl); return; }
            gm.ReturnToMenu();
        }
    }
}
