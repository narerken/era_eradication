using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using Emberfall.Save;

namespace Emberfall.UI
{

    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private MainMenuController menu;

        private void OnEnable()
        {
            var data = SaveManager.Instance != null ? SaveManager.Instance.Data : null;
            if (data == null) return;
            musicSlider.value = data.musicVolume;
            sfxSlider.value = data.sfxVolume;
        }

        public void OnVolumeChanged(float _)
        {
            ApplyToMixer();
            SaveManager.Instance?.SetVolumes(musicSlider.value, sfxSlider.value);
        }

        private void ApplyToMixer()
        {
            if (mixer == null) return;
            mixer.SetFloat("MusicVolume", LinearToDb(musicSlider.value));
            mixer.SetFloat("SfxVolume", LinearToDb(sfxSlider.value));
        }

        private static float LinearToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

        public void OnBackPressed() => menu.ShowMenu();
    }
}
