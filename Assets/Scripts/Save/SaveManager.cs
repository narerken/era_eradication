using System;
using System.IO;
using UnityEngine;

namespace Emberfall.Save
{

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        public SaveData Data { get; private set; }

        private string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.Log("[SaveManager] No save file yet, using defaults.");
                    Data = SaveData.CreateDefault();
                    return;
                }

                string json = File.ReadAllText(FilePath);
                var parsed = JsonUtility.FromJson<SaveData>(json);

                if (parsed == null || parsed.unlockedLevels == null || parsed.unlockedLevels.Count == 0)
                {
                    Debug.LogWarning("[SaveManager] save.json was unreadable/broken. Falling back to defaults.");
                    Data = SaveData.CreateDefault();
                    return;
                }

                Data = parsed;
            }
            catch (Exception e)
            {

                Debug.LogWarning($"[SaveManager] Failed to load save.json ({e.GetType().Name}: {e.Message}). Using defaults.");
                Data = SaveData.CreateDefault();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, prettyPrint: true);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Failed to write save.json: {e.Message}");
            }
        }

        public void UnlockLevel(int levelIndex)
        {
            if (!Data.unlockedLevels.Contains(levelIndex))
            {
                Data.unlockedLevels.Add(levelIndex);
                Save();
            }
        }

        public bool IsUnlocked(int levelIndex) => Data.unlockedLevels.Contains(levelIndex);

        public void SubmitScore(int score)
        {
            if (score > Data.bestScore)
            {
                Data.bestScore = score;
                Save();
            }
        }

        public void SetVolumes(float music, float sfx)
        {
            Data.musicVolume = music;
            Data.sfxVolume = sfx;
            Save();
        }

        public void ResetProgress()
        {
            Data = SaveData.CreateDefault();
            Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnApplicationQuit() => Save();
    }
}
