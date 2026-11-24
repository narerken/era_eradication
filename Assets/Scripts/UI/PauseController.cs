using UnityEngine;
using Emberfall.Audio;
using Emberfall.Core;

namespace Emberfall.UI
{

    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        private void Awake() => pausePanel.SetActive(false);

        public void Pause()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            AudioManager.Instance?.SetPaused(true);
        }

        public void OnResumePressed()
        {
            AudioManager.Instance?.PlayUIClick();
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
        }

        public void OnRestartPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
            GameManager.Instance?.ReloadCurrentLevel();
        }

        public void OnMenuPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
            GameManager.Instance?.ReturnToMenu();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.SetPaused(false);
        }
    }
}
