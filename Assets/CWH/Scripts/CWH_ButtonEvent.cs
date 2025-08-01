using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class CWH_ButtonEvent : MonoBehaviour
{
    [SerializeField] CWH_Pause _pause;
 
    public void OnPlayGame()
    {
        _pause._paused = false;
        Time.timeScale = 1f;
        _pause._pause.gameObject.SetActive(false);
    }

    public void OnSetting()
    {
        _pause._volumeSetting.gameObject.SetActive(true);
    }

    public void OnExitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // �����Ϳ��� ����
#else
        Application.Quit();  // ����� ���� ����
#endif
    }
}
