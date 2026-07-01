using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadCoroutine(sceneName));
    }

    private IEnumerator LoadCoroutine(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(SceneManager.GetActiveScene().name);
    }
}