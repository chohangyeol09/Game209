using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CWH_Pause : MonoBehaviour
{
    public bool _paused;
    public GameObject _pause;
    public Image _volumeSetting;
    private void Start()
    {
        _pause.gameObject.SetActive(false);
        _volumeSetting.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!_paused)
            {
                Time.timeScale = 0f;
                _pause.gameObject.SetActive(true);
                _paused = true;
            }
            else
            {
                Time.timeScale = 1f;
                _pause.gameObject.SetActive(false);
                _volumeSetting.gameObject.SetActive(false);
                _paused = false;
            }
        }
    }
}
