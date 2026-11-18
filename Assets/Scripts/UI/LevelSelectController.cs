using UnityEngine;
using UnityEngine.UI;
using Emberfall.Core;
using Emberfall.Data;
using Emberfall.Save;

namespace Emberfall.UI
{

    public class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private MainMenuController menu;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject levelButtonPrefab;

        public void Refresh()
        {
            foreach (Transform child in buttonContainer) Destroy(child.gameObject);

            var gm = GameManager.Instance;
            if (gm == null || gm.allLevels == null) return;

            foreach (var level in gm.allLevels)
            {
                bool unlocked = SaveManager.Instance == null || SaveManager.Instance.IsUnlocked(level.levelIndex);
                var buttonGO = Instantiate(levelButtonPrefab, buttonContainer);

                var label = buttonGO.transform.Find("Label")?.GetComponent<Text>();
                if (label != null) label.text = unlocked ? level.displayName : "LOCKED";

                var lockIcon = buttonGO.transform.Find("Lock");
                if (lockIcon != null) lockIcon.gameObject.SetActive(!unlocked);

                var button = buttonGO.GetComponent<Button>();
                button.interactable = unlocked;
                if (unlocked)
                {
                    LevelData captured = level;
                    button.onClick.AddListener(() => gm.LoadLevel(captured));
                }
            }
        }

        public void OnBackPressed() => menu.ShowMenu();
    }
}
