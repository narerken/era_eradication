using UnityEngine;
using Emberfall.Audio;
using Emberfall.Save;

namespace Emberfall.UI
{

    public class ResetProgressPanel : MonoBehaviour
    {
        [SerializeField] private GameObject confirmPanel;

        private void Awake() => confirmPanel.SetActive(false);

        public void OnResetPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            confirmPanel.SetActive(true);
        }

        public void OnCancelPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            confirmPanel.SetActive(false);
        }

        public void OnConfirmPressed()
        {
            AudioManager.Instance?.PlayUIClick();
            SaveManager.Instance?.ResetProgress();
            confirmPanel.SetActive(false);
        }
    }
}
