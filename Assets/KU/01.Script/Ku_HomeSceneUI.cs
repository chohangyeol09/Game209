using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ku_HomeSceneUI : MonoBehaviour
{
    [SerializeField] private Image uiImage;

    public GameObject _pauseObj;
    public  Image _volumeSetting;

    [SerializeField] private bool isTitle = true;

    private void Start()
    {
        if (!isTitle)
        {
            StartCoroutine(itsNotTitle());
        }
    }

    IEnumerator itsNotTitle()
    {
        if(uiImage != null) 
        {
            uiImage.DOFade(0f, 1f);
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
        }
    }
    public void OnPlayGame()
    {
        StartCoroutine(GameStart());
    }

    public void InAudioSet()
    {
        _volumeSetting.gameObject.SetActive(true);
    }
    public void OutAudioSet()
    {
        _volumeSetting.gameObject.SetActive(false);
    }
    public void OnExitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 정지
#else
        Application.Quit();  // 빌드된 게임 종료
#endif
    }

    IEnumerator GameStart()
    {
        if (isTitle)
        {
            uiImage.gameObject.SetActive(true);
            Color color = uiImage.color;
            color.a = 0f;
            uiImage.color = color;

            uiImage.DOFade(1f, 1f);
            yield return new WaitForSeconds(1.5f);
        }

        SceneManager.LoadScene(1);
    }
}
