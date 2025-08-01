using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class CWH_AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Scrollbar masterSlider;
    public Scrollbar bgmSlider;
    public Scrollbar sfxSlider;

    private void Start()
    {
        // 슬라이더 리스너 등록
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 저장된 값 불러오기
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // 슬라이더 값 반영
        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        sfxSlider.value = sfxVol;
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume <= 0.0001f ? 0.0001f : volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume <= 0.0001f ? 0.0001f : volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume <= 0.0001f ? 0.0001f : volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
