using UnityEngine;
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
        }

        public void OnResumePressed()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OnRestartPressed()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.ReloadCurrentLevel();
        }

        public void OnMenuPressed()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.ReturnToMenu();
        }

        private void OnDestroy() => Time.timeScale = 1f;
    }
}
