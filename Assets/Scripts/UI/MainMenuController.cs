using UnityEngine;
using Emberfall.Audio;
using Emberfall.Core;
using Emberfall.Data;
using Emberfall.Save;

namespace Emberfall.UI
{

    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private LevelSelectController levelSelect;

        private void Start() => ShowMenu();

        public void ShowMenu()
        {
            menuPanel.SetActive(true);
            levelSelectPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OnPlayPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            menuPanel.SetActive(false);
            levelSelectPanel.SetActive(true);
            levelSelect.Refresh();
        }

        public void OnSettingsPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            menuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnQuitPressed()
        {
            AudioManager.Instance?.PlayUIClick();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
