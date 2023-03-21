using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadGame : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMPro.TextMeshProUGUI progressText;
    AsyncOperation sceneLoading;
    void Start()
    {
        sceneLoading = SceneManager.LoadSceneAsync("StartScene");
        // StartCoroutine
        StartCoroutine(LoadingBar());
    }
    IEnumerator LoadingBar()
    {
        while (!sceneLoading.isDone)
        {
            progressBar.fillAmount = sceneLoading.progress;
            progressText.text = sceneLoading.progress * 100 + " %";
            yield return null;
        }
    }
}
