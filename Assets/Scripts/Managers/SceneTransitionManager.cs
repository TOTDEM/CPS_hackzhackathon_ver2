// ファイル名: SceneTransitionManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"シーン '{sceneName}' へ遷移します．");
        SceneManager.LoadScene(sceneName);
    }

    public void LoadBattleScene()
    {
        LoadScene("Battle");
    }

    public void LoadStartScene()
    {
        LoadScene("Start");
    }

    /// <summary>
    /// 失敗リザルトシーンへ遷移します．
    /// </summary>
    public void LoadFailureResultScene()
    {
        LoadScene("FailureResultScene");
    }

    /// <summary>
    /// 成功リザルトシーンへ遷移します．
    /// </summary>
    public void LoadSuccessResultScene()
    {
        LoadScene("SuccessResultScene");
    }
}