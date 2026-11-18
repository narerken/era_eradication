using UnityEngine;
using UnityEngine.SceneManagement;
using Emberfall.Data;
using Emberfall.Save;

namespace Emberfall.Core
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("All levels, in order")]
        public LevelData[] allLevels;

        public LevelData CurrentLevel { get; private set; }
        public int RunScore { get; private set; }
        public int RunHearts { get; private set; }

        private const string MenuSceneName = "MainMenu";
        private const string GameSceneName = "Game";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadLevel(LevelData level)
        {
            CurrentLevel = level;
            RunScore = 0;
            SceneManager.LoadScene(GameSceneName);
        }

        public void ReloadCurrentLevel()
        {
            if (CurrentLevel != null) LoadLevel(CurrentLevel);
        }

        public void ReturnToMenu() => SceneManager.LoadScene(MenuSceneName);

        public void AddScore(int amount)
        {
            RunScore += amount;
            GameEvents.RaiseScoreChanged(RunScore);
        }

        public void CompleteCurrentLevel()
        {
            if (CurrentLevel == null) return;
            SaveManager.Instance?.SubmitScore(RunScore);
            int next = CurrentLevel.levelIndex + 1;
            if (next <= allLevels.Length) SaveManager.Instance?.UnlockLevel(next);
        }
    }
}
