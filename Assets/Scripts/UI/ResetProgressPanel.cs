using UnityEngine;
using Emberfall.Save;

namespace Emberfall.UI
{

    public class ResetProgressPanel : MonoBehaviour
    {
        [SerializeField] private GameObject confirmPanel;

        private void Awake() => confirmPanel.SetActive(false);

        public void OnResetPressed() => confirmPanel.SetActive(true);
        public void OnCancelPressed() => confirmPanel.SetActive(false);

        public void OnConfirmPressed()
        {
            SaveManager.Instance?.ResetProgress();
            confirmPanel.SetActive(false);
        }
    }
}
